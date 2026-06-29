using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GateHelper.LogValidator.Models;

namespace GateHelper.LogValidator.Core
{
    public class LogParser
    {
        private static readonly Regex _timeRegex = new Regex(
            @"\d{4}-\d{2}-\d{2}\s\d{2}:\d{2}:\d{2}\.\d{3}",
            RegexOptions.Compiled
        );

        public Task<List<RawLogModel>> ParseLogFileAsync(string filePath)
        {
            return Task.Run(() => ParseLogFile(filePath));
        }

        public List<RawLogModel> ParseLogFile(string filePath)
        {
            var list = new List<RawLogModel>();
            if (!File.Exists(filePath)) return list;

            // 💡 인코딩 자동 감지: BOM이 있으면 BOM 기준, 없으면 UTF-8로 시도
            Encoding encoding = DetectEncoding(filePath);
            string[] lines = File.ReadAllLines(filePath, encoding);
            string pureFileName = Path.GetFileNameWithoutExtension(filePath);

            int lineIndex = 1;                      // 💡 파싱된 라인만 카운트 → 연속 번호 보장
            DateTime lastParsedTime = DateTime.MinValue; // 💡 시간 파싱 실패 시 이어받을 기준값

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var model = ParseLine(line, lineIndex, pureFileName, lastParsedTime);

                // 💡 시간이 성공적으로 파싱된 경우에만 기준값 갱신
                // Structure 하위 줄처럼 시간이 없는 줄은 직전 기준값을 유지
                if (model.LogTime != lastParsedTime)
                    lastParsedTime = model.LogTime;

                list.Add(model);
                lineIndex++;
            }

            return list;
        }

        private RawLogModel ParseLine(string line, int lineNo, string logType, DateTime fallbackTime)
        {
            var model = new RawLogModel
            {
                LineNo = lineNo,
                LogMessage = line,
                LogType = logType,
                LogTime = fallbackTime  // 💡 파싱 실패 시 이전 라인 시간을 기본값으로 사용
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

        /// <summary>
        /// 파일 첫 4바이트의 BOM을 확인하여 인코딩을 자동으로 감지합니다.
        /// UTF-16 LE (FF FE), UTF-16 BE (FE FF), UTF-8 BOM (EF BB BF), 그 외는 UTF-8로 처리합니다.
        /// </summary>
        private static Encoding DetectEncoding(string filePath)
        {
            byte[] bom = new byte[4];
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                fs.Read(bom, 0, 4);
            }

            if (bom[0] == 0xFF && bom[1] == 0xFE) return Encoding.Unicode;        // UTF-16 LE
            if (bom[0] == 0xFE && bom[1] == 0xFF) return Encoding.BigEndianUnicode; // UTF-16 BE
            if (bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF) return Encoding.UTF8; // UTF-8 BOM

            return Encoding.UTF8; // 기본값
        }
    }
}
