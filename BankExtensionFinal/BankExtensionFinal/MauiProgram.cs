using Microsoft.Extensions.Logging;

namespace BankExtensionFinal
{
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
                    fonts.AddFont("sf-ui-display-black.otf", "sf-ui-display-black");
                    fonts.AddFont("sf-ui-display-bold.otf", "sf-ui-display-bold");
                    fonts.AddFont("sf-ui-display-medium.otf", "sf-ui-display-medium");
                    fonts.AddFont("sf-ui-display-light.otf", "sf-ui-display-light");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
