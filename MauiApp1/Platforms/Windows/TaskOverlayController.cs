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

        private static FloatingButtonWindow? _floatingButton;
        private static Timer? _scheduleTimer;
        private static bool _overlayAtivoPorHorario = false;

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

            var children = mauiPanel.Children.ToList();
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
            Close();
            if (_lastBuilder != null)
            {
                var config = ConfigManager.Instance.Config;
                BuildAndShowOverlay(config, _lastBuilder);
            }
        }

        public static void Close()
        {
            OverlayManager.CloseOverlay();
        }

        // ============================
        // CONTROLE DO BOTÃO FLUTUANTE
        // ============================

        public static void ShowFloatingButton()
        {
            if (_floatingButton == null)
                _floatingButton = new FloatingButtonWindow();

            var mauiButton = new ImageButton
            {
                Source = "home.png",
                BackgroundColor = Colors.Transparent,
                WidthRequest = 75,
                HeightRequest = 75
            };

            mauiButton.Clicked += (s, e) =>
            {
                try
                {
                    var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                    System.Diagnostics.Process.Start(exePath);
                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erro ao reiniciar: {ex}");
                }
            };

            var mauiContext = MauiWinUIApplication.Current.Application.Windows
                .FirstOrDefault()?.Handler?.MauiContext;

            if (mauiContext == null) return;

            var nativeButton = mauiButton.ToPlatform(mauiContext) as FrameworkElement;
            if (nativeButton != null)
            {
                var grid = new Grid
                {
                    Width = 160,
                    Height = 180,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                var border = new Border
                {
                    Width = 120,
                    Height = 120,
                    Child = nativeButton
                };
                grid.Children.Add(border);

                _floatingButton.Show(grid);
            }

            StartScheduleCheck();
        }

        public static void CloseFloatingButton()
        {
            _floatingButton?.Close();
            _floatingButton = null;
            StopScheduleCheck();
        }

        // ============================
        // CONTROLE DO SCHEDULE
        // ============================

        private static void StartScheduleCheck()
        {
            if (_scheduleTimer != null) return;

            _scheduleTimer = new Timer(_ =>
            {
                try
                {
                    ConfigManager.Instance.LoadIfNeeded();
                    var config = ConfigManager.Instance.Config;

                    var agora = DateTime.Now;
                    int diaAtual = (int)agora.DayOfWeek; // 0 = domingo, 6 = sábado

                    bool dentroDoHorario = false;

                    if (config.Dias.ContainsKey(diaAtual))
                    {
                        var diaConfig = config.Dias[diaAtual];
                        if (diaConfig.Ativo)
                        {
                            dentroDoHorario = diaConfig.Periodos.Any(p =>
                            {
                                if (TimeSpan.TryParse(p.Inicio, out var inicio) &&
                                    TimeSpan.TryParse(p.Fim, out var fim))
                                {
                                    return agora.TimeOfDay >= inicio && agora.TimeOfDay < fim;
                                }
                                return false;
                            });
                        }
                    }

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        // Se horário ativo, mostra overlay com os cards
                        if (dentroDoHorario)
                        {
                            var builder = new OverlayCardBuilder();
                            ShowTasks(builder.BuildCards);
                            _overlayAtivoPorHorario = true;
                        }
                        else
                        {
                            CloseOverlayIfActive();
                        }
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erro no ScheduleChecker: {ex}");
                }
            }, null, 0, 1000);
        }

        private static void StopScheduleCheck()
        {
            _scheduleTimer?.Dispose();
            _scheduleTimer = null;
        }

        private static void CloseOverlayIfActive()
        {
            if (!_overlayAtivoPorHorario) return;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Close();
                _overlayAtivoPorHorario = false;
            });
        }
    }
}
#endif
