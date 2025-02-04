
using BixolonPrinter.Maui.Services;
#if ANDROID
using BixolonPrinter.Maui.Platforms.Android.Services;
#endif

using Microsoft.Extensions.Logging;

namespace BixolonPrinter.Maui;

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
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if ANDROID
        builder.Services.AddSingleton<IPrinterService, AndroidPrinterService>();

#endif
        // Register the MainPage in the DI container to be resolved by injection
        builder.Services.AddSingleton<MainPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}