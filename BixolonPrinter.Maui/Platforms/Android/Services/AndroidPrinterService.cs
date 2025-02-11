using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using global::Android.Bluetooth;
using global::Android.Content;
using global::Android.OS;
using global::Android.Graphics;
using global::Android.Provider;
using Java.IO;
using Java.Util;
using Jpos; 
using Com.Bxl.Config.Editor;
using BixolonPrinter.Maui.Models;
using BixolonPrinter.Maui.Services;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Dispatching;

namespace BixolonPrinter.Maui.Platforms.Android.Services
{
    public class AndroidPrinterService : IPrinterService
    {
        private POSPrinter _posPrinter;
        private BXLConfigLoader _bxlConfigLoader;

        public AndroidPrinterService()
        {
            // We use global::Android.App.Application.Context to get the Android context
            var context = global::Android.App.Application.Context;
            _posPrinter = new POSPrinter(context);
            _bxlConfigLoader = new BXLConfigLoader(context);
        }

        public Task<bool> Connect(string macAddress)
        {
            try
            {
                // Set up the printer using the MAC address
                _bxlConfigLoader.AddEntry(
                    "BIXOLON",
                    BXLConfigLoader.DeviceCategoryPosPrinter,
                    "SPP-R200III",
                    BXLConfigLoader.DeviceBusBluetooth,
                    macAddress
                );
                _bxlConfigLoader.SaveFile();

                // Open the device with the configured logical name
                _posPrinter.Open("BIXOLON");
                _posPrinter.Claim(5000);
                _posPrinter.DeviceEnabled = true;

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al conectar: " + ex.ToString());
                return Task.FromResult(false);
            }
        }

        public async Task PrintImage(string imageName)
        {
            try
            {
                // It is assumed that the image is located in Resources/drawable and its generated field is called "logo" (in lowercase)
                int resId = (int)typeof(Resource.Drawable)
                                .GetField(imageName.ToLowerInvariant())
                                .GetValue(null);
                Bitmap bitmap = BitmapFactory.DecodeResource(global::Android.App.Application.Context.Resources, resId);
                if (bitmap == null)
                    throw new Exception("No se pudo cargar la imagen");

                Bitmap processedBitmap = ConvertToMonochrome(bitmap);

                _posPrinter.PrintBitmap(
                    POSPrinterConst.PtrSReceipt,
                    processedBitmap,
                    _posPrinter.RecLineWidth,
                    POSPrinterConst.PtrBmCenter,
                    200
                );

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al imprimir imagen: " + ex.ToString());
                throw;
            }
        }
        public async Task PrintText(string text)
        {
            try
            {
                _posPrinter.PrintNormal(POSPrinterConst.PtrSReceipt, text + "\n");
            }
            catch (JposException jex)
            {
                System.Diagnostics.Debug.WriteLine("Error al imprimir texto: " + jex.ToString());
                throw;
            }
        }

        public async Task PrintTicket(List<TicketItem> items)
        {
            // Example: print ticket with logo, header, items and footer
            await PrintImage("logo"); 
            await PrintText("===== TICKET DE COMPRA =====\n");
            foreach (var item in items)
            {
                string line = $"Folio: {item.Folio}\nProducto: {item.ProductName}\nPrecio: ${item.Price:0.00}\n";
                await PrintText(line);
            }
            await PrintText("\n¡Gracias por su compra!\n");
        }

        public Task Disconnect()
        {
            try
            {
                _posPrinter.Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al desconectar: " + ex.ToString());
            }
            return Task.CompletedTask;
        }

        private Bitmap ConvertToMonochrome(Bitmap bitmap)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            Bitmap monochromeBitmap = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int pixel = bitmap.GetPixel(x, y);
                    int red = (pixel >> 16) & 0xFF;
                    int green = (pixel >> 8) & 0xFF;
                    int blue = pixel & 0xFF;
                    int gray = (red + green + blue) / 3;
                    int newColor = (gray > 128) ? unchecked((int)0xFFFFFFFF) : unchecked((int)0xFF000000);
                    
                    monochromeBitmap.SetPixel(x, y, new global::Android.Graphics.Color(newColor));
                }
            }
            return monochromeBitmap;
        }
    }
}
