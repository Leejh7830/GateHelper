using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using GateHelper.LogValidator.Models;

namespace GateHelper.LogValidator.Core
{
    public class LogValidatorEngine
    {
        /// <summary>
        /// 단 1회의 로그 리스트 순회로 복수의 시나리오를 동시 검증하는 코어 엔진
        /// </summary>
        public List<ScenarioReportModel> Validate(List<RawLogModel> rawLogs, List<ScenarioEvaluator> evaluators)
        {
            var reports = new List<ScenarioReportModel>();
            if (rawLogs == null || rawLogs.Count == 0 || evaluators == null || evaluators.Count == 0)
                return reports;

            // 💡 [단일 패스 엔진 가동] 로그 파일의 첫 줄부터 마지막 줄까지 단 1회만 하향 순회 ($O(N)$)
            foreach (var log in rawLogs)
            {
                string currentLineText = log.LogMessage;
                int currentLineNo = log.LineNo;

                // 로그 한 줄당 등록된 모든 시나리오 상태 머신을 병렬로 검사
                foreach (var eval in evaluators)
                {
                    // 이미 성공했거나 실패로 확정된 시나리오는 연산에서 가드(Skip)
                    if (eval.IsCompleted || eval.Status == EvaluationResultStatus.FailedMissingStep)
                        continue;

                    eval.Status = EvaluationResultStatus.Processing;

                    // 현재 이 시나리오가 기다리고 있는 스텝의 마스킹 규칙 패턴 가져오기
                    var targetStep = eval.Steps[eval.CurrentStepIndex];
                    string pattern = targetStep.MaskingPattern;

                    // 💡 [정규식 변환 매니저 인터락] 
                    // 시나리오 자산의 '*' 기호를 무결한 정규식 매칭 와일드카드 패턴으로 치환
                    string regexPattern = BuildRegexPattern(pattern);

                    // 현재 읽은 로그 줄이 시나리오의 대기 스텝 규칙과 완벽히 일치하는지 판정
                    if (Regex.IsMatch(currentLineText, regexPattern))
                    {
                        // 일치 시 포인터를 다음 단계로 전진 (State Transition)
                        eval.CurrentStepIndex++;
                    }
                }
            }

            // 💡 [최종 상태 결과 덤프 및 리포트 모델 빌드]
            foreach (var eval in evaluators)
            {
                var report = new ScenarioReportModel { ScenarioName = eval.ScenarioName };

                if (eval.IsCompleted)
                {
                    eval.Status = EvaluationResultStatus.Success;
                    report.Status = "SUCCESS";
                    report.Progress = $"{eval.Steps.Count} / {eval.Steps.Count}";
                    report.Message = "모든 통신 시퀀스가 순정 로그 내에서 정상 검측되었습니다.";
                }
                else
                {
                    eval.Status = EvaluationResultStatus.FailedMissingStep;
                    eval.ErrorLineNo = rawLogs[rawLogs.Count - 1].LineNo; // 마지막 누락 지점 가드

                    report.Status = "FAILED";
                    report.Progress = $"{eval.CurrentStepIndex} / {eval.Steps.Count}";
                    report.Message = $"스텝 {eval.CurrentStepIndex + 1} ({eval.Steps[eval.CurrentStepIndex].EventName}) 누락 혹은 순서 이탈 불량 발생.";
                }

                reports.Add(report);
            }

            return reports;
        }

        /// <summary>
        /// 시나리오 내의 수동 마스킹 문자열을 분석기용 무결성 정규식으로 자동 변환
        /// </summary>
        private string BuildRegexPattern(string scenarioPattern)
        {
            // 1. 정규식 예약 특수문자 안전 이스케이프
            string escaped = Regex.Escape(scenarioPattern);

            // 2. 이스케이프된 \* 문자열을 공백을 포함해 탐욕적이지 않게 매칭하는 (.*?) 패턴으로 치환
            string regexPattern = escaped.Replace(@"\*", "(.*?)");

            // 3. 행의 시작과 끝을 명시하여 부분 일치가 아닌 한 줄 전체 일치를 강제
            return $"^{regexPattern}$";
        }
    }
}