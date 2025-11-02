using System.Configuration;
using System.Data;
using System.Windows;

namespace Nexus_Omok_Game
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                // 게임 모드 선택 창 표시
                var gameModeWindow = new GameModeWindow();
                bool? result = gameModeWindow.ShowDialog();

                // DialogResult가 true인 경우만 메인 윈도우 시작
                if (result == true && gameModeWindow.SelectedSettings != null)
                {
                    // 선택된 설정으로 메인 윈도우 시작
                    var mainWindow = new MainWindow(gameModeWindow.SelectedSettings);

                    // MainWindow 속성 설정
                    MainWindow = mainWindow;

                    // MainWindow가 닫힐 때 애플리케이션 종료
                    mainWindow.Closed += (s, args) => Shutdown();

                    // 창을 표시하고 활성화
                    mainWindow.Show();
                    mainWindow.Activate();
                }
                else
                {
                    // 취소하거나 창을 닫으면 앱 종료
                    Shutdown();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                    "Application Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Shutdown();
            }
        }
    }
}
