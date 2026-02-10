#if WINDOWS
using Anti_Bunda_Mole.Models;
using Anti_Bunda_Mole.Methods;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Linq;
using System.Threading;
using Border = Microsoft.UI.Xaml.Controls.Border;
using Grid = Microsoft.UI.Xaml.Controls.Grid;
using HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment;
using ScrollView = Microsoft.Maui.Controls.ScrollView;
using Thickness = Microsoft.UI.Xaml.Thickness;
using VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment;
using FrameworkElement = Microsoft.UI.Xaml.FrameworkElement;

namespace Anti_Bunda_Mole.Platforms.Windows
{
    public static class TaskOverlayController
    {
        private static bool _subscribed = false;
        private static Func<ScrollView>? _lastBuilder;

        private static OverlayScheduleManager? _scheduleManager;

        public static void ShowTasks(Func<ScrollView> buildCards)
        {
            _lastBuilder = buildCards;

            ConfigManager.Instance.LoadIfNeeded();
            var config = ConfigManager.Instance.Config;

            if (!_subscribed)
            {
                ConfigManager.Instance.ConfigChanged += OnConfigChanged;
                _subscribed = true;
            }

            BuildAndShowOverlay(config, buildCards);
        }

        private static void BuildAndShowOverlay(Configuracoes config, Func<ScrollView> buildCards)
        {
            // Define side and order based on config
            OverlaySide side = config.PosicaoTarefas?.Contains("_l") == true
                ? OverlaySide.Left
                : OverlaySide.Right;

            bool bottomToTop = config.PosicaoTarefas?.Contains("lower") == true;

            var mauiPanel = buildCards();

            var xamlPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            var children = Enumerable.Empty<IView>();

            if (mauiPanel.Content is IVisualTreeElement visual)
            {
                children = visual.GetVisualChildren().OfType<IView>();
            }
            if (bottomToTop)
                children.Reverse();

            var mauiContext = MauiWinUIApplication.Current.Application.Windows
                .FirstOrDefault()?.Handler?.MauiContext;

            if (mauiContext == null) return;

            foreach (var mauiChild in children)
            {
                var native = mauiChild.ToPlatform(mauiContext);
                if (native is FrameworkElement element)
                    xamlPanel.Children.Add(element);
            }

            FrameworkElement containerChild;
            if (bottomToTop)
            {
                var grid = new Grid { VerticalAlignment = VerticalAlignment.Stretch };
                xamlPanel.VerticalAlignment = VerticalAlignment.Bottom;
                grid.Children.Add(xamlPanel);
                containerChild = grid;
            }
            else
            {
                containerChild = xamlPanel;
            }

            var scrollViewer = new Microsoft.UI.Xaml.Controls.ScrollViewer
            {
                Content = containerChild,
                VerticalScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollBarVisibility.Disabled
            };

            var container = new Border
            {
                Padding = new Thickness(16),
                Child = scrollViewer
            };

            OverlayManager.ShowOverlay(container, side, width: 400);
        }

        private static void OnConfigChanged()
        {
            CloseOverlay();
            if (_lastBuilder != null)
            {
                var config = ConfigManager.Instance.Config;
                BuildAndShowOverlay(config, _lastBuilder);
            }
        }

        public static void CloseOverlay()
        {
            OverlayManager.CloseOverlay();
        }

        // ============================
        // FLOATING BUTTON CONTROL
        // ============================



        // ============================
        // SCHEDULE CONTROL
        // ============================

        private static void StartSchedule()
        {
            if (_scheduleManager == null)
                _scheduleManager = new OverlayScheduleManager();

            _scheduleManager.Start();
        }

        private static void StopSchedule()
        {
            _scheduleManager?.Stop();
            _scheduleManager = null;
        }
    }

}
#endif
