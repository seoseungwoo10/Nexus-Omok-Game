using System.Windows;

namespace Nexus_Omok_Game
{
    public partial class AIDifficultyWindow : Window
    {
        public AIDifficulty SelectedDifficulty { get; private set; }
        public RapfiStrength SelectedRapfiStrength { get; private set; }

        public AIDifficultyWindow()
        {
            InitializeComponent();
            SelectedDifficulty = AIDifficulty.Easy;
            SelectedRapfiStrength = RapfiStrength.Intermediate;
            UpdateApiKeyStatus();
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (EasyRadio.IsChecked == true)
                SelectedDifficulty = AIDifficulty.Easy;
            else if (NormalRadio.IsChecked == true)
                SelectedDifficulty = AIDifficulty.Normal;
            else if (HardRadio.IsChecked == true)
                SelectedDifficulty = AIDifficulty.Hard;
            else if (ChatGPTRadio.IsChecked == true)
            {
                SelectedDifficulty = AIDifficulty.ChatGPT;

                // ChatGPT 모드는 API 키가 필요
                if (!SecureApiKeyManager.HasApiKey())
                {
                    var result = MessageBox.Show(
        "ChatGPT AI를 사용하려면 OpenAI API 키가 필요합니다.\n" +
              "지금 설정하시겠습니까?",
         "API 키 필요",
      MessageBoxButton.YesNo,
      MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        ShowApiKeyDialog();
                        if (!SecureApiKeyManager.HasApiKey())
                        {
                            return; // API 키 입력 취소됨
                        }
                    }
                    else
                    {
                        return; // ChatGPT 선택 취소
                    }
                }
            }
            else if (RapfiRadio.IsChecked == true)
            {
                SelectedDifficulty = AIDifficulty.Rapfi;

                // Rapfi 강도 가져오기
                if (RapfiStrengthCombo.SelectedItem is System.Windows.Controls.ComboBoxItem selectedItem)
                {
                    SelectedRapfiStrength = selectedItem.Tag?.ToString() switch
                    {
                        "Beginner" => RapfiStrength.Beginner,
              "Intermediate" => RapfiStrength.Intermediate,
                 "Advanced" => RapfiStrength.Advanced,
        "Master" => RapfiStrength.Master,
       "GrandMaster" => RapfiStrength.GrandMaster,
        _ => RapfiStrength.Intermediate
                    };
                }

                // Rapfi 엔진 파일 존재 확인 (선택사항)
                var result = MessageBox.Show(
        "🚀 Rapfi Engine (세계 최강급 오목 AI)\n\n" +
     $"선택한 강도: {GetRapfiStrengthName(SelectedRapfiStrength)}\n\n" +
 "Rapfi는 Gomocup 2023 대회 우승 엔진으로,\n" +
"프로 기사 수준의 플레이를 제공합니다.\n\n" +
       "계속하시겠습니까?",
       "Rapfi Engine 확인",
            MessageBoxButton.YesNo,
       MessageBoxImage.Information);

                if (result != MessageBoxResult.Yes)
                {
                    return; // Rapfi 선택 취소
                }
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ApiKeyButton_Click(object sender, RoutedEventArgs e)
        {
            ShowApiKeyDialog();
        }

        private void ShowApiKeyDialog()
        {
            var apiKeyWindow = new ApiKeyWindow();
            if (apiKeyWindow.ShowDialog() == true)
            {
                UpdateApiKeyStatus();
            }
        }

        private void UpdateApiKeyStatus()
        {
            if (SecureApiKeyManager.HasApiKey())
            {
                ApiKeyStatusText.Text = "✅ API 키 저장됨";
                ApiKeyStatusText.Foreground = System.Windows.Media.Brushes.Green;
            }
            else
            {
                ApiKeyStatusText.Text = "❌ 저장된 API 키 없음";
                ApiKeyStatusText.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        private void RapfiRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (RapfiStrengthPanel != null)
            {
                RapfiStrengthPanel.Visibility = Visibility.Visible;
            }
        }

        private void RapfiRadio_Unchecked(object sender, RoutedEventArgs e)
        {
            if (RapfiStrengthPanel != null)
            {
                RapfiStrengthPanel.Visibility = Visibility.Collapsed;
            }
        }

        private string GetRapfiStrengthName(RapfiStrength strength)
        {
            return strength switch
            {
                RapfiStrength.Beginner => "초급 (Beginner) - 승률 30%",
                RapfiStrength.Intermediate => "중급 (Intermediate) - 승률 60%",
                RapfiStrength.Advanced => "고급 (Advanced) - 승률 80%",
                RapfiStrength.Master => "마스터 (Master) - 승률 90%",
                RapfiStrength.GrandMaster => "그랜드마스터 - 승률 95% (프로 수준)",
                _ => "중급"
            };
        }
    }
}
