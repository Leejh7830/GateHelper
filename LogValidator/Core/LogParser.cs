using System;
using System.Collections.Generic;
using System.IO;
using GateHelper.LogValidator.Models;

namespace GateHelper.LogValidator.Core
{
    public class LogParser
    {
        public List<RawLogModel> ParseLogFile(string filePath)
        {
            var logList = new List<RawLogModel>();

            if (!File.Exists(filePath)) return logList;

            try
            {
                // 💡 파일의 모든 행을 원본 유실 없이 순정 상태로 판독
                string[] allLines = File.ReadAllLines(filePath);

                for (int i = 0; i < allLines.Length; i++)
                {
                    string currentLine = allLines[i];

                    // 💡 공백 유무에 따른 데이터 누락을 완전 방지
                    if (string.IsNullOrEmpty(currentLine)) continue;

                    // 💡 [아키텍처 정렬] 타임스탬프 분절 엔진을 완전히 폐기하고
                    // LogMessage 필드에 한 줄 전체 내용을 훼손 없이 통째로 덤프합니다.
                    logList.Add(new RawLogModel
                    {
                        LineNo = i + 1,
                        Timestamp = "*", // 스펙 변경으로 타임스탬프 단독 컬럼은 마스킹 기본값 처리
                        LogMessage = currentLine
                    });
                }
            }
            catch (Exception)
            {
                // 폼 레이어의 예외 포착 스위치로 제어권을 위임하기 위해 상위로 throw
                throw;
            }

            return logList;
        }
    }
}