#if WINDOWS
using Microsoft.Maui.Controls;
using Microsoft.UI.Windowing;
using Application = Microsoft.Maui.Controls.Application;

namespace Anti_Bunda_Mole.Platforms.Windows
{
    public static class WindowHelper
    {
        public static void HideMainWindow()
        {
            var mauiWindow = Application.Current?.MainPage?.Window;
            if (mauiWindow == null) return;

            var appWindow = mauiWindow.Handler?.PlatformView is Microsoft.UI.Xaml.Window winui
                ? winui.AppWindow
                : null;

            appWindow?.Hide();
        }

        public static void ActivateMainWindow()
        {
            var mauiWindow = Application.Current?.MainPage?.Window;
            if (mauiWindow == null) return;

            var appWindow = mauiWindow.Handler?.PlatformView is Microsoft.UI.Xaml.Window winui
                ? winui.AppWindow
                : null;

            appWindow?.Show();
        }
    }
}
#endif
