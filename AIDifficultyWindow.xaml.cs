using System.Windows;

namespace Nexus_Omok_Game
{
    public partial class AIDifficultyWindow : Window
    {
        public AIDifficulty SelectedDifficulty { get; private set; }

        public AIDifficultyWindow()
        {
            InitializeComponent();
            SelectedDifficulty = AIDifficulty.Easy;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (EasyRadio.IsChecked == true)
                SelectedDifficulty = AIDifficulty.Easy;
            else if (NormalRadio.IsChecked == true)
                SelectedDifficulty = AIDifficulty.Normal;
            else if (HardRadio.IsChecked == true)
                SelectedDifficulty = AIDifficulty.Hard;

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
