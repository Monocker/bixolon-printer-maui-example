
using Android.Bluetooth;
using Java.Util;
using System.Text;
using BixolonPrinter.Maui.Services;
using System.IO; 
using Android.Content;
using Android.OS;
using Android.Content.PM; 
using global::Android.App; 
using global::Android.Provider; 

namespace BixolonPrinter.Maui.Platforms.Android.Services
{
    public class AndroidPrinterService : IPrinterService
    {
        private BluetoothSocket _socket;
        private Stream _outputStream;
        private readonly UUID _uuid = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");
        private readonly Context _context;

        public AndroidPrinterService()
        {
            _context = global::Android.App.Application.Context;
        }

        public async Task<bool> Connect(string macAddress)
        {
            try
            {
                var adapter = BluetoothAdapter.DefaultAdapter;
                if (adapter == null || !adapter.IsEnabled)
                {
                    OpenBluetoothSettings();
                    return false;
                }

                if (!await CheckPermissions())
                    return false;

                var device = adapter.GetRemoteDevice(macAddress);
                if (device == null || device.BondState != Bond.Bonded)
                    return false;

                _socket = device.CreateRfcommSocketToServiceRecord(_uuid);
                await _socket.ConnectAsync();
                _outputStream = _socket.OutputStream; 

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Connection Error: {ex}");
                return false;
            }
        }

        private async Task<bool> CheckPermissions()
        {
            var permissions = new List<string>();

            if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
            {
                if (_context.CheckSelfPermission(global::Android.Manifest.Permission.BluetoothConnect) != global::Android.Content.PM.Permission.Granted)
                    permissions.Add(global::Android.Manifest.Permission.BluetoothConnect);
            }
            else
            {
                if (_context.CheckSelfPermission(global::Android.Manifest.Permission.Bluetooth) != global::Android.Content.PM.Permission.Granted)
                    permissions.Add(global::Android.Manifest.Permission.Bluetooth);

                if (_context.CheckSelfPermission(global::Android.Manifest.Permission.AccessFineLocation) != global::Android.Content.PM.Permission.Granted)
                    permissions.Add(global::Android.Manifest.Permission.AccessFineLocation);
            }

            if (permissions.Any())
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                    if (status != PermissionStatus.Granted)
                        throw new Exception("Location permissions required for Bluetooth");
                });
            }

            return true;
        }

        private void OpenBluetoothSettings()
        {
            var intent = new Intent(global::Android.Provider.Settings.ActionBluetoothSettings);
            intent.AddFlags(ActivityFlags.NewTask);
            _context.StartActivity(intent);
        }

        public async Task PrintText(string text)
        {
            if (_outputStream == null)
                throw new InvalidOperationException("There is no connection to the printer");

            try
            {
                var data = Encoding.ASCII.GetBytes(text);
                var command = new List<byte> { 0x1B, 0x40 };

                await _outputStream.WriteAsync(command.ToArray(), 0, command.Count);
                await _outputStream.WriteAsync(data, 0, data.Length);
                await _outputStream.WriteAsync(new byte[] { 0x0A }, 0, 1); 

                await _outputStream.FlushAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Print Error: {ex}");
                await Disconnect();
                throw;
            }
        }

        public Task Disconnect()
        {
            _outputStream?.Close();
            _socket?.Close();
            return Task.CompletedTask;
        }
    }
}

