using System;
using System.IO;
using System.Threading.Tasks;

namespace Nexus_Omok_Game
{
    /// <summary>
    /// AI 플레이어 팩토리
    /// </summary>
    public static class AIPlayerFactory
    {
        public static async Task<IAIPlayer> CreateAsync(
            AIDifficulty difficulty,
            string? openAIApiKey = null,
            string? rapfiEnginePath = null)
        {
            return difficulty switch
            {
                AIDifficulty.Easy => new EasyAI(),
                AIDifficulty.Normal => new NormalAI(),
                AIDifficulty.Hard => new HardAI(),
                AIDifficulty.ChatGPT => new ChatGPTAI(openAIApiKey
                   ?? throw new ArgumentNullException(nameof(openAIApiKey), "ChatGPT 모드는 API 키가 필요합니다")),
                AIDifficulty.Rapfi => await CreateRapfiAIAsync(rapfiEnginePath),
                _ => new EasyAI() // 기본값
            };
        }

        // 동기 버전 (하위 호환성)
        public static IAIPlayer Create(AIDifficulty difficulty, string? openAIApiKey = null)
        {
            return difficulty switch
            {
                AIDifficulty.Easy => new EasyAI(),
                AIDifficulty.Normal => new NormalAI(),
                AIDifficulty.Hard => new HardAI(),
                AIDifficulty.ChatGPT => new ChatGPTAI(openAIApiKey
             ?? throw new ArgumentNullException(nameof(openAIApiKey), "ChatGPT 모드는 API 키가 필요합니다")),
                AIDifficulty.Rapfi => throw new InvalidOperationException("Rapfi는 CreateAsync를 사용하세요"),
                _ => new EasyAI() // 기본값
            };
        }

        private static async Task<RapfiAI> CreateRapfiAIAsync(string? enginePath)
        {
            string path = enginePath ?? FindRapfiExecutable();
            var rapfi = new RapfiAI(path);

            bool initialized = await rapfi.InitializeAsync();
            if (!initialized)
            {
                throw new Exception("Failed to initialize Rapfi engine. Please check if pbrain-rapfi*.exe exists.");
            }

            return rapfi;
        }

        /// <summary>
        /// Rapfi 실행 파일 자동 검색
        /// </summary>
        private static string FindRapfiExecutable()
        {
            // 우선순위: AVXVNNI > AVX2 > SSE
            string[] engineNames = new[]
            {
                "pbrain-rapfi-windows-avxvnni.exe",
                "pbrain-rapfi-windows-avx2.exe",                
                "pbrain-rapfi-windows-sse.exe"
            };

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            foreach (var engineName in engineNames)
            {
                // 1. RAPFI 폴더
                string rapfiPath = Path.Combine(baseDir, "RAPFI", engineName);
                if (File.Exists(rapfiPath))
                {
                    return rapfiPath;
                }

                // 2. Engines 폴더
                string enginesPath = Path.Combine(baseDir, "Engines", engineName);
                if (File.Exists(enginesPath))
                {
                    return enginesPath;
                }

                // 3. 현재 디렉토리
                string localPath = Path.Combine(baseDir, engineName);
                if (File.Exists(localPath))
                {
                    return localPath;
                }
            }

            throw new FileNotFoundException(
                  "Rapfi engine not found. Please place pbrain-rapfi*.exe in RAPFI/ or Engines/ folder.\n" +
                 "Download from https://github.com/dhbloo/rapfi/releases");
        }
    }
}
