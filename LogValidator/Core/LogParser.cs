using GateHelper.LogValidator.Models;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GateHelper.LogValidator.Core
{
    public class LogParser
    {
        private static readonly Regex _timeRegex = new Regex(
            @"\d{4}-\d{2}-\d{2}\s\d{2}:\d{2}:\d{2}\.\d{3}",
            RegexOptions.Compiled
        );

        public Task<List<RawLogModel>> ParseLogFileAsync(string filePath, string logType, string sourceFileName)
        {
            return Task.Run(() => ParseLogFile(filePath, logType, sourceFileName));
        }

        public List<RawLogModel> ParseLogFile(string filePath, string logType, string sourceFileName)
        {
            var list = new List<RawLogModel>();
            if (!File.Exists(filePath)) return list;

            Encoding encoding = DetectEncoding(filePath);
            IEnumerable<string> lines = File.ReadLines(filePath, encoding);

            int lineIndex = 1;
            DateTime lastParsedTime = DateTime.MinValue;

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                // 💡 억지로 추출하던 파일명 로직을 삭제하고 파라미터로 받은 값을 그대로 릴레이
                var model = ParseLine(line, lineIndex, logType, lastParsedTime, sourceFileName);

                if (model.LogTime != lastParsedTime)
                    lastParsedTime = model.LogTime;

                list.Add(model);
                lineIndex++;
            }

            return list;
        }

        private RawLogModel ParseLine(string line, int lineNo, string logType, DateTime fallbackTime, string sourceFileName)
        {
            // 💡 private set으로 닫힌 속성들은 생성자를 통해서 단 한 번만 주입됩니다.
            var model = new RawLogModel(lineNo, line, logType, sourceFileName)
            {
                LogTime = fallbackTime
            };

            Match match = _timeRegex.Match(line);
            if (match.Success &&
                DateTime.TryParseExact(
                    match.Value,
                    "yyyy-MM-dd HH:mm:ss.fff",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime parsed))
            {
                model.LogTime = parsed;
            }

            return model;
        }

        private static Encoding DetectEncoding(string filePath)
        {
            byte[] bom = new byte[4];
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                fs.Read(bom, 0, 4);
            }

            if (bom[0] == 0xFF && bom[1] == 0xFE) return Encoding.Unicode;
            if (bom[0] == 0xFE && bom[1] == 0xFF) return Encoding.BigEndianUnicode;
            if (bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF) return Encoding.UTF8;

            return Encoding.UTF8;
        }
    }
}