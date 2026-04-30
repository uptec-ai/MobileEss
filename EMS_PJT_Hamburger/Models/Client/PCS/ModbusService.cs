using Modbus.Device;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace EMS_PJT_Hamburger.Models.Client.PCS
{
    public sealed class HoldingRegistersEventArgs : EventArgs
    {
        public int StartAddress { get; }
        public int[] Values { get; }
        public DateTime Timestamp { get; }

        public HoldingRegistersEventArgs(int startAddress, int[] values, DateTime timestamp)
        {
            StartAddress = startAddress;
            Values = values ?? Array.Empty<int>();
            Timestamp = timestamp;
        }
    }

    public sealed class InputRegistersEventArgs : EventArgs
    {
        public int StartAddress { get; }
        public ushort[] Values { get; }
        public DateTime Timestamp { get; }

        public InputRegistersEventArgs(int startAddress, ushort[] values, DateTime timestamp)
        {
            StartAddress = startAddress;
            Values = values ?? Array.Empty<ushort>();
            Timestamp = timestamp;
        }
    }

    public class ModbusService : IDisposable
    {
        private readonly object _sync = new object();
        private TcpClient _client;
        private IModbusMaster _master;
        private CancellationTokenSource _loopCts;
        private Task _loopTask;

        private bool _disposed;
        private bool _configured;
        private const ushort MaxRegistersPerRequest = 125; // Modbus FC03/FC04 단건 최대 읽기 개수

        private string _host;
        private int _port;
        private byte _slaveId = 1;
        private int _timeoutMs = 10000;
        private ushort _pollStartAddress;
        private ushort _pollCount = 1;
        private TimeSpan _pollInterval = TimeSpan.FromMilliseconds(500);
        private ushort _keepAliveStartAddress = 256; // pcs heartbeat 주소
        private ushort _keepAliveCount = 1;
        private TimeSpan _keepAliveInterval = TimeSpan.FromSeconds(1);
        private TimeSpan _minBackoff = TimeSpan.FromSeconds(1);
        private TimeSpan _maxBackoff = TimeSpan.FromSeconds(15);

        public bool IsConnected => _client?.Connected ?? false;

        public event Action<bool> ConnectionStateChanged;
        public event EventHandler<HoldingRegistersEventArgs> KeepAliveHoldingReceived;
        public event EventHandler<InputRegistersEventArgs> InputRegistersReceived;
        public event Action<string> Log;

        // 연결/폴링 설정은 Configure 단계에서 명시적으로 받습니다.
        public void Configure(
            string host,
            int port,
            byte slaveId = 1,
            int timeoutMs = 10000,
            ushort pollStartAddress = 0,
            ushort pollCount = 1,
            TimeSpan? pollInterval = null,
            ushort keepAliveStartAddress = 256,
            ushort keepAliveCount = 1,
            TimeSpan? keepAliveInterval = null,
            TimeSpan? minBackoff = null,
            TimeSpan? maxBackoff = null)
        {
            ThrowIfDisposed();

            if (string.IsNullOrWhiteSpace(host)) throw new ArgumentNullException(nameof(host));
            if (port <= 0 || port > 65535) throw new ArgumentOutOfRangeException(nameof(port));
            if (timeoutMs <= 0) throw new ArgumentOutOfRangeException(nameof(timeoutMs));
            if (pollCount <= 0) throw new ArgumentOutOfRangeException(nameof(pollCount));
            if (keepAliveCount <= 0) throw new ArgumentOutOfRangeException(nameof(keepAliveCount));

            _host = host;
            _port = port;
            _slaveId = slaveId;
            _timeoutMs = timeoutMs;
            _pollStartAddress = pollStartAddress;
            _pollCount = pollCount;
            _keepAliveStartAddress = keepAliveStartAddress;
            _keepAliveCount = keepAliveCount;

            if (pollInterval.HasValue)
            {
                if (pollInterval.Value <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(pollInterval));
                _pollInterval = pollInterval.Value;
            }

            if (keepAliveInterval.HasValue)
            {
                if (keepAliveInterval.Value <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(keepAliveInterval));
                _keepAliveInterval = keepAliveInterval.Value;
            }

            if (minBackoff.HasValue)
            {
                if (minBackoff.Value <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(minBackoff));
                _minBackoff = minBackoff.Value;
            }

            if (maxBackoff.HasValue)
            {
                if (maxBackoff.Value <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(maxBackoff));
                _maxBackoff = maxBackoff.Value;
            }

            if (_maxBackoff < _minBackoff)
            {
                _maxBackoff = _minBackoff;
            }

            _configured = true;
        }

        // Configure -> StartAsync -> RunLoopAsync 흐름으로 상시 연결/재접속을 관리합니다.
        private readonly SemaphoreSlim _startStopGate = new SemaphoreSlim(1, 1);
        public async Task StartAsync(CancellationToken ct = default)
        {
            ThrowIfDisposed();
            if (!_configured)
                throw new InvalidOperationException("Call Configure(...) before StartAsync().");

            await _startStopGate.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                // 이미 실행 중이면 중복 시작 차단
                if (_loopTask != null && !_loopTask.IsCompleted)
                    return;

                _loopCts?.Dispose();
                _loopCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                _loopTask = Task.Run(() => RunLoopAsync(_loopCts.Token));

                LogSafe("Modbus loop started.");
            }
            finally
            {
                _startStopGate.Release();
            }
        }

        public async Task StopAsync()
        {
            await _startStopGate.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_loopTask == null)
                {
                    CloseConnection();
                    RaiseConnection(false);
                    return;
                }

                _loopCts.Cancel();
                CloseConnection(); // pending read 즉시 해제

                try { await _loopTask.ConfigureAwait(false); }
                catch (OperationCanceledException) { }
                catch (Exception ex) { LogSafe($"Modbus loop stop error: {ex.Message}"); }
                finally
                {
                    _loopTask = null;
                    _loopCts.Dispose();
                    _loopCts = null;

                    CloseConnection();
                    RaiseConnection(false);
                    LogSafe("Modbus loop stopped.");
                }
            }
            finally
            {
                _startStopGate.Release();
            }
        }

        // 기존 호출부 호환용: 1회 연결 메서드
        public async Task ConnectAsync(string host, int port, int timeoutMs, CancellationToken ct)
        {
            ThrowIfDisposed();
            Configure(
                host,
                port,
                _slaveId,
                timeoutMs,
                _pollStartAddress,
                _pollCount,
                _pollInterval,
                _keepAliveStartAddress,
                _keepAliveCount,
                _keepAliveInterval,
                _minBackoff,
                _maxBackoff);
            await EnsureConnectedAsync(ct).ConfigureAwait(false);
            RaiseConnection(true);
        }

        // slaveId는 RTU 장치 주소(UnitId)
        public async Task<ushort[]> ReadInputRegistersAsync(byte slaveId, ushort startAddress, ushort count, CancellationToken ct)
        {
            ThrowIfDisposed();

            var master = _master;
            if (master == null || !IsConnected) throw new InvalidOperationException("Not connected.");
            if (count == 0) throw new ArgumentOutOfRangeException(nameof(count));
            if ((uint)startAddress + count - 1u > ushort.MaxValue) throw new ArgumentOutOfRangeException(nameof(count));

            ct.ThrowIfCancellationRequested();
            if (count <= MaxRegistersPerRequest)
            {
                var data = await master.ReadInputRegistersAsync(slaveId, startAddress, count).ConfigureAwait(false);

                // 응답 길이 검증으로 데이터 신뢰성 강화
                if (data == null || data.Length != count)
                    throw new InvalidOperationException($"Invalid register length. expected={count}, actual={data?.Length ?? 0}");

                return data;
            }

            // 125개 초과 요청은 장비 제약에 맞춰 분할 읽기 후 합칩니다.
            var merged = new ushort[count];
            var copied = 0;
            var remaining = (int)count;
            var currentAddress = (int)startAddress;

            while (remaining > 0)
            {
                ct.ThrowIfCancellationRequested();

                var take = (ushort)Math.Min(remaining, MaxRegistersPerRequest);
                var chunk = await master.ReadInputRegistersAsync(slaveId, (ushort)currentAddress, take).ConfigureAwait(false);
                if (chunk == null || chunk.Length != take)
                    throw new InvalidOperationException($"Invalid register chunk length. expected={take}, actual={chunk?.Length ?? 0}, start={currentAddress}");

                Array.Copy(chunk, 0, merged, copied, chunk.Length);
                copied += chunk.Length;
                remaining -= take;
                currentAddress += take;
            }

            return merged;
        }

        public async Task<ushort[]> ReadHoldingRegistersAsync(byte slaveId, ushort startAddress, ushort count, CancellationToken ct)
        {
            ThrowIfDisposed();

            var master = _master;
            if (master == null || !IsConnected) throw new InvalidOperationException("Not connected.");
            if (count == 0) throw new ArgumentOutOfRangeException(nameof(count));
            if ((uint)startAddress + count - 1u > ushort.MaxValue) throw new ArgumentOutOfRangeException(nameof(count));

            ct.ThrowIfCancellationRequested();
            if (count <= MaxRegistersPerRequest)
            {
                var data = await master.ReadHoldingRegistersAsync(slaveId, startAddress, count).ConfigureAwait(false);

                // 응답 길이 검증으로 keep-alive 신뢰성을 보강합니다.
                if (data == null || data.Length != count)
                    throw new InvalidOperationException($"Invalid holding register length. expected={count}, actual={data?.Length ?? 0}");

                return data;
            }

            // 125개 초과 요청은 장비 제약에 맞춰 분할 읽기 후 합칩니다.
            var merged = new ushort[count];
            var copied = 0;
            var remaining = (int)count;
            var currentAddress = (int)startAddress;

            while (remaining > 0)
            {
                ct.ThrowIfCancellationRequested();

                var take = (ushort)Math.Min(remaining, MaxRegistersPerRequest);
                var chunk = await master.ReadHoldingRegistersAsync(slaveId, (ushort)currentAddress, take).ConfigureAwait(false);
                if (chunk == null || chunk.Length != take)
                    throw new InvalidOperationException($"Invalid holding register chunk length. expected={take}, actual={chunk?.Length ?? 0}, start={currentAddress}");

                Array.Copy(chunk, 0, merged, copied, chunk.Length);
                copied += chunk.Length;
                remaining -= take;
                currentAddress += take;
            }

            return merged;
        }

        public async Task WriteSingleRegisterAsync(ushort address, ushort value)
        {
            ThrowIfDisposed();

            var master = _master;
            if (master == null || !IsConnected) throw new InvalidOperationException("Not connected.");

            await master.WriteSingleRegisterAsync(_slaveId, address, value).ConfigureAwait(false);
        }

        public async Task WriteMultipleRegistersAsync(ushort startAddress, ushort[] values)
        {
            ThrowIfDisposed();

            if (values == null || values.Length == 0) throw new ArgumentNullException(nameof(values));

            var master = _master;
            if (master == null || !IsConnected) throw new InvalidOperationException("Not connected.");

            await master.WriteMultipleRegistersAsync(_slaveId, startAddress, values).ConfigureAwait(false);
        }

        private async Task RunLoopAsync(CancellationToken ct)
        {
            var backoff = _minBackoff; // 재접속 대기시간
            var nextKeepAliveAt = DateTime.UtcNow; // keep alive 시각

            while (!ct.IsCancellationRequested) // 연결 유지, 재접속 루프
            {
                try
                {
                    await EnsureConnectedAsync(ct).ConfigureAwait(false); // 연결 + UI스레드로 복귀 X
                    RaiseConnection(true); // 연결상태 true
                    backoff = _minBackoff;

                    while (!ct.IsCancellationRequested) // Alive 확인 루프
                    {
                        var values = await ReadHoldingRegistersAsync(_slaveId, _pollStartAddress, _pollCount, ct).ConfigureAwait(false); // Alive확인 + UI스레드로 복귀 X
                        RaiseInputRegisters(_pollStartAddress, values); // 결과 이벤트

                        //var now = DateTime.UtcNow;
                        //if (now >= nextKeepAliveAt)
                        //{
                        //    var keepAliveValues = await ReadHoldingRegistersAsync(_slaveId, _keepAliveStartAddress, _keepAliveCount, ct).ConfigureAwait(false); // Alive 경량 요청 (heartbeat)
                        //    RaiseKeepAliveHolding(_keepAliveStartAddress, keepAliveValues);
                        //    nextKeepAliveAt = now + _keepAliveInterval; // now + 1s
                        //}

                        await Task.Delay(_pollInterval, ct).ConfigureAwait(false); // 500ms delay
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    RaiseConnection(false);
                    LogSafe($"Modbus disconnected: {ex.Message}");
                    CloseConnection();

                    try { await Task.Delay(backoff, ct).ConfigureAwait(false); }
                    catch (OperationCanceledException) { break; }

                    var nextMs = Math.Min(backoff.TotalMilliseconds * 2, _maxBackoff.TotalMilliseconds);
                    backoff = TimeSpan.FromMilliseconds(nextMs);
                }
            }

            CloseConnection();
            RaiseConnection(false);
        }

        private async Task EnsureConnectedAsync(CancellationToken ct)
        {
            if (IsConnected && _master != null)
                return;

            CloseConnection();

            var client = new TcpClient
            {
                ReceiveTimeout = _timeoutMs,
                SendTimeout = _timeoutMs
            };

            // 취소 시 소켓을 닫아 ConnectAsync 대기를 해제합니다.
            using (ct.Register(() => { try { client.Close(); } catch { } }))
            {
                await client.ConnectAsync(_host, _port).ConfigureAwait(false);
            }

            lock (_sync)
            {
                _client = client;
                _master = ModbusIpMaster.CreateIp(_client);
            }

            LogSafe($"Connected to {_host}:{_port}");
        }

        private void RaiseInputRegisters(ushort startAddress, ushort[] values)
        {
            if (values == null) return;

            var timestamp = DateTime.UtcNow;
            var safeValues = (ushort[])values.Clone();

            try { InputRegistersReceived?.Invoke(this, new InputRegistersEventArgs(startAddress, safeValues, timestamp)); }
            catch(Exception ex)
            {
                var a = ex.Message;
            }
        }

        private void RaiseKeepAliveHolding(ushort startAddress, ushort[] values)
        {
            if (values == null) return;

            var timestamp = DateTime.UtcNow;
            var intValues = Array.ConvertAll(values, v => (int)v);

            try { KeepAliveHoldingReceived?.Invoke(this, new HoldingRegistersEventArgs(startAddress, intValues, timestamp)); }
            catch { }
        }

        private void RaiseConnection(bool connected)
        {
            try { ConnectionStateChanged?.Invoke(connected); }
            catch { }
        }

        private void LogSafe(string message)
        {
            try { Log?.Invoke(message); }
            catch { }
        }

        private void CloseConnection()
        {
            lock (_sync)
            {
                try { _client?.Close(); } catch { }
                _master = null;
                _client = null;
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ModbusService));
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            try { StopAsync().GetAwaiter().GetResult(); }
            catch { }

            _startStopGate.Dispose();

            ConnectionStateChanged = null;
            KeepAliveHoldingReceived = null;
            InputRegistersReceived = null;
            Log = null;
        }
    }
}
