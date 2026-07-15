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
    /// <summary>PCAN 시작 결과 — 실패를 회복 가능 여부로 구분한다.</summary>
    public enum PcanStartResult
    {
        Success,
        /// <summary>어댑터 미장착·채널 사용 불가 등 — 재시도로 회복 가능</summary>
        TransientFailure,
        /// <summary>PCANBasic 네이티브 dll 부재/불일치 — 프로세스 생존 중 재시도로 회복 불가</summary>
        PermanentFailure,
    }

    public class PcanRxService : IDisposable
    {
        private readonly PcanChannel _channel;
        private readonly Bitrate _bitrate;
        private Thread _rxThread; // read용 루프 스레드
        private bool _disposed;
        public event Action<uint, byte[]> FrameReceived; // (canId, data)
        public event Action<bool> ConnectionStateChanged;
        public bool IsBmsReady { get; set; }

        public PcanRxService(PcanChannel channel, Bitrate bitrate)
        {
            _channel = channel;
            _bitrate = bitrate;
        }

        public PcanStartResult Start()
        {
            ThrowIfDisposed();

            try
            {
                var st = Api.Initialize(_channel, _bitrate);
                if (st != PcanStatus.OK)
                {
                    RaiseConnection(false);
                    return PcanStartResult.TransientFailure;
                }

                IsBmsReady = true;
                _rxThread = new Thread(ReadLoop);
                _rxThread.IsBackground = true;
                _rxThread.Start();

                RaiseConnection(true);
                return PcanStartResult.Success;
            }
            catch (Exception ex) when (IsPermanentFailure(ex))
            {
                RaiseConnection(false);
                return PcanStartResult.PermanentFailure;
            }
            catch (Exception)
            {
                RaiseConnection(false);
                return PcanStartResult.TransientFailure;
            }
        }

        // PCANBasic.dll 자체가 없거나(x86/x64 불일치 포함) 로드 불가한 경우 —
        // TypeInitializationException은 원인을 감싸므로 InnerException까지 검사한다.
        private static bool IsPermanentFailure(Exception ex)
        {
            for (var e = ex; e != null; e = e.InnerException)
            {
                if (e is DllNotFoundException ||
                    e is BadImageFormatException ||
                    e is EntryPointNotFoundException ||
                    e is TypeInitializationException)
                    return true;
            }
            return false;
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

            if (_rxThread != null && _rxThread.IsAlive)
            {
                // 무한 대기로 종료가 막히지 않도록 제한 시간을 둔다.
                _rxThread.Join(1000);
            }

            // PCAN 드라이버 해제
            Api.Uninitialize(_channel);
            _rxThread = null;
            RaiseConnection(false);
        }

        private void RaiseConnection(bool connected)
        {
            try { ConnectionStateChanged?.Invoke(connected); }
            catch { }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(PcanRxService));
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            try { Stop(); } catch { }
            FrameReceived = null; // 구독자 참조를 해제해 누수 위험을 줄이기 위해서
            ConnectionStateChanged = null;
            GC.SuppressFinalize(this);
        }
    }
}
