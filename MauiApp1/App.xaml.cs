#if WINDOWS
using Anti_Bunda_Mole.Platforms.Windows;
#endif
namespace Anti_Bunda_Mole
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
#if WINDOWS
            // Inicializa o botão flutuante assim que o app estiver carregado
            Microsoft.Maui.Handlers.WindowHandler.Mapper.AppendToMapping("FloatingButtonInit", (handler, view) =>
            {
                TaskOverlayController.ShowFloatingButton();
            });
#endif
        }
    }
}
