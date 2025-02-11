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
    public MainPage() : this(DependencyService.Get<IPrinterService>())
    {
    }

    private async void OnPrintImageClicked(object sender, EventArgs e)
    {
        try
        {
            bool connected = await _printerService.Connect("74:F0:7D:E5:91:F7");
            if (connected)
            {
                await _printerService.PrintImage("logo");
                await DisplayAlert("Success", "Image printed correctly", "OK");
            }
            else
            {
                await DisplayAlert("Error", "Could not connect to printer", "OK");
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
