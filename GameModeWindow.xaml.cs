using System.Windows;

namespace Nexus_Omok_Game
{
    public partial class GameModeWindow : Window
    {
      public GameSettings SelectedSettings { get; private set; }

        public GameModeWindow()
        {
      InitializeComponent();
            SelectedSettings = new GameSettings();
        }

        private void TwoPlayerButton_Click(object sender, RoutedEventArgs e)
        {
SelectedSettings.Mode = GameMode.TwoPlayer;
            DialogResult = true;
    Close();
        }

        private void AIButton_Click(object sender, RoutedEventArgs e)
        {
        // AI 난이도 선택 창 표시
         var difficultyWindow = new AIDifficultyWindow();
    if (difficultyWindow.ShowDialog() == true)
            {
            SelectedSettings.Mode = GameMode.VsAI;
     SelectedSettings.AIDifficulty = difficultyWindow.SelectedDifficulty;
          DialogResult = true;
      Close();
 }
    // else 블록 추가: 취소 시에는 아무것도 하지 않고 GameModeWindow로 돌아감
     // DialogResult를 설정하지 않아서 창이 열린 상태로 유지됨
        }
    }
}
