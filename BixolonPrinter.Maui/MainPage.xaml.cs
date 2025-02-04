using BixolonPrinter.Maui.Services;

namespace BixolonPrinter.Maui;

public partial class MainPage : ContentPage
{
    private readonly IPrinterService _printerService;

    public MainPage(IPrinterService printerService)
    {
        InitializeComponent();
        _printerService = printerService;
    }

    // Parameterless constructor, required for instantiation via XAML.
    //  Here DependencyService is used to get the implementation.
    public MainPage() : this(DependencyService.Get<IPrinterService>())
    {
    }

    private async void OnPrintClicked(object sender, EventArgs e)
    {
        IsBusy = true;

        try
        {
            var connected = await _printerService.Connect("74:F0:7D:E5:91:F7");
            if (connected)
            {
                await _printerService.PrintText("Test in MAUI!\nBixolon SPP-R200III");
                await DisplayAlert("Success", "Printing completed", "OK");
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
            IsBusy = false;
        }
    }
}
