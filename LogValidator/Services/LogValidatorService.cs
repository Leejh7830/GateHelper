using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GateHelper.LogValidator.Core;
using GateHelper.LogValidator.Models;

namespace GateHelper.LogValidator.Services
{
    /// <summary>
    /// ✅ [신규] LogValidatorForm에서 비즈니스 로직을 분리한 서비스 클래스.
    /// Form은 UI 표시와 사용자 입력만 담당하고, 실제 데이터 처리는 여기서 수행합니다.
    /// 덕분에 Form 코드가 절반 이하로 줄고, 이 클래스만 따로 테스트할 수 있습니다.
    /// </summary>
    public class LogValidatorService
    {
        private readonly LogParser _logParser = new LogParser();
        private readonly LogValidatorEngine _engine = new LogValidatorEngine();

        // ✅ 파일명 → LogType 매핑 테이블: Form 내부 if/else 체인을 데이터로 교체
        private static readonly Dictionary<string, string> _logTypeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "VARIABLE_TRACE", "PLC" },
            { "ERROR",          "ALARM" },
            { "ALARM",          "ALARM" },
            { "TRANSFER",       "TRANSFER" },
        };

        /// <summary>
        /// 드롭된 경로들(파일 or 폴더 혼합 가능)을 파싱하고 시간순 정렬된 통합 로그 리스트를 반환합니다.
        /// 폴더가 포함된 경우 내부 로그 파일(.log, .txt)을 재귀적으로 수집합니다.
        /// </summary>
        public async Task<List<RawLogModel>> LoadLogFilesAsync(IEnumerable<string> droppedPaths)
        {
            var allLogs = new List<RawLogModel>();

            // 폴더/파일 혼합 드롭 → 실제 파일 경로만 수집
            var filePaths = ResolveFilePaths(droppedPaths);

            foreach (string filePath in filePaths)
            {
                if (!File.Exists(filePath)) continue;

                var parsed = await _logParser.ParseLogFileAsync(filePath);
                string logType = ResolveLogType(Path.GetFileName(filePath));
                string sourceFileName = Path.GetFileName(filePath); // 소스 파일명 기록

                foreach (var log in parsed)
                {
                    log.LogType = logType;
                    log.SourceFileName = sourceFileName;
                }

                allLogs.AddRange(parsed);
            }

            // 💡 시간순 정렬 + 보조 기준 추가
            // 같은 시간대 로그가 여러 파일에 걸칠 때 SourceFileName → LineNo 순으로 안정적 정렬
            return allLogs
                .OrderBy(l => l.LogTime)
                .ThenBy(l => l.SourceFileName)
                .ThenBy(l => l.LineNo)
                .ToList();
        }

        /// <summary>
        /// 드롭된 경로 목록에서 실제 파일 경로만 추출합니다.
        /// 폴더이면 내부 .log/.txt 파일을 재귀 수집하고, 파일이면 그대로 사용합니다.
        /// </summary>
        private static IEnumerable<string> ResolveFilePaths(IEnumerable<string> droppedPaths)
        {
            var result = new List<string>();
            var supportedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".log", ".txt" };

            foreach (string path in droppedPaths)
            {
                if (Directory.Exists(path))
                {
                    // 폴더: 하위 파일 재귀 수집 (SearchOption.AllDirectories로 하위 폴더까지)
                    var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                        .Where(f => supportedExtensions.Contains(Path.GetExtension(f)));
                    result.AddRange(files);
                }
                else if (File.Exists(path) && supportedExtensions.Contains(Path.GetExtension(path)))
                {
                    result.Add(path);
                }
            }

            return result;
        }

        /// <summary>
        /// 체크된 시나리오들을 JSON에서 로드하고 검증을 실행합니다.
        /// </summary>
        public List<ScenarioEvaluator> RunValidation(
            List<RawLogModel> rawLogs,
            IEnumerable<ScenarioEvaluator> checkedEvaluators,
            string scenarioDirectory)
        {
            var targetEvaluators = new List<ScenarioEvaluator>();

            foreach (var eval in checkedEvaluators)
            {
                string fullPath = Path.Combine(scenarioDirectory, $"{eval.ScenarioName}.json");
                if (!File.Exists(fullPath)) continue;

                string json = File.ReadAllText(fullPath);
                var steps = JsonSerializer.Deserialize<List<ScenarioStepModel>>(json);
                if (steps == null) continue;

                eval.Steps = steps;
                eval.CurrentStepIndex = 0;
                eval.Status = EvaluationResultStatus.Ready;
                targetEvaluators.Add(eval);
            }

            return _engine.Validate(rawLogs, targetEvaluators);
        }

        // ✅ 파일명 → LogType 변환: 테이블 기반으로 if/else 체인 제거
        private static string ResolveLogType(string fileName)
        {
            string upper = fileName.ToUpper();
            foreach (var entry in _logTypeMap)
            {
                if (upper.Contains(entry.Key))
                    return entry.Value;
            }
            return "UNKNOWN";
        }
    }
}
