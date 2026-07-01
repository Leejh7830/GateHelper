using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using GateHelper.LogValidator.Models;

namespace GateHelper.LogValidator.Core
{
    public class LogValidatorEngine
    {
        /// <summary>
        /// 💡 [전수 전사 통계 엔진] 단 1회의 로그 순회로 모든 시나리오의 다중 발생 주기 및 유실 사이클을 전수 추적
        /// </summary>
        public List<ScenarioEvaluator> Validate(List<RawLogModel> rawLogs, List<ScenarioEvaluator> evaluators)
        {
            if (rawLogs == null || rawLogs.Count == 0 || evaluators == null || evaluators.Count == 0)
                return evaluators;

            var engineContexts = new List<ScenarioBuildContext>();
            foreach (var eval in evaluators)
            {
                eval.CurrentStepIndex = 0;
                eval.StepReports.Clear();
                engineContexts.Add(new ScenarioBuildContext(eval));
            }

            // 💡 [단일 패스 루프] 로그 전체 1회 순회 O(N)
            foreach (var log in rawLogs)
            {
                foreach (var ctx in engineContexts)
                {
                    ProcessLog(ctx, log);
                }
            }

            // 💡 [로그 종료 가드] 끝까지 읽었는데 진행 중인 잔여 사이클 → 실패 처리
            var lastLog = rawLogs[rawLogs.Count - 1];
            foreach (var ctx in engineContexts)
            {
                if (ctx.Master.CurrentStepIndex > 0)
                    DumpFailedCycle(ctx, lastLog.LineNo, lastLog.SourceFileName);

                ctx.Master.Status = (ctx.TotalCount > 0 && ctx.SuccessCount == ctx.TotalCount)
                    ? EvaluationResultStatus.SUCCESS
                    : EvaluationResultStatus.FAILED;

                ctx.Master.Progress = $"{ctx.SuccessCount} / {ctx.TotalCount} PASSED";
                ctx.Master.Message = $"Total {ctx.TotalCount} cycle(s) detected in log.";
            }

            return evaluators;
        }

