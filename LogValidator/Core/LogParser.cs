using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;
using GateHelper.LogValidator.Models; // 💡 상위 레이어의 RawLogModel을 참조하기 위해 반드시 필요합니다.

namespace GateHelper.LogValidator.Core
{
    public class LogParser
    {
        // 💡 [전역 타임스탬프 스캐너 인터락] 
        // 문자열 내 위치를 불문하고 yyyy-MM-dd HH:mm:ss.fff 서식을 무결하게 도려내는 정규식 락인
        private static readonly Regex _timeRegex = new Regex(
            @"\d{4}-\d{2}-\d{2}\s\d{2}:\d{2}:\d{2}\.\d{3}",
            RegexOptions.Compiled
        );

        public List<RawLogModel> ParseLogFile(string filePath)
        {
            var list = new List<RawLogModel>();

            if (!File.Exists(filePath))
                return list;

            try
            {
                // 확장자를 걷어낸 파일명 자체를 순정 식별 코드로 바인딩 (자율 책임 표시제)
                string pureFileName = Path.GetFileNameWithoutExtension(filePath);
                string[] lines = File.ReadAllLines(filePath);
                int currentLineIndex = 1;

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        currentLineIndex++;
                        continue;
                    }

                    var model = new RawLogModel();
                    model.LineNo = currentLineIndex;
                    model.LogMessage = line;
                    model.LogType = pureFileName;
                    model.LogTime = DateTime.Now; // 파싱 실패 시 튕김을 막는 최소한의 런타임 가드

                    // 💡 [전역 위치 스캐닝] 
                    // 시간이 대괄호 안에 있든, 문장 중간에 생뚱맞게 처박혀 있든 탐색 성공 처리
                    Match match = _timeRegex.Match(line);

                    if (match.Success)
                    {
                        string extractedTimeStr = match.Value;

                        // 도려낸 23자리 텍스트 스트림을 순정 DateTime 자원으로 안전 변환
                        if (DateTime.TryParseExact(
                                extractedTimeStr,
                                "yyyy-MM-dd HH:mm:ss.fff",
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.None,
                                out DateTime parsedDate))
                        {
                            model.LogTime = parsedDate;
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[Time Pattern Missing] File: {pureFileName}, Line {currentLineIndex}");
                    }

                    list.Add(model);
                    currentLineIndex++;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Parser Critical Error] {ex.Message}");
                throw;
            }

            return list;
        }
    }
}