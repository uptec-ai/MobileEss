using Modbus.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
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
    public class ModbusService 
    {
        private TcpClient _client;
        private IModbusMaster _master;
        public bool IsConnected => _client?.Connected ?? false;
        public event Action<bool> ConnectionStateChanged;
        public event EventHandler<HoldingRegistersEventArgs> KeepAliveHoldingReceived;

        public async Task ConnectAsync(string host, int port, int timeoutMs, CancellationToken ct)
        {
            Dispose(); // 기존 연결 정리

            _client = new TcpClient();
            _client.ReceiveTimeout = timeoutMs;
            _client.SendTimeout = timeoutMs;

            // TcpClient.ConnectAsync는 .NET Framework에서도 사용 가능
            using (ct.Register(() => { try { _client.Close(); } catch { } }))
            {
                await _client.ConnectAsync(host, port).ConfigureAwait(false);
            }

            _master = ModbusIpMaster.CreateIp(_client);
            // 필요 시 타임아웃/재시도는 라이브러리/소켓 옵션으로 조절
        }

        // slaveId는 "RTU 장치 주소(UnitId)"에 해당 (게이트웨이가 RTU로 전달)
        public Task<ushort[]> ReadInputRegistersAsync(byte slaveId, ushort startAddress, ushort count, CancellationToken ct)
        {
            if (_master == null) throw new InvalidOperationException("Not connected.");

            // NModbus4가 동기 중심이므로, UI thread block 방지용으로 오프로딩

            return Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();
                return _master.ReadInputRegistersAsync(slaveId, startAddress, count);
            }, ct);
        }

        public async Task WriteSingleRegisterAsync(ushort address, ushort value)
        {
            await _master.WriteSingleRegisterAsync(1, address, value);
        }

        public void Dispose()
        {
            try { _client?.Close(); } catch { }
            _client = null;
            _master = null;
        }
    }
}
