using System.Security.Cryptography;
using System.Text;

namespace Nexus_Omok_Game
{
    /// <summary>
    /// OpenAI API 키를 안전하게 저장/로드
    /// Windows DPAPI를 사용한 암호화
    /// </summary>
    public static class SecureApiKeyManager
  {
        private const string SETTINGS_KEY = "EncryptedOpenAIApiKey";

        /// <summary>
        /// API 키를 암호화하여 저장
        /// </summary>
     public static void SaveApiKey(string apiKey)
        {
    if (string.IsNullOrWhiteSpace(apiKey))
   {
         throw new ArgumentException("API key cannot be empty", nameof(apiKey));
   }

      try
      {
     // Windows DPAPI를 사용한 암호화
                var plainBytes = Encoding.UTF8.GetBytes(apiKey);
      var encrypted = ProtectedData.Protect(
          plainBytes,
    null,
      DataProtectionScope.CurrentUser
);

 // Base64로 인코딩하여 저장
              var encryptedBase64 = Convert.ToBase64String(encrypted);
         
      // Settings에 저장 (Settings.settings 파일 필요)
       Properties.Settings.Default[SETTINGS_KEY] = encryptedBase64;
  Properties.Settings.Default.Save();
     }
  catch (Exception ex)
 {
        throw new InvalidOperationException("Failed to save API key", ex);
    }
        }

        /// <summary>
        /// 저장된 API 키를 복호화하여 로드
        /// </summary>
     public static string? LoadApiKey()
        {
     try
         {
       var encryptedBase64 = Properties.Settings.Default[SETTINGS_KEY] as string;
                
           if (string.IsNullOrEmpty(encryptedBase64))
     {
          return null;
 }

 // Base64 디코딩
             var encrypted = Convert.FromBase64String(encryptedBase64);
                
       // DPAPI 복호화
          var decrypted = ProtectedData.Unprotect(
   encrypted,
       null,
        DataProtectionScope.CurrentUser
          );

          return Encoding.UTF8.GetString(decrypted);
     }
       catch
   {
     // 복호화 실패 시 null 반환 (키가 손상되었거나 다른 사용자가 저장)
                return null;
        }
        }

        /// <summary>
        /// 저장된 API 키 삭제
        /// </summary>
  public static void DeleteApiKey()
        {
            Properties.Settings.Default[SETTINGS_KEY] = string.Empty;
 Properties.Settings.Default.Save();
        }

    /// <summary>
        /// API 키가 저장되어 있는지 확인
      /// </summary>
    public static bool HasApiKey()
        {
            var encryptedBase64 = Properties.Settings.Default[SETTINGS_KEY] as string;
  return !string.IsNullOrEmpty(encryptedBase64);
        }
    }
}
