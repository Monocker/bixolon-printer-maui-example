
# Bixolon Printer Integration in .NET MAUI (Android)

This project demonstrates how to integrate and use a Bixolon Bluetooth printer in a .NET 8 MAUI application for Android. The solution is based on consuming the official Bixolon SDK via a binding library, with custom metadata adjustments to remove unnecessary classes, and a dedicated Android printing service.

---

## Features

- **Bluetooth Connection & Configuration**:  
  Uses the BXLConfigLoader and POSPrinter classes from the Bixolon SDK to configure and connect to the printer using its MAC address.

- **Text and Image Printing**:  
  Supports printing both text and images. Images are processed (converted to a monochrome format) for optimal thermal printing.

- **Permission Handling**:  
  Manages the necessary Bluetooth (and location for pre-Android 12 devices) permissions at runtime.

- **Service-Based Architecture**:  
  All printer-related functionality is encapsulated in an Android-specific printing service (AndroidPrinterService) that is registered via dependency injection and used by the MAUI UI.

---

## Project Structure

- **BixolonPrinter.Maui**:  
  The main .NET MAUI application that contains the UI (e.g., MainPage.xaml) and application logic.

- **BixolonPrinter.Binding**:  
  A binding library that includes the Bixolon SDK (JAR, AAR files, etc.) and applies metadata transformations via a *Transforms/Metadata.xml* file. This metadata file forces the exposure of the essential classes (BXLConfigLoader, POSPrinter, etc.) and removes unnecessary packages (e.g. Apache Xerces, XML, HTML).

- **BixolonPrinter.Maui.Platforms.Android.Services**:  
  The Android-specific printing service implementation (AndroidPrinterService) that implements the IPrinterService interface.

---

## Binding Configuration

The **Metadata.xml** file (located in the *Transforms* folder of the **BixolonPrinter.Binding** project) is used to:

- **Force the exposure** of essential classes (BXLConfigLoader, POSPrinter, POSPrinterConst, JposException).
- **Remove unnecessary classes** from packages starting with "mf.org.apache" (which include Apache Xerces, XML, HTML, etc.) to avoid compilation conflicts.
- **Adjust the API signature**: For example, force the third parameter of the `printBitmap` method to be an `int` rather than an `Android.Graphics.Color`.

Example **Metadata.xml**:

```xml
<?xml version="1.0" encoding="utf-8"?>
<metadata>
  <!-- Force mapping of essential classes -->
  <attr path="/api/package[@name='com.bxl.config.editor']/class[@name='BXLConfigLoader']" name="managedName">BXLConfigLoader</attr>
  <attr path="/api/package[@name='jpos']/class[@name='POSPrinter']" name="managedName">POSPrinter</attr>
  <attr path="/api/package[@name='jpos']/class[@name='POSPrinterConst']" name="managedName">POSPrinterConst</attr>
  <attr path="/api/package[@name='jpos']/class[@name='JposException']" name="managedName">JposException</attr>

  <!-- Remove all packages starting with "mf.org.apache" -->
  <remove-node path="/api/package[starts-with(@name, 'mf.org.apache')]" />

  <!-- Optionally remove unused Bixolon packages -->
  <remove-node path="/api/package[@name='com.bixolon.commonlib.server']" />
  <remove-node path="/api/package[@name='com.bixolon.commonlib.connectivity.searcher']" />

  <!-- Force the third parameter of the printBitmap method to be an int -->
  <attr path="/api/package[@name='jpos']/class[@name='POSPrinter']/method[@name='printBitmap' and count(parameter)=5]/parameter[3]" name="type">int</attr>
</metadata>
```

The binding project’s .csproj file (BixolonPrinter.Binding.csproj) includes the required SDK files and applies the metadata transformation.

---

## Android Printer Service

The **AndroidPrinterService** (located in *BixolonPrinter.Maui.Platforms.Android.Services*) implements the IPrinterService interface and encapsulates:

- **Bluetooth connection** and printer configuration using the Bixolon SDK.
- **Image printing**: Loading an image from the Android resources, converting it to a monochrome bitmap, and printing it using the SDK’s PrintBitmap method.
- **Text printing** and ticket printing.
- **Disconnection logic**.

Below is an abbreviated version of the service:

