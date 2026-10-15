using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GateHelper.AiTrend
{
    public class AiTrendService
    {
        private static readonly string CacheDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "_meta", "cache");
        private static DateTime _lastRequestTime = DateTime.MinValue;
        private const int MinRequestIntervalSeconds = 8; // 새로고침 연타 방지 쿨타임 (초)

        private static readonly string FavoritesFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "_meta", "favorite_repos.json");

        public bool HasApiKey => !string.IsNullOrWhiteSpace(GetGeminiApiKey());

        public string GetGeminiApiKey()
        {
            try
            {
                return Util_Option.LoadUserOptions()?.GeminiApiKey ?? "";
            }
            catch
            {
                return "";
            }
        }

        #region [Favorites Management]

        public List<AiRepoItem> LoadFavorites()
        {
            try
            {
                if (!File.Exists(FavoritesFilePath)) return new List<AiRepoItem>();
                string json = File.ReadAllText(FavoritesFilePath, Encoding.UTF8);
                var items = JsonConvert.DeserializeObject<List<AiRepoItem>>(json) ?? new List<AiRepoItem>();
                int rank = 1;
                foreach (var it in items)
                {
                    it.Rank = rank++;
                    it.IsFavorite = true;
                }
                return items;
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, LogManager.Level.Warning, "AiTrend LoadFavorites Failed");
                return new List<AiRepoItem>();
            }
        }

        public bool IsFavorite(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return false;
            var list = LoadFavorites();
            return list.Exists(x => string.Equals(x.FullName, fullName, StringComparison.OrdinalIgnoreCase));
        }

        public bool ToggleFavorite(AiRepoItem item)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.FullName)) return false;

            try
            {
                var list = LoadFavorites();
                int idx = list.FindIndex(x => string.Equals(x.FullName, item.FullName, StringComparison.OrdinalIgnoreCase));

                bool nowFavorite;
                if (idx >= 0)
                {
                    list.RemoveAt(idx);
                    nowFavorite = false;
                }
                else
                {
                    item.IsFavorite = true;
                    list.Insert(0, item);
                    nowFavorite = true;
                }

                string dir = Path.GetDirectoryName(FavoritesFilePath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                string json = JsonConvert.SerializeObject(list, Formatting.Indented);
                File.WriteAllText(FavoritesFilePath, json, Encoding.UTF8);

                item.IsFavorite = nowFavorite;
                return nowFavorite;
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, LogManager.Level.Warning, "AiTrend ToggleFavorite Failed");
                return false;
            }
        }

        #endregion

        public List<AiTrendCategory> GetCategories()
        {
            return new List<AiTrendCategory>
            {
                new AiTrendCategory
                {
                    Id = "trending",
                    DisplayName = "🔥 전체 깃허브 급상승 (최근 30일)",
                    DynamicQuery = () => $"created:>{DateTime.UtcNow.AddDays(-30):yyyy-MM-dd}&sort=stars&order=desc"
                },
                new AiTrendCategory
                {
                    Id = "ai_trending",
                    DisplayName = "🤖 최신 AI & 에이전트 핫 트렌드",
                    DynamicQuery = () => $"ai+created:>{DateTime.UtcNow.AddMonths(-6):yyyy-MM-dd}&sort=stars&order=desc"
                },
                new AiTrendCategory
                {
                    Id = "productivity",
                    DisplayName = "💻 직장인 생산성 & 업무 자동화 꿀도구",
                    Query = "productivity+OR+developer-tools+OR+workflow+stars:>2000&sort=stars&order=desc"
                },
                new AiTrendCategory
                {
                    Id = "local_ai",
                    DisplayName = "🏠 내 PC 로컬 AI (오프라인/사내보안)",
                    Query = "ollama+OR+llama.cpp+OR+local-ai+OR+open-webui+stars:>1000&sort=stars&order=desc"
                },
                new AiTrendCategory
                {
                    Id = "speech_ai",
                    DisplayName = "🎙️ 음성인식 & 회의록 자동화 AI (STT)",
                    Query = "whisper+OR+speech-to-text+OR+speech-recognition+stars:>1000&sort=stars&order=desc"
                },
                new AiTrendCategory
                {
                    Id = "top_stars",
                    DisplayName = "⭐ 역대 최고 스타 (All-Time Legends)",
                    Query = "ai+stars:>10000&sort=stars&order=desc"
                },
                new AiTrendCategory
                {
                    Id = "favorites",
                    DisplayName = "⭐ 내가 찜한 프로젝트",
                    Query = ""
                }
            };
        }

        public async Task<AiTrendFetchResult> FetchRepositoriesAsync(AiTrendCategory category, bool forceRefresh = false)
        {
            if (category == null)
            {
                return new AiTrendFetchResult
                {
                    DisplayStatus = "카테고리가 올바르지 않습니다.",
                    IsLiveSuccess = false
                };
            }

            // [관심 저장소 (즐겨찾기) 전용 분기]
            if (category.Id == "favorites")
            {
                var favs = LoadFavorites();
                return new AiTrendFetchResult
                {
                    Items = favs,
                    IsLiveSuccess = true,
                    IsFallbackCache = false,
                    DataTimestamp = DateTime.Now,
                    DisplayStatus = favs.Count > 0
                        ? $"⭐ 내가 찜한 AI 프로젝트 ({favs.Count}개 보관 중)"
                        : "⭐ 찜한 프로젝트가 없습니다. 세부 정보창에서 '⭐ 찜하기'를 눌러 저장해보세요!"
                };
            }

            string queryStr = category.GetEffectiveQuery();
            string queryHash = GetShortHash(queryStr);
            string cacheFilePath = Path.Combine(CacheDir, $"ai_trend_{category.Id}_{queryHash}.json");

            // 1. 연타 방지 쿨타임 검사 (강제 새로고침 시 시간당 60회 쿼터 방어)
            if (forceRefresh)
            {
                double elapsedSeconds = (DateTime.Now - _lastRequestTime).TotalSeconds;
                if (elapsedSeconds < MinRequestIntervalSeconds)
                {
                    int remainSec = (int)Math.Ceiling(MinRequestIntervalSeconds - elapsedSeconds);
                    if (File.Exists(cacheFilePath))
                    {
                        var (cachedItems, fTime) = TryReadCache(cacheFilePath);
                        if (cachedItems != null && cachedItems.Count > 0)
                        {
                            EnsureAiSummaries(cachedItems);
                            SyncFavoriteStatus(cachedItems);
                            return new AiTrendFetchResult
                            {
                                Items = cachedItems,
                                IsLiveSuccess = false,
                                IsFallbackCache = false,
                                DataTimestamp = fTime,
                                DisplayStatus = $"요청 간격이 너무 빠릅니다. ({remainSec}초 후 가능) - 기존 데이터 표시"
                            };
                        }
                    }
                    return new AiTrendFetchResult
                    {
                        DisplayStatus = $"요청 간격이 너무 빠릅니다. ({remainSec}초 후 시도 가능)",
                        IsLiveSuccess = false
                    };
                }
            }

            // 2. 캐시가 유효하고 강제 새로고침이 아닌 경우 로컬 캐시 반환 (3시간 유효)
            if (!forceRefresh && File.Exists(cacheFilePath))
            {
                var (cachedItems, fTime) = TryReadCache(cacheFilePath);
                if (cachedItems != null && cachedItems.Count > 0 && (DateTime.Now - fTime).TotalHours < 3)
                {
                    EnsureAiSummaries(cachedItems);
                    SyncFavoriteStatus(cachedItems);
                    return new AiTrendFetchResult
                    {
                        Items = cachedItems,
                        IsLiveSuccess = true,
                        IsFallbackCache = false,
                        DataTimestamp = fTime,
                        DisplayStatus = $"📦 저장된 캐시 데이터 ({fTime:yyyy-MM-dd HH:mm} 저장본)"
                    };
                }
            }

            // 3. GitHub Search API 호출
            try
            {
                _lastRequestTime = DateTime.Now;
                string apiUrl = $"https://api.github.com/search/repositories?q={queryStr}&per_page=15";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiUrl);
                request.Method = "GET";
                request.UserAgent = "GateHelper-DesktopApp";
                request.Accept = "application/vnd.github.v3+json";
                request.Timeout = 10000;

                string responseText = "";
                using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    responseText = await reader.ReadToEndAsync();
                }

                var rootObj = JObject.Parse(responseText);
                var itemsArray = rootObj["items"] as JArray;

                var resultList = new List<AiRepoItem>();
                if (itemsArray != null)
                {
                    var favSet = new HashSet<string>(LoadFavorites().ConvertAll(x => x.FullName), StringComparer.OrdinalIgnoreCase);
                    int rank = 1;
                    foreach (var item in itemsArray)
                    {
                        var repo = new AiRepoItem
                        {
                            Rank = rank++,
                            FullName = item["full_name"]?.ToString() ?? "",
                            RepoName = item["name"]?.ToString() ?? "",
                            Owner = item["owner"]?["login"]?.ToString() ?? "",
                            HtmlUrl = item["html_url"]?.ToString() ?? "",
                            Description = item["description"]?.ToString() ?? "설명이 제공되지 않은 저장소입니다.",
                            Stars = item["stargazers_count"]?.ToObject<int>() ?? 0,
                            Forks = item["forks_count"]?.ToObject<int>() ?? 0,
                            Language = item["language"]?.ToString() ?? "기타",
                            AiSummary = "",
                            PracticalTip = ""
                        };

                        // [최근 커밋(pushed_at) 파싱]
                        if (DateTime.TryParse(item["pushed_at"]?.ToString(), out DateTime dtPushed))
                        {
                            repo.PushedAt = dtPushed;
                        }

                        if (item["topics"] is JArray topicsArr)
                        {
                            foreach (var t in topicsArr)
                            {
                                if (t != null) repo.Topics.Add(t.ToString());
                            }
                        }

                        repo.IsFavorite = favSet.Contains(repo.FullName);
                        resultList.Add(repo);
                    }
                }

                // 기존 캐시에서 이미 생성된 AI 요약이 있다면 먼저 채워넣음
                MergeExistingAiSummaries(resultList, cacheFilePath);

                // 4. Gemini AI 요약 처리 (키가 있으면 생성, 없으면 친절한 안내 문구)
                bool isAiQuotaExceeded = false;
                string apiKey = GetGeminiApiKey();
                if (!string.IsNullOrWhiteSpace(apiKey) && resultList.Count > 0)
                {
                    var status = await TryEnrichWithAiSummariesAsync(resultList, apiKey);
                    if (status == AiEnrichStatus.QuotaExceeded)
                    {
                        isAiQuotaExceeded = true;
                        foreach (var item in resultList)
                        {
                            if (string.IsNullOrWhiteSpace(item.AiSummary))
                            {
                                item.AiSummary = "[할당량 초과] Gemini 분당 호출 한도(15회)에 도달하여 원문 설명을 임시 표시합니다. (잠시 후 새로고침 시 요약 생성)";
                            }
                        }
                    }
                }
                else
                {
                    foreach (var item in resultList)
                    {
                        if (string.IsNullOrWhiteSpace(item.AiSummary))
                        {
                            item.AiSummary = "[API Key 미등록] '🔑 API 설정'을 통해 키를 등록하면 한국어 핵심 요약이 제공됩니다.";
                        }
                    }
                }

                // 5. 로컬 캐시 파일 저장
                SaveCache(cacheFilePath, resultList);

                string displayStatus = isAiQuotaExceeded
                    ? $"✔ GitHub 실시간 최신 데이터 (⚠️ AI 요약 분당 한도 초과로 원문 임시 표시 중 - 잠시 후 새로고침)"
                    : $"✔ GitHub 실시간 최신 데이터 ({DateTime.Now:yyyy-MM-dd HH:mm})";

                return new AiTrendFetchResult
                {
                    Items = resultList,
                    IsLiveSuccess = true,
                    IsFallbackCache = false,
                    IsAiQuotaExceeded = isAiQuotaExceeded,
                    DataTimestamp = DateTime.Now,
                    DisplayStatus = displayStatus
                };
            }
            catch (WebException webEx)
            {
                LogManager.LogException(webEx, LogManager.Level.Warning, "AiTrend GitHub API Call Failed");
                string errorReason = AnalyzeWebException(webEx);

                // 네트워크/사내망/한도 오류 발생 시 기존 캐시가 있다면 오프라인 데이터로 안전 복구
                if (File.Exists(cacheFilePath))
                {
                    var (cachedItems, fTime) = TryReadCache(cacheFilePath);
                    if (cachedItems != null && cachedItems.Count > 0)
                    {
                        EnsureAiSummaries(cachedItems);
                        SyncFavoriteStatus(cachedItems);
                        return new AiTrendFetchResult
                        {
                            Items = cachedItems,
                            IsLiveSuccess = false,
                            IsFallbackCache = true,
                            DataTimestamp = fTime,
                            FailureReason = errorReason,
                            DisplayStatus = $"⚠️ [이전 저장본] {fTime:yyyy-MM-dd HH:mm} 기준 (실시간 갱신 실패: {errorReason})"
                        };
                    }
                }

                // 캐시조차 없는 경우 (최초 실행 + 오프라인/사내망 차단) -> 내장 스타터 팩(대표 AI 오픈소스 5선) 안전망 반환!
                var starterItems = GetStarterPackItems();
                SyncFavoriteStatus(starterItems);
                return new AiTrendFetchResult
                {
                    Items = starterItems,
                    IsLiveSuccess = false,
                    IsFallbackCache = false,
                    IsStarterPack = true,
                    DataTimestamp = DateTime.Now,
                    FailureReason = errorReason,
                    DisplayStatus = $"📡 [오프라인 스타터 팩] 인터넷 연결 대기 중 - 대표 AI 오픈소스 가이드 표시 (사유: {errorReason})"
                };
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, LogManager.Level.Error, "AiTrend Fetch General Error");
                
                if (File.Exists(cacheFilePath))
                {
                    var (cachedItems, fTime) = TryReadCache(cacheFilePath);
                    if (cachedItems != null && cachedItems.Count > 0)
                    {
                        EnsureAiSummaries(cachedItems);
                        SyncFavoriteStatus(cachedItems);
                        return new AiTrendFetchResult
                        {
                            Items = cachedItems,
                            IsLiveSuccess = false,
                            IsFallbackCache = true,
                            DataTimestamp = fTime,
                            FailureReason = ex.Message,
                            DisplayStatus = $"⚠️ [이전 저장본] {fTime:yyyy-MM-dd HH:mm} 기준 (오류 발생: {ex.Message})"
                        };
                    }
                }

                var starterItems = GetStarterPackItems();
                SyncFavoriteStatus(starterItems);
                return new AiTrendFetchResult
                {
                    Items = starterItems,
                    IsLiveSuccess = false,
                    IsFallbackCache = false,
                    IsStarterPack = true,
                    DataTimestamp = DateTime.Now,
                    FailureReason = ex.Message,
                    DisplayStatus = $"📡 [오프라인 스타터 팩] 대표 AI 오픈소스 가이드 표시 (사유: {ex.Message})"
                };
            }
        }

        /// <summary>
        /// 인터넷이 불가하고 캐시도 없는 환경에서 사용자에게 흥미와 읽을거리를 제공하는 대표 AI 오픈소스 5선 기본 데이터
        /// </summary>
        public static List<AiRepoItem> GetStarterPackItems()
        {
            return new List<AiRepoItem>
            {
                new AiRepoItem
                {
                    Rank = 1,
                    FullName = "ollama / ollama",
                    RepoName = "ollama",
                    Owner = "ollama",
                    HtmlUrl = "https://github.com/ollama/ollama",
                    Stars = 98500,
                    Forks = 8200,
                    Language = "Go",
                    Topics = new List<string> { "llm", "local-ai", "llama3", "cli" },
                    PushedAt = DateTime.UtcNow.AddDays(-1),
                    Description = "Get up and running with Llama 3.3, Mistral, Gemma 2, and other large language models locally.",
                    AiSummary = "인터넷 없이 내 PC/서버에서 Llama 3 등 최신 AI를 1초 만에 실행하는 로컬 LLM의 제왕",
                    PracticalTip = "외부 데이터 유출 걱정 없는 사내 폐쇄망 보안 챗봇 및 업무 도우미 구축에 최적"
                },
                new AiRepoItem
                {
                    Rank = 2,
                    FullName = "openai / whisper",
                    RepoName = "whisper",
                    Owner = "openai",
                    HtmlUrl = "https://github.com/openai/whisper",
                    Stars = 73200,
                    Forks = 8900,
                    Language = "Python",
                    Topics = new List<string> { "speech-recognition", "audio", "transcription", "deep-learning" },
                    PushedAt = DateTime.UtcNow.AddDays(-3),
                    Description = "Robust Speech Recognition via Large-Scale Weak Supervision.",
                    AiSummary = "전 세계 100개 언어 음성을 사람 귀 수준으로 정밀하게 텍스트로 변환하는 최고 성능 오픈소스 STT",
                    PracticalTip = "사내 회의 녹음 파일, 세미나 영상, 고객 통화 음성을 자동으로 텍스트 회의록으로 변환"
                },
                new AiRepoItem
                {
                    Rank = 3,
                    FullName = "AUTOMATIC1111 / stable-diffusion-webui",
                    RepoName = "stable-diffusion-webui",
                    Owner = "AUTOMATIC1111",
                    HtmlUrl = "https://github.com/AUTOMATIC1111/stable-diffusion-webui",
                    Stars = 142000,
                    Forks = 26500,
                    Language = "Python",
                    Topics = new List<string> { "image-generation", "stable-diffusion", "gradio", "webui" },
                    PushedAt = DateTime.UtcNow.AddDays(-4),
                    Description = "Stable Diffusion web UI, a browser interface based on Gradio library for Stable Diffusion.",
                    AiSummary = "텍스트 몇 줄로 고화질 일러스트와 실사 이미지를 생성하는 브라우저 기반 GUI 오픈소스",
                    PracticalTip = "사내 보고서 삽화, 프레젠테이션 디자인, 웹 배너 제작 시 이미지 외주 비용 없이 자체 제작"
                },
                new AiRepoItem
                {
                    Rank = 4,
                    FullName = "langchain-ai / langchain",
                    RepoName = "langchain",
                    Owner = "langchain-ai",
                    HtmlUrl = "https://github.com/langchain-ai/langchain",
                    Stars = 99100,
                    Forks = 16200,
                    Language = "Python",
                    Topics = new List<string> { "llm", "agents", "rag", "framework" },
                    PushedAt = DateTime.UtcNow.AddHours(-12),
                    Description = "Build context-aware reasoning applications with LangChain's flexible abstractions and AI components.",
                    AiSummary = "대형 언어 모델(LLM)에 사내 문서(PDF/Excel), DB, 외부 검색 엔진을 손쉽게 연결하는 표준 프레임워크",
                    PracticalTip = "사내 업무 매뉴얼, 설계 가이드, 규정집을 기반으로 정확한 답변을 찾아주는 사내 RAG 시스템 구축"
                },
                new AiRepoItem
                {
                    Rank = 5,
                    FullName = "vllm-project / vllm",
                    RepoName = "vllm",
                    Owner = "vllm-project",
                    HtmlUrl = "https://github.com/vllm-project/vllm",
                    Stars = 33400,
                    Forks = 5100,
                    Language = "Python",
                    Topics = new List<string> { "llm-serving", "inference", "pagedattention", "gpu" },
                    PushedAt = DateTime.UtcNow.AddHours(-6),
                    Description = "A high-throughput and memory-efficient inference and serving engine for LLMs.",
                    AiSummary = "PagedAttention 메모리 기법으로 LLM의 응답 속도를 최대 24배 가속하는 최고속도 서빙 엔진",
                    PracticalTip = "고가의 GPU 서버 증설 없이도 사내 AI 서비스의 동시 접속자 수용량을 10배 이상 확대하여 비용 절감"
                }
            };
        }

        public enum AiEnrichStatus
        {
            Success,
            QuotaExceeded,
            Failed
        }

        public static string GetShortHash(string input)
        {
            if (string.IsNullOrEmpty(input)) return "default";
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(hash).Replace("-", "").Substring(0, 8).ToLowerInvariant();
            }
        }

        /// <summary>
        /// API Key 등록 후 화면의 기존 리스트를 즉시 한국어 AI 요약으로 채워주는 기능
        /// </summary>
        public async Task<bool> ReSummarizeItemsAsync(List<AiRepoItem> items, string categoryId)
        {
            string apiKey = GetGeminiApiKey();
            if (string.IsNullOrWhiteSpace(apiKey) || items == null || items.Count == 0) return false;

            try
            {
                var status = await TryEnrichWithAiSummariesAsync(items, apiKey);
                if (Directory.Exists(CacheDir))
                {
                    string[] existingCaches = Directory.GetFiles(CacheDir, $"ai_trend_{categoryId}*.json");
                    foreach (var cf in existingCaches)
                    {
                        SaveCache(cf, items);
                    }
                }
                return status == AiEnrichStatus.Success;
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, LogManager.Level.Warning, "AiTrend ReSummarizeItemsAsync Failed");
                return false;
            }
        }

        private static readonly string[] GeminiModelsToTry = new[]
        {
            "gemini-2.5-flash",
            "gemini-flash-latest",
            "gemini-3.5-flash-lite",
            "gemini-3.5-flash",
            "gemini-pro-latest"
        };

        private async Task<AiEnrichStatus> TryEnrichWithAiSummariesAsync(List<AiRepoItem> items, string apiKey)
        {
            bool had429 = false;
            try
            {
                var sbPrompt = new StringBuilder();
                sbPrompt.AppendLine("당신은 전세계 최신 AI 오픈소스 트렌드를 분석하는 전문 기술 분석가입니다.");
                sbPrompt.AppendLine("다음 GitHub 저장소 목록을 보고, 각 도구에 대해 다음 2가지 정보를 반드시 한국어로 작성하세요:");
                sbPrompt.AppendLine("1) summary: 이 도구가 무엇인지, 어떤 문제를 해결하는지 명쾌한 1줄 정의 (30~60자)");
                sbPrompt.AppendLine("2) practicalTip: 실무 개발/사내 업무에 어떻게 써먹을 수 있는지 구체적인 활용 포인트 1줄 (30~60자)");
                sbPrompt.AppendLine("반드시 아래 JSON 배열 형식으로만 응답하세요. 백틱이나 마크다운 없이 순수 JSON 문자열만 출력하세요:");
                sbPrompt.AppendLine("[ {\"repo\": \"레포명\", \"summary\": \"1줄 핵심 요약\", \"practicalTip\": \"2줄 실무/업무 활용 포인트\"}, ... ]");
                sbPrompt.AppendLine();

                int limit = Math.Min(items.Count, 15);
                for (int i = 0; i < limit; i++)
                {
                    string desc = string.IsNullOrWhiteSpace(items[i].Description) ? "No description" : items[i].Description;
                    sbPrompt.AppendLine($"- {items[i].FullName}: {desc} (Topics: {string.Join(", ", items[i].Topics)})");
                }

                string promptText = sbPrompt.ToString();
                string jsonResponse = "";

                foreach (var modelName in GeminiModelsToTry)
                {
                    try
                    {
                        string url = $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:generateContent?key={apiKey}";
                        string jsonBody = new JObject
                        {
                            ["contents"] = new JArray
                            {
                                new JObject
                                {
                                    ["role"] = "user",
                                    ["parts"] = new JArray
                                    {
                                        new JObject { ["text"] = promptText }
                                    }
                                }
                            }
                        }.ToString(Formatting.None);

                        byte[] bytes = Encoding.UTF8.GetBytes(jsonBody);
                        HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                        req.Method = "POST";
                        req.ContentType = "application/json";
                        req.ContentLength = bytes.Length;
                        req.Timeout = 14000;

                        using (Stream s = await req.GetRequestStreamAsync())
                        {
                            await s.WriteAsync(bytes, 0, bytes.Length);
                        }

                        using (HttpWebResponse resp = (HttpWebResponse)await req.GetResponseAsync())
                        using (Stream rs = resp.GetResponseStream())
                        using (StreamReader r = new StreamReader(rs, Encoding.UTF8))
                        {
                            string aiRawResp = await r.ReadToEndAsync();
                            var jsonObj = JObject.Parse(aiRawResp);
                            var textToken = jsonObj["candidates"]?[0]?["content"]?["parts"]?[0]?["text"];
                            if (textToken != null)
                            {
                                jsonResponse = textToken.ToString().Trim();
                                if (!string.IsNullOrWhiteSpace(jsonResponse)) break;
                            }
                        }
                    }
                    catch (WebException webEx)
                    {
                        if (webEx.Response is HttpWebResponse httpResp)
                        {
                            int statusCode = (int)httpResp.StatusCode;
                            if (statusCode == 429)
                            {
                                had429 = true;
                                LogManager.LogMessage($"[AiTrend Gemini] 모델 '{modelName}' HTTP 429(분당 한도). 다음 모델 시도.", LogManager.Level.Warning);
                                await Task.Delay(500);
                                continue;
                            }
                            if (statusCode == 404)
                            {
                                LogManager.LogMessage($"[AiTrend Gemini] 모델 '{modelName}' HTTP 404 발생. 다음 모델 시도.", LogManager.Level.Warning);
                                await Task.Delay(300);
                                continue;
                            }
                        }
                        LogManager.LogException(webEx, LogManager.Level.Warning, $"[AiTrend Gemini] 모델 '{modelName}' 호출 통신 오류");
                    }
                }

                if (string.IsNullOrWhiteSpace(jsonResponse))
                {
                    return had429 ? AiEnrichStatus.QuotaExceeded : AiEnrichStatus.Failed;
                }

                // [견고한 파싱] 백틱 제거 및 다단계 파싱
                if (jsonResponse.StartsWith("```json", StringComparison.OrdinalIgnoreCase)) jsonResponse = jsonResponse.Substring(7);
                if (jsonResponse.StartsWith("```")) jsonResponse = jsonResponse.Substring(3);
                if (jsonResponse.EndsWith("```")) jsonResponse = jsonResponse.Substring(0, jsonResponse.Length - 3);
                jsonResponse = jsonResponse.Trim();

                var dict = ParseSummariesSafely(jsonResponse);
                if (dict.Count > 0)
                {
                    foreach (var item in items)
                    {
                        if (dict.TryGetValue(item.FullName, out var pair) || dict.TryGetValue(item.RepoName, out pair))
                        {
                            if (!string.IsNullOrWhiteSpace(pair.summary)) item.AiSummary = pair.summary;
                            if (!string.IsNullOrWhiteSpace(pair.practicalTip)) item.PracticalTip = pair.practicalTip;
                        }
                    }
                    return AiEnrichStatus.Success;
                }
                return had429 ? AiEnrichStatus.QuotaExceeded : AiEnrichStatus.Failed;
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, LogManager.Level.Warning, "AiTrend Gemini Enrichment Failed");
                return had429 ? AiEnrichStatus.QuotaExceeded : AiEnrichStatus.Failed;
            }
        }

        /// <summary>
        /// Gemini JSON 파싱 오류 방어 로직 (JArray 파싱 실패 시 잘림 복구 및 정규식 추출 fallback)
        /// </summary>
        private Dictionary<string, (string summary, string practicalTip)> ParseSummariesSafely(string rawJson)
        {
            var dict = new Dictionary<string, (string summary, string practicalTip)>(StringComparer.OrdinalIgnoreCase);

            // 1차 시도: 표준 JArray 파싱
            try
            {
                var summaryArray = JArray.Parse(rawJson);
                foreach (var row in summaryArray)
                {
                    string repo = row["repo"]?.ToString() ?? row["name"]?.ToString() ?? "";
                    string summary = row["summary"]?.ToString() ?? "";
                    string tip = row["practicalTip"]?.ToString() ?? row["tip"]?.ToString() ?? "";
                    if (!string.IsNullOrEmpty(repo) && (!string.IsNullOrEmpty(summary) || !string.IsNullOrEmpty(tip)))
                    {
                        dict[repo] = (summary, tip);
                    }
                }
                if (dict.Count > 0) return dict;
            }
            catch { }

            // 2차 시도: 토큰 잘림으로 끝이 안 닫힌 경우 (']' 복구 시도)
            try
            {
                string fixedJson = rawJson;
                int lastBrace = fixedJson.LastIndexOf('}');
                if (lastBrace > 0)
                {
                    fixedJson = fixedJson.Substring(0, lastBrace + 1) + "]";
                    var summaryArray = JArray.Parse(fixedJson);
                    foreach (var row in summaryArray)
                    {
                        string repo = row["repo"]?.ToString() ?? row["name"]?.ToString() ?? "";
                        string summary = row["summary"]?.ToString() ?? "";
                        string tip = row["practicalTip"]?.ToString() ?? row["tip"]?.ToString() ?? "";
                        if (!string.IsNullOrEmpty(repo) && (!string.IsNullOrEmpty(summary) || !string.IsNullOrEmpty(tip)))
                        {
                            dict[repo] = (summary, tip);
                        }
                    }
                    if (dict.Count > 0) return dict;
                }
            }
            catch { }

            // 3차 시도: 정규식 기반 부분 추출 (가능한 요약문은 1개라도 전부 구출)
            try
            {
                var matches = Regex.Matches(rawJson, @"[""'](?:repo|name)[""']\s*:\s*[""'](?<repo>[^""']+)[""']\s*,\s*[""']summary[""']\s*:\s*[""'](?<sum>[^""']+)[""'](?:\s*,\s*[""'](?:practicalTip|tip)[""']\s*:\s*[""'](?<tip>[^""']+)[""'])?", RegexOptions.IgnoreCase);
                foreach (Match m in matches)
                {
                    string repo = m.Groups["repo"].Value.Trim();
                    string sum = m.Groups["sum"].Value.Trim();
                    string tip = m.Groups["tip"].Success ? m.Groups["tip"].Value.Trim() : "";
                    if (!string.IsNullOrEmpty(repo) && (!string.IsNullOrEmpty(sum) || !string.IsNullOrEmpty(tip)))
                    {
                        dict[repo] = (sum, tip);
                    }
                }
            }
            catch { }

            return dict;
        }

        private string AnalyzeWebException(WebException webEx)
        {
            if (webEx.Response is HttpWebResponse httpResp)
            {
                int statusCode = (int)httpResp.StatusCode;
                if (statusCode == 403)
                {
                    return "GitHub 시간당 요청 한도(60회) 초과 (잠시 후 다시 시도해 주세요)";
                }
                if (statusCode == 407)
                {
                    return "사내 프록시 인증 필요 (HTTP 407)";
                }
                if (statusCode == 429)
                {
                    return "요청 빈도 제한 도달 (HTTP 429)";
                }
                return $"GitHub 서버 응답 오류 (HTTP {statusCode})";
            }

            switch (webEx.Status)
            {
                case WebExceptionStatus.NameResolutionFailure:
                    return "인터넷 미연결 또는 사내망 DNS 차단";
                case WebExceptionStatus.Timeout:
                    return "GitHub 서버 응답 시간 초과 (네트워크 지연)";
                case WebExceptionStatus.TrustFailure:
                case WebExceptionStatus.SecureChannelFailure:
                    return "사내망 보안 인증서(SSL) 제한";
                case WebExceptionStatus.ConnectFailure:
                    return "GitHub 서버에 연결할 수 없음 (방화벽/네트워크 차단)";
                default:
                    return webEx.Message;
            }
        }

        private (List<AiRepoItem> items, DateTime lastWriteTime) TryReadCache(string cacheFilePath)
        {
            try
            {
                var fileInfo = new FileInfo(cacheFilePath);
                string cachedJson = File.ReadAllText(cacheFilePath, Encoding.UTF8);
                var cachedItems = JsonConvert.DeserializeObject<List<AiRepoItem>>(cachedJson);
                return (cachedItems, fileInfo.LastWriteTime);
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, LogManager.Level.Warning, "AiTrend TryReadCache Failed");
                return (null, DateTime.MinValue);
            }
        }

        private void SaveCache(string cacheFilePath, List<AiRepoItem> items)
        {
            try
            {
                if (!Directory.Exists(CacheDir)) Directory.CreateDirectory(CacheDir);
                string jsonToSave = JsonConvert.SerializeObject(items, Formatting.Indented);
                File.WriteAllText(cacheFilePath, jsonToSave, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, LogManager.Level.Warning, "AiTrend SaveCache Failed");
            }
        }

        private void MergeExistingAiSummaries(List<AiRepoItem> newItems, string cacheFilePath)
        {
            if (!File.Exists(cacheFilePath)) return;
            try
            {
                var (oldItems, _) = TryReadCache(cacheFilePath);
                if (oldItems == null) return;

                var dict = new Dictionary<string, (string summary, string tip)>(StringComparer.OrdinalIgnoreCase);
                foreach (var old in oldItems)
                {
                    if (!string.IsNullOrWhiteSpace(old.AiSummary) && !old.AiSummary.StartsWith("[API Key 미등록]"))
                    {
                        dict[old.FullName] = (old.AiSummary, old.PracticalTip ?? "");
                    }
                }

                foreach (var item in newItems)
                {
                    if (dict.TryGetValue(item.FullName, out var oldPair))
                    {
                        if (string.IsNullOrWhiteSpace(item.AiSummary)) item.AiSummary = oldPair.summary;
                        if (string.IsNullOrWhiteSpace(item.PracticalTip)) item.PracticalTip = oldPair.tip;
                    }
                }
            }
            catch { }
        }

        private void EnsureAiSummaries(List<AiRepoItem> items)
        {
            bool hasKey = HasApiKey;
            foreach (var item in items)
            {
                if (string.IsNullOrWhiteSpace(item.AiSummary))
                {
                    item.AiSummary = hasKey
                        ? item.Description
                        : "[API Key 미등록] '🔑 API 설정'을 통해 키를 등록하면 한국어 핵심 요약이 제공됩니다.";
                }
            }
        }

        private void SyncFavoriteStatus(List<AiRepoItem> items)
        {
            if (items == null) return;
            try
            {
                var favs = LoadFavorites();
                var set = new HashSet<string>(favs.ConvertAll(x => x.FullName), StringComparer.OrdinalIgnoreCase);
                foreach (var item in items)
                {
                    item.IsFavorite = set.Contains(item.FullName);
                }
            }
            catch { }
        }
    }
}
