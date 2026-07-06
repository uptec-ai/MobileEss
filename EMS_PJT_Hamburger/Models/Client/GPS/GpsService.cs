using System;
using System.IO.Ports;
using System.Text;

namespace EMS_PJT_Hamburger.Models.Client.GPS
{
    /// <summary>
    /// GlobalSat BU-353N GPS ?μ튂????쒕━???듭떊???대떦?섎뒗 ?쒕퉬??
    /// - DataReceived ?대깽?몃? ?듯빐 ?섏떊 ?곗씠?곕? ?쇱씤 ?⑥쐞濡?踰꾪띁留?
    /// - NMEA 泥댄겕??寃利????좏슚??臾몄옣留?SentenceReceived ?대깽?몃줈 ?꾨떖
    /// </summary>
    public class GpsService : IDisposable
    {
        private SerialPort _serialPort;
        private string _buffer = string.Empty;
        private bool _disposed = false;

        // ??? ?대깽?????????????????????????????????????????????????????????
        /// <summary>泥댄겕?ъ씠 ?좏슚??NMEA 臾몄옣 ?섏떊 ??諛쒖깮 (諛깃렇?쇱슫???ㅻ젅??</summary>
        public event Action<string> SentenceReceived;

        /// <summary>$ 濡??쒖옉?섎뒗 紐⑤뱺 ?쇱씤 ?섏떊 ??諛쒖깮. isValid = 泥댄겕???듦낵 ?щ?</summary>
        public event Action<string, bool> RawSentenceReceived;

        /// <summary>?쒕━???ы듃 ?ㅻ쪟 諛쒖깮 ??(諛깃렇?쇱슫???ㅻ젅??</summary>
        public event Action<Exception> ErrorOccurred;

        // ??? ?곹깭 ?????????????????????????????????????????????????????????
        /// <summary>?꾩옱 ?ы듃媛 ?대젮 ?덈뒗吏 ?щ?</summary>
        public bool IsConnected => _serialPort?.IsOpen ?? false;

        // ??? ?곌껐 / ?댁젣 ??????????????????????????????????????????????????
        /// <summary>
        /// 吏?뺣맂 COM ?ы듃濡?GPS ?μ튂???곌껐?⑸땲??
        /// BU-353N 확인 설정: 4800 bps, 8N1
        /// </summary>
        public void Connect(string portName, int baudRate)
        {
            if (_serialPort?.IsOpen == true)
                throw new InvalidOperationException("?대? ?곌껐?섏뼱 ?덉뒿?덈떎.");

            _serialPort = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One)
            {
                Encoding   = Encoding.ASCII,
                ReadTimeout  = 3000,
                WriteTimeout = 500,
                Handshake  = Handshake.None,
                RtsEnable  = false,
                DtrEnable  = false
            };

            _serialPort.DataReceived  += OnDataReceived;
            _serialPort.ErrorReceived += OnErrorReceived;

            _buffer = string.Empty;
            _serialPort.Open();
        }

        /// <summary>GPS ?μ튂 ?곌껐???댁젣?⑸땲??</summary>
        public void Disconnect()
        {
            if (_serialPort == null) return;

            _serialPort.DataReceived  -= OnDataReceived;
            _serialPort.ErrorReceived -= OnErrorReceived;

            if (_serialPort.IsOpen)
            {
                try { _serialPort.Close(); }
                catch { /* ?대? ?ロ삍嫄곕굹 ?ㅻ쪟 臾댁떆 */ }
            }

            _buffer = string.Empty;
        }

        // ??? ?섏떊 泥섎━ ????????????????????????????????????????????????????
        /// <summary>
        /// DataReceived ?대깽???몃뱾??(諛깃렇?쇱슫???ㅻ젅??
        /// ?섏떊 ?곗씠?곕? 踰꾪띁???꾩쟻?섍퀬 '\n' 湲곗??쇰줈 ?쇱씤 ?⑥쐞 遺꾨━ 泥섎━
        /// </summary>
        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string incoming = _serialPort.ReadExisting();
                _buffer += incoming;

                int idx;
                while ((idx = _buffer.IndexOfAny(new[] { '\n', '\r' })) >= 0)
                {
                    string line = _buffer.Substring(0, idx).Trim('\r', '\n');
                    _buffer = _buffer.Substring(idx + 1);

                    if (string.IsNullOrEmpty(line) || !line.StartsWith("$")) continue;

                    bool valid = ValidateChecksum(line);
                    RawSentenceReceived?.Invoke(line, valid);
                    if (valid)
                        SentenceReceived?.Invoke(line);
                }

                // 踰꾪띁 ?ㅻ쾭?뚮줈??諛⑹? (4KB 珥덇낵 ??珥덇린??
                if (_buffer.Length > 4096)
                    _buffer = string.Empty;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(ex);
            }
        }

        private void OnErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            ErrorOccurred?.Invoke(new Exception($"?쒕━???ы듃 ?섎뱶?⑥뼱 ?ㅻ쪟: {e.EventType}"));
        }

        // ??? NMEA 泥댄겕??寃利??????????????????????????????????????????????
        /// <summary>
        /// NMEA 泥댄겕??寃利? '$'? '*' ?ъ씠 紐⑤뱺 臾몄옄??XOR 寃곌낵瑜?鍮꾧탳
        /// 泥댄겕?ъ씠 ?녿뒗 臾몄옣? ?듦낵(true)濡?泥섎━
        /// </summary>
        private static bool ValidateChecksum(string sentence)
        {
            int starIdx = sentence.LastIndexOf('*');

            // 泥댄겕???녿뒗 臾몄옣? ?듦낵
            if (starIdx < 0) return true;

            // 泥댄겕???뚯떛 遺덇? ??嫄곕?
            if (starIdx + 2 >= sentence.Length) return false;
            if (!int.TryParse(sentence.Substring(starIdx + 1, 2),
                System.Globalization.NumberStyles.HexNumber, null, out int expected))
                return false;

            // '$'(?몃뜳??0) ?쒖쇅, '*' 吏곸쟾源뚯? XOR
            int calculated = 0;
            for (int i = 1; i < starIdx; i++)
                calculated ^= sentence[i];

            return calculated == expected;
        }

        // ??? IDisposable ??????????????????????????????????????????????????
        public void Dispose()
        {
            if (!_disposed)
            {
                Disconnect();
                _serialPort?.Dispose();
                _disposed = true;
            }
        }
    }
}

