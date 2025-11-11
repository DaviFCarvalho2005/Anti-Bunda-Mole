#if WINDOWS
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using System;
using System.Runtime.InteropServices;
using WinRT;
using FrameworkElement = Microsoft.UI.Xaml.FrameworkElement;
using WinUIWindow = Microsoft.UI.Xaml.Window;

namespace Anti_Bunda_Mole.Platforms.Windows
{
    public class FloatingButtonWindow
    {
        private WinUIWindow? _window;
        private IntPtr _hwnd;
        private IntPtr _oldWndProc = IntPtr.Zero;
        private WndProcDelegate _wndProcDelegate;

        private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        private const int GWL_STYLE = -16;
        private const int GWL_EXSTYLE = -20;
        private const int GWL_WNDPROC = -4;

        private const uint WS_CAPTION = 0x00C00000;
        private const uint WS_SYSMENU = 0x00080000;
        private const uint WS_MINIMIZEBOX = 0x00020000;
        private const uint WS_MAXIMIZEBOX = 0x00010000;
        private const uint WS_THICKFRAME = 0x00040000;
        private const uint WS_EX_TOOLWINDOW = 0x00000080;
        private const uint WS_EX_TOPMOST = 0x00000008;

        private const int WM_CLOSE = 0x0010;
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, WndProcDelegate newProc);
        [DllImport("user32.dll")]
        private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
            int X, int Y, int cx, int cy, uint uFlags);

        public void Show(FrameworkElement content, int width = 150, int height = 150)
        {
            if (_window != null) return;

            _window = new WinUIWindow
            {
                ExtendsContentIntoTitleBar = true,
                Content = content
            };

            _window.Activate();
            _hwnd = WinRT.Interop.WindowNative.GetWindowHandle(_window);

            // Mantém título apenas para arrastar, remove X, minimizar/maximizar e redimensionamento
            long style = GetWindowLongPtr(_hwnd, GWL_STYLE).ToInt64();
            style &= ~(WS_SYSMENU | WS_MINIMIZEBOX | WS_MAXIMIZEBOX | WS_THICKFRAME);
            style |= WS_CAPTION; // mantém a barra de título para arraste
            SetWindowLongPtr(_hwnd, GWL_STYLE, new IntPtr(style));

            long exStyle = GetWindowLongPtr(_hwnd, GWL_EXSTYLE).ToInt64();
            exStyle |= WS_EX_TOPMOST | WS_EX_TOOLWINDOW;
            SetWindowLongPtr(_hwnd, GWL_EXSTYLE, new IntPtr(exStyle));

            // Intercepta fechamento
            _wndProcDelegate = CustomWndProc;
            _oldWndProc = SetWindowLongPtr(_hwnd, GWL_WNDPROC, _wndProcDelegate);

            // Tamanho e posição inicial
            SetWindowPos(_hwnd, HWND_TOPMOST, 100, 100, width, height, 0);
        }

        public void Close()
        {
            if (_window == null) return;

            _window.Close();
            _window = null;
            _hwnd = IntPtr.Zero;
            _oldWndProc = IntPtr.Zero;
        }

        private IntPtr CustomWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (msg == WM_CLOSE)
                return IntPtr.Zero; // bloqueia Alt+F4/X
            return CallWindowProc(_oldWndProc, hWnd, msg, wParam, lParam);
        }
    }
}
#endif