        private void ProcessLog(ScenarioBuildContext ctx, RawLogModel log)
        {
            var targetStep = ctx.Master.Steps[ctx.Master.CurrentStepIndex];
            string regexPattern = BuildRegexPattern(targetStep.MaskingPattern);

            // 💡 타임아웃 체크: 이전 스텝 매칭 후 현재 스텝까지 허용 시간 초과 여부 확인
            // (사이클 진행 중이고, 이전 스텝에 타임아웃이 설정된 경우에만 검사)
            if (ctx.Master.CurrentStepIndex > 0 && ctx.LastMatchedTime != DateTime.MinValue)
            {
                var prevStep = ctx.Master.Steps[ctx.Master.CurrentStepIndex - 1];
                if (prevStep.TimeoutSeconds > 0)
                {
                    double elapsed = (log.LogTime - ctx.LastMatchedTime).TotalSeconds;

                    // 💡 음수 방어: 로그 시간 역전(병합 오류) 또는 파싱 실패 시 타임아웃 체크 스킵
                    if (elapsed >= 0 && elapsed > prevStep.TimeoutSeconds)
                    {
                        // 타임아웃 초과 → 현재 사이클 실패 처리
                        DumpTimeoutCycle(ctx, log.LineNo, log.SourceFileName, prevStep, elapsed);

                        // 이 로그가 첫 스텝과 일치하면 새 사이클로 즉시 전환
                        string restartPattern = BuildRegexPattern(ctx.Master.Steps[0].MaskingPattern);
                        if (Regex.IsMatch(log.LogMessage, restartPattern))
                        {
                            ctx.CurrentCycleStartLine = log.LineNo;
                            ctx.CurrentCycleStartSource = log.SourceFileName;
                            ctx.LastMatchedTime = log.LogTime;
                            ctx.ActiveMatchedLines.Add((log.LineNo, log.SourceFileName));
                            ctx.Master.CurrentStepIndex = 1;
                        }
                        return;
                    }
                }
            }

            if (Regex.IsMatch(log.LogMessage, regexPattern))
            {
                // 💡 새 사이클 시작 시 시작 위치(LineNo + SourceFileName) 기록
                if (ctx.Master.CurrentStepIndex == 0)
                {
                    ctx.CurrentCycleStartLine = log.LineNo;
                    ctx.CurrentCycleStartSource = log.SourceFileName;
                    ctx.ActiveMatchedLines.Clear();
                }

                ctx.ActiveMatchedLines.Add((log.LineNo, log.SourceFileName));
                ctx.LastMatchedTime = log.LogTime;
                ctx.Master.CurrentStepIndex++;

                if (ctx.Master.CurrentStepIndex >= ctx.Master.Steps.Count)
                    DumpSuccessCycle(ctx, log.LineNo, log.SourceFileName);
            }
            else if (ctx.Master.CurrentStepIndex > 0)
            {
                // 💡 Optional 스텝 처리: 현재 대기 중인 스텝이 Optional이면 스킵하고 다음 스텝과 재비교
                while (ctx.Master.CurrentStepIndex < ctx.Master.Steps.Count &&
                       ctx.Master.Steps[ctx.Master.CurrentStepIndex].IsOptional)
                {
                    ctx.Master.CurrentStepIndex++;

                    // Optional 스텝을 건너뛴 후 마지막 스텝까지 도달하면 SUCCESS
                    if (ctx.Master.CurrentStepIndex >= ctx.Master.Steps.Count)
                    {
                        DumpSuccessCycle(ctx, log.LineNo, log.SourceFileName);
                        return;
                    }

                    // 스킵 후 현재 로그가 다음 스텝과 일치하는지 재비교
                    string nextPattern = BuildRegexPattern(ctx.Master.Steps[ctx.Master.CurrentStepIndex].MaskingPattern);
                    if (Regex.IsMatch(log.LogMessage, nextPattern))
                    {
                        ctx.ActiveMatchedLines.Add((log.LineNo, log.SourceFileName));
                        ctx.LastMatchedTime = log.LogTime;
                        ctx.Master.CurrentStepIndex++;

                        if (ctx.Master.CurrentStepIndex >= ctx.Master.Steps.Count)
                            DumpSuccessCycle(ctx, log.LineNo, log.SourceFileName);
                        return;
                    }
                }

                // 진행 중인 사이클이 있는데 첫 스텝이 다시 나타나면 → 기존 사이클 실패, 새 사이클 시작
                string restartPattern = BuildRegexPattern(ctx.Master.Steps[0].MaskingPattern);
                if (Regex.IsMatch(log.LogMessage, restartPattern))
                {
                    DumpFailedCycle(ctx, log.LineNo - 1, log.SourceFileName);
                    ctx.CurrentCycleStartLine = log.LineNo;
                    ctx.CurrentCycleStartSource = log.SourceFileName;
                    ctx.LastMatchedTime = log.LogTime;
                    ctx.Master.CurrentStepIndex = 1;
                }
            }
        }

        private void DumpSuccessCycle(ScenarioBuildContext ctx, int endLineNo, string endSource)
        {
            ctx.TotalCount++;
            ctx.SuccessCount++;

            ctx.Master.StepReports.Add(new StepValidationReport
            {
                StepDisplayHeader = $"🔄 Cycle {ctx.TotalCount} (Line {ctx.CurrentCycleStartLine} ~ {endLineNo})",
                StepStatus = "SUCCESS",
                StepProgress = $"{ctx.Master.Steps.Count} / {ctx.Master.Steps.Count}",
                StepMessage = "All steps completed successfully.",
                StartLineNo = ctx.CurrentCycleStartLine,
                StartSourceFileName = ctx.CurrentCycleStartSource,
                MatchedLineNumbers = new List<(int, string)>(ctx.ActiveMatchedLines)
            });

            ctx.ActiveMatchedLines.Clear();
            ctx.LastMatchedTime = DateTime.MinValue;
            ctx.Master.CurrentStepIndex = 0;
        }

