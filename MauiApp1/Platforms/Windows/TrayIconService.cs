#if WINDOWS
using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Maui.Controls;
using Application = Microsoft.Maui.Controls.Application;

namespace Anti_Bunda_Mole.Platforms.Windows
{
    public static class TrayIconService
    {
        private static NotifyIcon? trayIcon;

        public static void Init()
        {
            if (trayIcon != null) return;

            trayIcon = new NotifyIcon
            {
                Icon = LoadEmbeddedIcon(),
                Text = "Anti Bunda Mole",
                Visible = true
            };

            var menu = new ContextMenuStrip();

            menu.Items.Add("Open", null, (_, __) =>
            {
                Application.Current?.Dispatcher.Dispatch(ActivateMainWindow);
            });

            menu.Items.Add("Exit", null, (_, __) =>
            {
                trayIcon.Visible = false;
                trayIcon.Dispose();
                Environment.Exit(0);
            });

            trayIcon.ContextMenuStrip = menu;

            trayIcon.DoubleClick += (_, __) =>
            {
                Application.Current?.Dispatcher.Dispatch(ActivateMainWindow);
            };
        }

        private static Icon LoadEmbeddedIcon()
        {
            var assembly = Assembly.GetExecutingAssembly();

            // ISSO TEM QUE BATER COM O NAMESPACE + PASTA + NOME
            const string resourceName =
                "Anti_Bunda_Mole.Platforms.Windows.Assets.app.ico";

            using var stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException(
                    $"Embedded icon not found: {resourceName}"
                );

            return new Icon(stream);
        }

        private static void ActivateMainWindow()
        {
            var mauiWindow = Application.Current?.MainPage?.Window;
            if (mauiWindow == null) return;

            var winuiWindow =
                mauiWindow.Handler?.PlatformView as Microsoft.UI.Xaml.Window;

            winuiWindow?.Activate();
        }
    }
}
#endif