```csharp
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
using Microsoft.Maui.Dispatching;

namespace BixolonPrinter.Maui.Platforms.Android.Services
{
    public class AndroidPrinterService : IPrinterService
    {
        private POSPrinter _posPrinter;
        private BXLConfigLoader _bxlConfigLoader;

        public AndroidPrinterService()
        {
            var context = global::Android.App.Application.Context;
            _posPrinter = new POSPrinter(context);
            _bxlConfigLoader = new BXLConfigLoader(context);
        }

        public Task<bool> Connect(string macAddress)
        {
            try
            {
                _bxlConfigLoader.AddEntry(
                    "BIXOLON",
                    BXLConfigLoader.DeviceCategoryPosPrinter,
                    "SPP-R200III",
                    BXLConfigLoader.DeviceBusBluetooth,
                    macAddress
                );
                _bxlConfigLoader.SaveFile();

                _posPrinter.Open("BIXOLON");
                _posPrinter.Claim(5000);
                _posPrinter.DeviceEnabled = true;
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error connecting: " + ex);
                return Task.FromResult(false);
            }
        }

        public async Task PrintImage(string imageName)
        {
            try
            {
                int resId = (int)typeof(Resource.Drawable)
                                .GetField(imageName.ToLowerInvariant())
                                .GetValue(null);
                Bitmap bitmap = BitmapFactory.DecodeResource(global::Android.App.Application.Context.Resources, resId);
                if (bitmap == null)
                    throw new Exception("Unable to load image");

                Bitmap processedBitmap = ConvertToMonochrome(bitmap);

                _posPrinter.PrintBitmap(
                    POSPrinterConst.PtrSReceipt,
                    processedBitmap,
                    _posPrinter.RecLineWidth,      // width (as in the original Java code)
                    POSPrinterConst.PtrBmCenter,     // alignment
                    200                            // height/scale value
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error printing image: " + ex);
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
                System.Diagnostics.Debug.WriteLine("Error printing text: " + jex);
                throw;
            }
        }

        public async Task PrintTicket(List<TicketItem> items)
        {
            await PrintImage("logo");
            await PrintText("===== TICKET =====\n");
            foreach (var item in items)
            {
                string line = $"Folio: {item.Folio}\nProduct: {item.ProductName}\nPrice: ${item.Price:0.00}\n";
                await PrintText(line);
            }
            await PrintText("\nThank you for your purchase!\n");
        }

        public Task Disconnect()
        {
            try { _posPrinter.Close(); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine("Error disconnecting: " + ex); }
            return Task.CompletedTask;
        }

        private Bitmap ConvertToMonochrome(Bitmap bitmap)
        {
            int width = bitmap.Width, height = bitmap.Height;
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
```

---

## MAUI Integration

### Registering the Service

In **MauiProgram.cs**, the Android-specific printing service is registered only for Android:

```csharp
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using BixolonPrinter.Maui.Services;
using BixolonPrinter.Maui.Platforms.Android.Services;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

#if ANDROID
        // Register the Android printer service
        builder.Services.AddSingleton<IPrinterService, AndroidPrinterService>();
#endif

        return builder.Build();
    }
}
```

### Using the Service in the UI

In **MainPage.xaml** and **MainPage.xaml.cs**, the service is injected and used to trigger printing (for example, when a button is clicked):

**MainPage.xaml**:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage 
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    x:Class="BixolonPrinter.Maui.MainPage">
    <ScrollView>
        <VerticalStackLayout Spacing="20" Padding="20">
            <Label Text="Bixolon Printer MAUI" FontSize="28" HorizontalOptions="Center"/>
            <Button Text="Print Image" Clicked="OnPrintImageClicked" />
            <ActivityIndicator x:Name="activityIndicator" IsRunning="{Binding IsBusy}" Color="Blue" />
        </VerticalStackLayout>
    </ScrollView>
</ContentPage>
```

**MainPage.xaml.cs**:

```csharp
using BixolonPrinter.Maui.Services;
using BixolonPrinter.Maui.Models;

namespace BixolonPrinter.Maui;

public partial class MainPage : ContentPage
{
    private readonly IPrinterService _printerService;

    public MainPage(IPrinterService printerService)
    {
        InitializeComponent();
        _printerService = printerService;
    }

    private async void OnPrintImageClicked(object sender, EventArgs e)
    {
        try
        {
            bool connected = await _printerService.Connect("74:F0:7D:E5:91:F7");
            if (connected)
            {
                await _printerService.PrintImage("logo");
                await DisplayAlert("Success", "Image printed successfully", "OK");
            }
            else
            {
                await DisplayAlert("Error", "Unable to connect to the printer", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            await _printerService.Disconnect();
        }
    }
}
```

---

## Additional Considerations

- **Image Scaling**:  
  If the printed image is too large, adjust the values passed to `PrintBitmap` (such as the last parameter) or pre-scale the image before printing.
- **Permissions**:  
  Make sure the application properly requests and handles the required Bluetooth (and location) permissions at runtime.
- **Resource Names**:  
  Confirm that your image resource (e.g., "logo.png") is correctly placed in the Resources/Drawable folder and that its generated name matches what you are using in the code (in lowercase).

