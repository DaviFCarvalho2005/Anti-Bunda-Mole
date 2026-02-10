#if WINDOWS
using Anti_Bunda_Mole.Platforms.Windows;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.UI.Windowing;
#endif

namespace Anti_Bunda_Mole
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()

#if WINDOWS
                .ConfigureLifecycleEvents(events =>
                {
                    events.AddWindows(windows =>
                    {
                        windows.OnWindowCreated(window =>
                        {
                            TrayIconService.Init();

                            var appWindow = window.AppWindow;

                            appWindow.Closing += (_, args) =>
                            {
                                args.Cancel = true;
                                WindowHelper.HideMainWindow();
                            };
                        });
                    });
                })
#endif
                ;

            return builder.Build();
        }
    }
}
