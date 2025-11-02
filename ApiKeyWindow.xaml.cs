using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace Nexus_Omok_Game
{
    public partial class ApiKeyWindow : Window
    {
     public ApiKeyWindow()
  {
        InitializeComponent();

    // 기존 API 키가 있으면 표시 (보안상 일부만)
   var existingKey = SecureApiKeyManager.LoadApiKey();
 if (!string.IsNullOrEmpty(existingKey))
          {
    // 처음 10자만 표시
 ApiKeyBox.Password = existingKey.Substring(0, Math.Min(10, existingKey.Length)) + "...";
   }
      }

      private void OkButton_Click(object sender, RoutedEventArgs e)
        {
   var apiKey = ApiKeyBox.Password.Trim();

   if (string.IsNullOrWhiteSpace(apiKey))
     {
      MessageBox.Show(
       "API 키를 입력해주세요.",
           "입력 오류",
  MessageBoxButton.OK,
  MessageBoxImage.Warning);
     return;
 }

 // 기본적인 API 키 형식 검증 (sk-로 시작)
  if (!apiKey.StartsWith("sk-"))
    {
    var result = MessageBox.Show(
       "OpenAI API 키는 일반적으로 'sk-'로 시작합니다.\n" +
    "입력한 키가 올바른지 확인하시겠습니까?",
   "API 키 형식 확인",
     MessageBoxButton.YesNo,
    MessageBoxImage.Question);

     if (result == MessageBoxResult.No)
       {
   return;
   }
   }

            try
 {
           // API 키 저장 (선택한 경우)
  if (SaveKeyCheckBox.IsChecked == true)
     {
      SecureApiKeyManager.SaveApiKey(apiKey);
     }

     // 임시 저장 (이번 세션용)
    Properties.Settings.Default.TempApiKey = apiKey;
 Properties.Settings.Default.Save();

   MessageBox.Show(
          "API 키가 성공적으로 설정되었습니다.\n" +
  (SaveKeyCheckBox.IsChecked == true 
       ? "다음번에도 자동으로 불러옵니다." 
        : "이번 세션에서만 사용됩니다."),
        "성공",
      MessageBoxButton.OK,
    MessageBoxImage.Information);

  DialogResult = true;
        Close();
    }
          catch (Exception ex)
    {
   MessageBox.Show(
     $"API 키 저장 중 오류가 발생했습니다:\n{ex.Message}",
  "오류",
 MessageBoxButton.OK,
     MessageBoxImage.Error);
    }
  }

 private void CancelButton_Click(object sender, RoutedEventArgs e)
     {
            DialogResult = false;
   Close();
     }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
      try
{
     // 기본 브라우저로 링크 열기
         Process.Start(new ProcessStartInfo
  {
  FileName = e.Uri.AbsoluteUri,
        UseShellExecute = true
  });
    e.Handled = true;
       }
      catch
          {
       MessageBox.Show(
       "브라우저를 열 수 없습니다. URL을 수동으로 복사해주세요:\n" +
          e.Uri.AbsoluteUri,
  "오류",
      MessageBoxButton.OK,
       MessageBoxImage.Warning);
  }
        }
    }
}
