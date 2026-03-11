using Peak.Can.Basic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace EMS_PJT_Hamburger.Models.Client.BMS
{
    public class PcanRxService : IDisposable
    {
        private readonly PcanChannel _channel;
        private readonly Bitrate _bitrate;
        private Thread _rxThread;
        public event Action<uint, byte[]> FrameReceived; // (canId, data)
        public bool IsBmsReady { get; set; }

        public PcanRxService(PcanChannel channel, Bitrate bitrate)
        {
            _channel = channel;
            _bitrate = bitrate;
        }

        public bool Start()
        {
            try
            {
                var st = Api.Initialize(_channel, _bitrate);
                if (st != PcanStatus.OK)
                    return false;

                IsBmsReady = true;
                _rxThread = new Thread(ReadLoop);
                _rxThread.IsBackground = true;
                _rxThread.Start();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private void ReadLoop()
        {
            while (IsBmsReady)
            {
                PcanMessage msg;
                ulong ts;

                var st = Api.Read(_channel, out msg, out ts);
                if (st == PcanStatus.OK)
                {
                    if (msg.ID >= 0x150 && msg.ID <= 0x164)
                    {
                        var data = new byte[msg.Length];
                        for (int i = 0; i < msg.Length; i++)
                            data[i] = msg.Data[i];

                        FrameReceived?.Invoke(msg.ID, data);
                    }
                }
                else if (st != PcanStatus.InvalidValue)
                {
                    // 필요 시 에러 로깅
                }

            }
        }
        public void Stop()
        {
            IsBmsReady = false;
            _rxThread?.Join();
            Api.Uninitialize(_channel);
        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
