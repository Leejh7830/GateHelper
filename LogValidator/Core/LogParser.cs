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
            string[] allLines = File.ReadAllLines(filePath);

            for (int i = 0; i < allLines.Length; i++)
            {
                string currentLine = allLines[i];
                string timestamp = "*";

                // 시간 문자열 수동 스플릿 엔진 가동
                if (currentLine.Length > 23 && currentLine.Contains("-") && currentLine.Contains(":"))
                {
                    int firstSpaceIndex = currentLine.IndexOf(' ', currentLine.IndexOf(' ') + 1);
                    if (firstSpaceIndex > 0)
                    {
                        timestamp = currentLine.Substring(0, firstSpaceIndex).Trim();
                        currentLine = currentLine.Substring(firstSpaceIndex).Trim();
                    }
                }

                logList.Add(new RawLogModel
                {
                    LineNo = i + 1,
                    Timestamp = timestamp,
                    LogMessage = currentLine
                });
            }

            return logList;
        }
    }
}