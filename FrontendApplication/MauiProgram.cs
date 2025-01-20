using FrontendApplication.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Plugin.Firebase.Auth;
using Microsoft.Maui.LifecycleEvents;
using System;
#if IOS
using Plugin.Firebase.Core.Platforms.iOS;
#elif ANDROID
using Plugin.Firebase.Core.Platforms.Android;
using CommunityToolkit.Maui;

#endif
namespace FrontendApplication
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.UseMauiApp<App>().ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            }).RegisterFirebaseServices().UseMauiCommunityToolkit();
            
            // Configure Https
            var baseUrl = new Uri(AppConfig.ApiBaseUrl);
#if ANDROID
            baseUrl = new Uri("https://10.0.2.2:7226/api");
#endif
            // Register the HttpClient with a platform-specific base address
            builder.Services.AddSingleton<HttpClientService>();

            builder.Services.AddHttpClient("DefaultClient", client =>
            {
                client.BaseAddress = baseUrl;
            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                var httpClientService = builder.Services.BuildServiceProvider().GetRequiredService<HttpClientService>();
                return httpClientService.GetPlatformSpecificHttpMessageHandler();
            });

            // Register the services that use the shared HttpClient
            builder.Services.AddSingleton<UserServiceApi>();
            builder.Services.AddSingleton<GroupServiceApi>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        private static MauiAppBuilder RegisterFirebaseServices(this MauiAppBuilder builder)
        {
            builder.ConfigureLifecycleEvents(events =>
            {
#if IOS
                events.AddiOS(iOS => iOS.WillFinishLaunching((_,__) =>
                {
                    CrossFirebase.Initialize();
                    return false;
                }));
#elif ANDROID
                events.AddAndroid(android => android.OnCreate((activity, _) => CrossFirebase.Initialize(activity)));
#endif
            });

            builder.Services.AddSingleton(_ => CrossFirebaseAuth.Current);
            return builder;
        }
    }
}