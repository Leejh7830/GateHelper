using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace GateHelper.MysteryTime
{
    public class MysteryAiEngine
    {
        private readonly string _apiKey;
        private readonly JavaScriptSerializer _jsonSerializer;

        public MysteryAiEngine(string apiKey = "")
        {
            _apiKey = apiKey;
            _jsonSerializer = new JavaScriptSerializer();
        }

        public bool IsApiKeyConfigured => !string.IsNullOrWhiteSpace(_apiKey);

        /// <summary>
        /// 유저의 질문에 대해 예/아니오/관련없음/모호함 4가지 중 판정
        /// </summary>
        public async Task<MysteryQuestion> EvaluateQuestionAsync(MysteryCase currentCase, string userQuestion, int questionNumber)
        {
            var result = new MysteryQuestion
            {
                QuestionNumber = questionNumber,
                QuestionText = userQuestion
            };

            if (currentCase == null || string.IsNullOrWhiteSpace(userQuestion))
            {
                result.AnswerType = MysteryAnswerType.SystemNotice;
                result.AnswerMessage = "질문 내용이 비어있습니다.";
                return result;
            }

            if (!IsApiKeyConfigured)
            {
                // API 키가 없을 때의 로컬 키워드 간이 처리 fallback
                return EvaluateQuestionLocally(currentCase, userQuestion, questionNumber);
            }

            try
            {
                string systemInstruction = $@"너는 추리 게임 '미스터리 타임'의 AI 진행자(Game Master)다.
사건 제목: {currentCase.Title}
사건 현장 설명: {currentCase.Situation}
비밀 진실(정답): {currentCase.Truth}

규칙:
1. 유저의 질문이 비밀 진실과 부합하거나 맞으면 오직 '예' 라고만 답변해라.
2. 유저의 질문이 비밀 진실과 어긋나거나 틀리면 오직 '아니오' 라고만 답변해라.
3. 유저의 질문이 사건 추리와 전혀 상관없거나 불필요한 내용이면 오직 '사건과 관련 없습니다' 라고만 답변해라.
4. 유저의 질문이 이중적인 의미를 가지거나 모호하거나, '예/아니오'로 단정하여 답할 수 없는 질문이면 오직 '질문이 모호합니다. 더 구체적인 예/아니오 질문으로 다시 작성해주세요.' 라고 답변해라.
5. 절대로 사족을 붙이거나 힌트, 비밀 진실을 직접 유출하지 마라.";

                string prompt = $"유저 질문: {userQuestion}";
                string aiResponse = await CallGeminiApiAsync(systemInstruction, prompt);

                aiResponse = aiResponse.Trim();

                if (aiResponse.Contains("질문이 모호합니다"))
                {
                    result.AnswerType = MysteryAnswerType.Ambiguous;
                    result.AnswerMessage = "질문이 모호합니다. 더 구체적인 문장으로 다시 질문해 주세요.";
                }
                else if (aiResponse.Contains("예"))
                {
                    result.AnswerType = MysteryAnswerType.Yes;
                    result.AnswerMessage = "예! (맞습니다)";
                }
                else if (aiResponse.Contains("아니오"))
                {
                    result.AnswerType = MysteryAnswerType.No;
                    result.AnswerMessage = "아니오! (아닙니다)";
                }
                else
                {
                    result.AnswerType = MysteryAnswerType.Irrelevant;
                    result.AnswerMessage = "사건과 관련 없거나 중요하지 않습니다.";
                }
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, LogManager.Level.Warning, "EvaluateQuestionAsync AI Call Failed. Fallback to Local.");
                return EvaluateQuestionLocally(currentCase, userQuestion, questionNumber);
            }

            return result;
        }

        /// <summary>
        /// 유저의 최종 정답 제출 검증
        /// </summary>
        public async Task<(bool isCorrect, string feedback)> EvaluateAnswerAsync(MysteryCase currentCase, string userSolution)
        {
            if (currentCase == null || string.IsNullOrWhiteSpace(userSolution))
            {
                return (false, "제출된 정답이 비어있습니다.");
            }

            if (!IsApiKeyConfigured)
            {
                // 로컬 키워드 매칭 fallback
                int matchCount = 0;
                foreach (var kw in currentCase.Keywords)
                {
                    if (userSolution.Contains(kw)) matchCount++;
                }

                bool localCorrect = matchCount >= Math.Min(1, currentCase.Keywords.Count);
                string localMsg = localCorrect 
                    ? $"[정답 축하합니다!]\n사건의 비밀 전말: {currentCase.Truth}"
                    : $"[아쉽지만 오답입니다.]\n사건의 비밀 전말: {currentCase.Truth}";

                return (localCorrect, localMsg);
            }

            try
            {
                string systemInstruction = $@"너는 추리 게임 '미스터리 타임'의 정답 검수자다.
사건 비밀 진실: {currentCase.Truth}

규칙:
유저가 작성한 사건 전말 답안이 비밀 진실의 핵심 내용(원인, 트릭, 결과)을 정확히 파악했는지 검증해라.
첫 줄에는 오직 [정답] 또는 [오답] 을 적고, 두 번째 줄부터 친절한 총평과 전체 사건의 비밀 전말 해설을 적어라.";

                string prompt = $"유저가 작성한 정답: {userSolution}";
                string aiResponse = await CallGeminiApiAsync(systemInstruction, prompt);

                bool isCorrect = aiResponse.Contains("[정답]");
                return (isCorrect, aiResponse);
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, LogManager.Level.Warning, "EvaluateAnswerAsync AI Failed");
                return (false, $"[검증 중 오류 발생]\n비밀 전말: {currentCase.Truth}");
            }
        }

        private MysteryQuestion EvaluateQuestionLocally(MysteryCase currentCase, string userQuestion, int questionNumber)
        {
            var q = new MysteryQuestion
            {
                QuestionNumber = questionNumber,
                QuestionText = userQuestion
            };

            bool match = false;
            foreach (var kw in currentCase.Keywords)
            {
                if (userQuestion.Contains(kw))
                {
                    match = true;
                    break;
                }
            }

            if (match)
            {
                q.AnswerType = MysteryAnswerType.Yes;
                q.AnswerMessage = "예! (핵심 관련 내용입니다)";
            }
            else
            {
                q.AnswerType = MysteryAnswerType.Irrelevant;
                q.AnswerMessage = "사건과 크게 관련이 없거나 중요하지 않습니다.";
            }

            return q;
        }

        private async Task<string> CallGeminiApiAsync(string systemInstruction, string userPrompt)
        {
            string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_apiKey}";

            var requestData = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[]
                        {
                            new { text = systemInstruction + "\n\n" + userPrompt }
                        }
                    }
                }
            };

            string jsonBody = _jsonSerializer.Serialize(requestData);
            byte[] bytes = Encoding.UTF8.GetBytes(jsonBody);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = bytes.Length;
            request.Timeout = 10000; // 10초 타임아웃

            using (Stream requestStream = await request.GetRequestStreamAsync())
            {
                await requestStream.WriteAsync(bytes, 0, bytes.Length);
            }

            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            using (Stream responseStream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(responseStream, Encoding.UTF8))
            {
                string jsonResponse = await reader.ReadToEndAsync();
                dynamic dict = _jsonSerializer.Deserialize<dynamic>(jsonResponse);

                try
                {
                    var candidates = dict["candidates"] as System.Collections.ArrayList;
                    if (candidates != null && candidates.Count > 0)
                    {
                        var first = candidates[0] as Dictionary<string, object>;
                        var content = first["content"] as Dictionary<string, object>;
                        var parts = content["parts"] as System.Collections.ArrayList;
                        var firstPart = parts[0] as Dictionary<string, object>;
                        return firstPart["text"].ToString();
                    }
                }
                catch { }

                return string.Empty;
            }
        }
    }
}
