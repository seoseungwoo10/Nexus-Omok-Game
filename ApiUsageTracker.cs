namespace Nexus_Omok_Game
{
    /// <summary>
    /// OpenAI API 사용량 추적
    /// 월별 사용량을 추적하여 과다 사용 방지
    /// </summary>
    public static class ApiUsageTracker
    {
        private const int MONTHLY_REQUEST_LIMIT = 1000; // 월 1000회 제한

        /// <summary>
        /// 현재 월의 사용량을 가져옵니다
 /// </summary>
   public static int GetMonthlyUsage()
        {
var currentMonth = DateTime.Now.ToString("yyyy-MM");
 var savedMonth = Properties.Settings.Default.UsageMonth;

          if (savedMonth != currentMonth)
         {
     // 새 달 시작 - 리셋
    Properties.Settings.Default.UsageMonth = currentMonth;
   Properties.Settings.Default.UsageCount = 0;
    Properties.Settings.Default.Save();
return 0;
    }

  return Properties.Settings.Default.UsageCount;
        }

        /// <summary>
     /// 사용량을 1 증가시킵니다
        /// </summary>
        public static void IncrementUsage()
        {
    Properties.Settings.Default.UsageCount++;
   Properties.Settings.Default.Save();
        }

        /// <summary>
  /// API 요청을 할 수 있는지 확인 (한도 체크)
  /// </summary>
 public static bool CanMakeRequest()
 {
        return GetMonthlyUsage() < MONTHLY_REQUEST_LIMIT;
   }

        /// <summary>
        /// 남은 요청 횟수를 가져옵니다
        /// </summary>
   public static int GetRemainingRequests()
        {
  return Math.Max(0, MONTHLY_REQUEST_LIMIT - GetMonthlyUsage());
        }

 /// <summary>
      /// 사용량을 리셋합니다 (테스트용)
  /// </summary>
      public static void ResetUsage()
        {
   Properties.Settings.Default.UsageCount = 0;
    Properties.Settings.Default.Save();
        }
    }
}
