using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using EasyModbus;
using System.Net.Sockets;

namespace EMS_PJT_Hamburger.Models.Client
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

    public sealed class ModbusTcpConnectionService : IDisposable
    {
        /// <summary> 연결 상태 변경 이벤트(true=Connected, false=Disconnected) </summary>
        public event Action<bool> ConnectionStateChanged;

        /// <summary> 상태 메시지 로그용(선택). 구독 시 서비스 내부 상태 로그를 받을 수 있음. </summary>
        public event Action<string> Log;

        public event EventHandler<HoldingRegistersEventArgs> KeepAliveHoldingReceived;

        // keep-alive 읽기 주소/개수 및 전달 여부 설정값
        int _kaStart = 0;
        int _kaCount = 1;
        bool _kaForward = false; // 기본: 끊김 감지만, 데이터는 올리지 않음

        // UI 스레드로 이벤트 마샬링용 컨텍스트 (Start를 UI 스레드에서 호출하세요)
        SynchronizationContext _ctx;

        /// <summary> 현재 연결 여부 </summary>
        public bool IsConnected => _client?.Connected ?? false;

        // ---- 구성 값 ----
        string _host;
        int _port;
        int _unitId = 1;
        int _socketTimeoutMs = 5000;

        // keepalive/poll 및 재접속 백오프
        // Backoff : 재 접속할 때 일정 시간 쉬었다가 시도
        // Exponential Backoff : 지수 증가 방식, 재시도할 때마다 대기 시간을 늘려가는 방식.
        TimeSpan _keepAliveInterval = TimeSpan.FromSeconds(10); // 응답이 없어도 10초간 지속
        TimeSpan _minBackoff = TimeSpan.FromSeconds(1);         // 재접속 시 최소 대기시간
        TimeSpan _maxBackoff = TimeSpan.FromSeconds(15);        // 재접속 시 최대 대기시간

        // ---- 런타임 ----
        CancellationTokenSource _cts;
        Task _loopTask;
        ModbusClient _client;
        readonly object _sync = new object();
        bool _disposed;

        public void ConfigureKeepAliveRead(int startAddress, int count, bool forwardData = true)
        {
            if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
            _kaStart = startAddress;
            _kaCount = count;
            _kaForward = forwardData;
        }

        /// <summary>
        /// 접속 파라미터 구성. Start 전 호출.
        /// </summary>
        public void Configure(
            string host,
            int port,
            int unitId = 1,
            int socketTimeoutMs = 5000,
            TimeSpan? keepAliveInterval = null,
            TimeSpan? minBackoff = null, 
            TimeSpan? maxBackoff = null) 
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
            _port = port;
            _unitId = unitId;
            _socketTimeoutMs = socketTimeoutMs;

            if (keepAliveInterval != null) _keepAliveInterval = keepAliveInterval.Value;
            if (minBackoff != null) _minBackoff = minBackoff.Value;
            if (maxBackoff != null) _maxBackoff = maxBackoff.Value;
        }

        /// <summary>
        /// 백그라운드 접속/재접속 루프 시작(중복 시작 방지).
        /// </summary>
        public async Task Start()
        {
            // 중복 시작 방지
            ThrowIfDisposed();
            if (_loopTask != null) return;

            // async start
            _ctx = SynchronizationContext.Current; // UI 스레드에서 호출 시 UI 컨텍스트 캡처
            _cts = new CancellationTokenSource();
            _loopTask = Task.Run(() => RunLoopAsync(_cts.Token));
            LogSafe("Modbus loop started.");
        }

        /// <summary>
        /// 루프 중지 및 소켓 정리.
        /// </summary>
        public async Task StopAsync()
        {
            if (_loopTask == null) return;

            _cts.Cancel();
            try { await _loopTask.ConfigureAwait(false); }
            catch { /* ignore */ }

            _loopTask = null;
            _cts.Dispose();
            _cts = null;

            SafeCloseClient();
            RaiseConnection(false);
            LogSafe("Modbus loop stopped.");
        }

        /// <summary>
        /// 홀딩 레지스터 읽기 (동기 API를 Task로 래핑).
        /// </summary>
        public Task<int[]> ReadHoldingAsync(int start, int count, CancellationToken ct)
            => Task.Run(() =>
            {
                ThrowIfDisposed();
                EnsureConnectedOrThrow();
                return _client.ReadHoldingRegisters(start, count);
            }, ct);

        /// <summary>
        /// 레지스터 여러 개 쓰기.
        /// </summary>
        public Task WriteMultipleAsync(int start, int[] values, CancellationToken ct)
            => Task.Run(() =>
            {
                ThrowIfDisposed();
                EnsureConnectedOrThrow();
                _client.WriteMultipleRegisters(start, values);
            }, ct);

        /// <summary>
        /// Coil 읽기.
        /// </summary>
        public Task<bool[]> ReadCoilsAsync(int start, int count, CancellationToken ct)
            => Task.Run(() =>
            {
                ThrowIfDisposed();
                EnsureConnectedOrThrow();
                return _client.ReadCoils(start, count);
            }, ct);

        /// <summary>
        /// Coil 쓰기.
        /// </summary>
        public Task WriteCoilsAsync(int start, bool[] values, CancellationToken ct)
            => Task.Run(() =>
            {
                ThrowIfDisposed();
                EnsureConnectedOrThrow();
                _client.WriteMultipleCoils(start, values);
            }, ct);

        public Task<int[]> ReadInputAsync(int start, int count, CancellationToken ct)
            => Task.Run(() =>
            {
                //ThrowIfDisposed();
                //EnsureConnectedOrThrow();
                return _client.ReadInputRegisters(start, count);
            }, ct);

        /*
        public Task<int[]> ReadInputAsync(int start, int count, CancellationToken ct)
    => Task.Run(() =>
    {
        // 내부 보호 메서드 이름은 예시: ThrowIfDisposed(), EnsureConnectedOrThrow()
        // 기존 클래스에 이미 있다면 그대로 사용
        // 없으면 동일 패턴으로 구현
        // (예시 코드: 기존 클래스에 있는 메서드명과 일치하도록 조정)
        // ThrowIfDisposed();
        // EnsureConnectedOrThrow();
        return _client.ReadInputRegisters(start, count);
    }, ct);
        */

        // ===================== 내부 로직 =====================

        async Task RunLoopAsync(CancellationToken ct)
        {
            var backoff = _minBackoff;

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    EnsureClientCreated();

                    // 동기 Connect (EasyModbus 제약) — 백그라운드 스레드에서 수행
                    _client.Connect(_host, _port);
                    _client.UnitIdentifier = (byte)_unitId;
                    _client.ConnectionTimeout = _socketTimeoutMs;

                    RaiseConnection(true);
                    LogSafe($"Connected to {_host}:{_port} (UnitId={_unitId})");
                    backoff = _minBackoff;

                    // 연결 유지 루프(keepalive)
                    while (!ct.IsCancellationRequested && _client.Connected)
                    {
                        await Task.Delay(_keepAliveInterval, ct).ConfigureAwait(false);

                        // 경량 keepalive: 실제 설비에 존재하는 주소로 변경 권장
                        // 실패(예외) 시 catch로 빠져 재접속 수행
                        var vals = _client.ReadHoldingRegisters(_kaStart, _kaCount);

                        if (_kaForward) // 필요한 경우에만 값 올림
                            RaiseKeepAliveHolding(_kaStart, vals);
                    }

                    throw new SocketException((int)SocketError.NotConnected);
                }
                catch (OperationCanceledException)
                {
                    break; // 정상 종료
                }
                catch (Exception ex)
                {
                    RaiseConnection(false);
                    LogSafe($"Disconnected: {ex.Message}");
                    SafeCloseClient();

                    // 지수 백오프
                    try { await Task.Delay(backoff, ct).ConfigureAwait(false); }
                    catch (OperationCanceledException) { break; }

                    var nextMs = Math.Min(backoff.TotalMilliseconds * 2, _maxBackoff.TotalMilliseconds);
                    backoff = TimeSpan.FromMilliseconds(nextMs);
                }
            }
        }

        void RaiseKeepAliveHolding(int start, int[] values)
        {
            var args = new HoldingRegistersEventArgs(start, values, DateTime.UtcNow);

            void fire()
            {
                try { KeepAliveHoldingReceived?.Invoke(this, args); } catch { }
            }

            // UI 컨텍스트가 있으면 그쪽으로 마샬링
            if (_ctx != null) _ctx.Post(_ => fire(), null);
            else fire();
        }

        void EnsureClientCreated()
        {
            if (_client != null) return;

            lock (_sync)
            {
                if (_client != null) return;
                _client = new ModbusClient();
                // 필요 시 여기서 파라미터 추가 설정
                // _client.SocketTimeout = _socketTimeoutMs; // (버전에 따라 존재)
                LogSafe("ModbusClient created.");
            }
        }

        void EnsureConnectedOrThrow()
        {
            if (!IsConnected)
                throw new InvalidOperationException("Modbus is not connected.");
        }

        void SafeCloseClient()
        {
            lock (_sync)
            {
                try
                {
                    if (_client?.Connected == true)
                        _client.Disconnect();
                }
                catch { /* ignore */ }
                finally
                {
                    _client = null;
                    LogSafe("ModbusClient disposed.");
                }
            }
        }

        void RaiseConnection(bool state)
        {
            try { ConnectionStateChanged?.Invoke(state); }
            catch { /* listener side errors ignored */ }
        }

        void LogSafe(string msg)
        {
            try { Log?.Invoke(msg); }
            catch { /* listener side errors ignored */ }
        }

        void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ModbusTcpConnectionService));
        }

        // ===================== IDisposable =====================
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            try { _ = StopAsync(); }
            catch { /* ignore */ }
        }
    }
}
