using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace EMS_PJT_Hamburger.Models.Client.PCS
{
    public class PcsModel : ViewModelBase
    {
        public CancellationTokenSource _cts;
        public ModbusService _client = new ModbusService();
        public ConnectionSettings Conn_Settings { get; set; }
        public ConnectionState Conn_State { get; set; }
        public ObservableCollection<RegisterItem> KeepAliveRegisters { get; } = new ObservableCollection<RegisterItem>();

        // 상태
        protected IDispatcherService Dispatcher => GetService<IDispatcherService>();
        public bool IsConnected { get => GetProperty(() => IsConnected); set => SetProperty(() => IsConnected, value); } // Server connected
        public bool IsReceive { get => GetProperty(() => IsReceive); set => SetProperty(() => IsReceive, value); } // RX(수신)
        public bool IsTransmit { get => GetProperty(() => IsTransmit); set => SetProperty(() => IsTransmit, value); } // TX(송신)
        public bool IsWrite { get => GetProperty(() => IsWrite); set => SetProperty(() => IsWrite, value); } // Send Write 동작?
        public bool IsRelay { get => GetProperty(() => IsRelay); set => SetProperty(() => IsRelay, value); } // BMS Relay 동작?
        public string SystemMsg { get; set; } = string.Empty; // [Log] system msg

        #region [ Function ]

        public async Task UpdateAsync()
        {
            var data = await _client.ReadInputRegistersAsync(1, 0, 355, _cts.Token);

            var parsed = ModbusParser.ParseRegisters(data, PcsSpecs.All);
        }
        public async Task StartPolling()
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                await UpdateAsync();
                await Task.Delay(500, _cts.Token);
            }
        }
        public void StopPolling()
        {
            _cts?.Cancel();
        }

        public async void OnConnectionChanged(bool connected)
        {
            if (Dispatcher != null)
            {
                await Dispatcher.BeginInvoke(() =>
                {
                    IsConnected = connected;

                    Conn_State.Status = connected ? "Connected." : "Disconnected.";
                    SystemMsg = connected ? "[C] connected to server." : "[C] disconnected from server";
                });
            }
            else
            {
                await Application.Current?.Dispatcher?.BeginInvoke(
                    new Action(() =>
                    {
                        IsConnected = connected;

                        Conn_State.Status = connected ? "Connected." : "Disconnected.";
                        SystemMsg = connected ? "[C] connected to server." : "[C] disconnected from server";

                        StopPolling();
                    }));
            }
        }

        private async Task Write(string ctrl, int input)
        {
            var spec = PcsSpecs.ControlWrite[ctrl];

            ushort value = (ushort)(input / spec.Scale);

            await _client.WriteSingleRegisterAsync(spec.Address, value);
        }

        #endregion
    }
}