        private void DumpFailedCycle(ScenarioBuildContext ctx, int endLineNo, string endSource)
        {
            ctx.TotalCount++;
            var missingStep = ctx.Master.Steps[ctx.Master.CurrentStepIndex];

            ctx.Master.StepReports.Add(new StepValidationReport
            {
                StepDisplayHeader = $"❌ Cycle {ctx.TotalCount} (Line {ctx.CurrentCycleStartLine} ~ {endLineNo})",
                StepStatus = "FAILED",
                StepProgress = $"{ctx.Master.CurrentStepIndex} / {ctx.Master.Steps.Count}",
                StepMessage = $"Step {ctx.Master.CurrentStepIndex + 1} ({missingStep.EventName}) missing or out of order.",
                StartLineNo = ctx.CurrentCycleStartLine,
                StartSourceFileName = ctx.CurrentCycleStartSource,
                MatchedLineNumbers = new List<(int, string)>(ctx.ActiveMatchedLines)
            });

            ctx.ActiveMatchedLines.Clear();
            ctx.LastMatchedTime = DateTime.MinValue;
            ctx.Master.CurrentStepIndex = 0;
        }

        // 💡 타임아웃 초과 실패 - 어느 스텝에서 몇 초 초과했는지 메시지에 명시
        private void DumpTimeoutCycle(ScenarioBuildContext ctx, int endLineNo, string endSource,
            ScenarioStepModel timedOutStep, double elapsedSeconds)
        {
            ctx.TotalCount++;
            var nextStep = ctx.Master.Steps[ctx.Master.CurrentStepIndex];

            ctx.Master.StepReports.Add(new StepValidationReport
            {
                StepDisplayHeader = $"⏱ Cycle {ctx.TotalCount} (Line {ctx.CurrentCycleStartLine} ~ {endLineNo})",
                StepStatus = "FAILED",
                StepProgress = $"{ctx.Master.CurrentStepIndex} / {ctx.Master.Steps.Count}",
                StepMessage = $"[TIMEOUT] {timedOutStep.EventName}: {elapsedSeconds:F1}s exceeded " +
                              $"(allowed: {timedOutStep.TimeoutSeconds}s) — {nextStep.EventName} not received.",
                StartLineNo = ctx.CurrentCycleStartLine,
                StartSourceFileName = ctx.CurrentCycleStartSource,
                MatchedLineNumbers = new List<(int, string)>(ctx.ActiveMatchedLines)
            });

            ctx.ActiveMatchedLines.Clear();
            ctx.LastMatchedTime = DateTime.MinValue;
            ctx.Master.CurrentStepIndex = 0;
        }

        private static string BuildRegexPattern(string scenarioPattern)
        {
            if (string.IsNullOrEmpty(scenarioPattern)) return string.Empty;
            string escaped = Regex.Escape(scenarioPattern);
            return $"^{escaped.Replace(@"\*", "(.*?)")}$";
        }

        private class ScenarioBuildContext
        {
            public ScenarioEvaluator Master { get; }
            public int TotalCount { get; set; }
            public int SuccessCount { get; set; }
            public int CurrentCycleStartLine { get; set; }
            public string CurrentCycleStartSource { get; set; }
            public DateTime LastMatchedTime { get; set; } = DateTime.MinValue; // 💡 타임아웃 계산 기준점

            // 💡 (LineNo, SourceFileName) 쌍으로 저장 - 파일 간 LineNo 충돌 방지
            public List<(int LineNo, string SourceFileName)> ActiveMatchedLines { get; } = new List<(int, string)>();

            public ScenarioBuildContext(ScenarioEvaluator master) { Master = master; }
        }
    }
}
