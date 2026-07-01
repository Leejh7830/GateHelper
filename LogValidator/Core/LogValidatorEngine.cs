using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using GateHelper.LogValidator.Models;

namespace GateHelper.LogValidator.Core
{
    public class LogValidatorEngine
    {
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

            foreach (var log in rawLogs)
            {
                foreach (var ctx in engineContexts)
                    ProcessLog(ctx, log);
            }

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
            if (ctx.Master.CurrentStepIndex >= ctx.Master.Steps.Count) return;

            var targetStep = ctx.Master.Steps[ctx.Master.CurrentStepIndex];

            // ── 타임아웃 체크 ──────────────────────────────────────────
            if (ctx.Master.CurrentStepIndex > 0 && ctx.LastMatchedTime != DateTime.MinValue)
            {
                // AND 그룹 진행 중이면 그룹의 첫 스텝 기준 Timeout 사용
                ScenarioStepModel timeoutRef;
                if (ctx.ActiveGroupId > 0)
                {
                    // AND 그룹 시작 직전 스텝의 Timeout
                    int groupFirstIdx = ctx.Master.Steps.FindIndex(s => s.GroupId == ctx.ActiveGroupId);
                    timeoutRef = groupFirstIdx > 0 ? ctx.Master.Steps[groupFirstIdx - 1] : null;
                }
                else
                {
                    timeoutRef = ctx.Master.Steps[ctx.Master.CurrentStepIndex - 1];
                }

                if (timeoutRef != null && timeoutRef.TimeoutSeconds > 0)
                {
                    double elapsed = (log.LogTime - ctx.LastMatchedTime).TotalSeconds;
                    if (elapsed >= 0 && elapsed > timeoutRef.TimeoutSeconds)
                    {
                        DumpTimeoutCycle(ctx, log.LineNo, log.SourceFileName, timeoutRef, elapsed);

                        string restartPat = BuildRegexPattern(ctx.Master.Steps[0].MaskingPattern);
                        if (Regex.IsMatch(log.LogMessage, restartPat))
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

            // ── AND 그룹 진행 중 ───────────────────────────────────────
            // 현재 대기 중인 스텝이 AND 그룹에 속하면, 그룹 내 남은 패턴 중 하나라도 매칭되면 제거
            if (ctx.ActiveGroupId > 0)
            {
                bool matchedAny = false;
                foreach (var pattern in ctx.PendingGroupPatterns.ToList())
                {
                    if (Regex.IsMatch(log.LogMessage, BuildRegexPattern(pattern.MaskingPattern)))
                    {
                        ctx.PendingGroupPatterns.Remove(pattern);
                        ctx.ActiveMatchedLines.Add((log.LineNo, log.SourceFileName));
                        ctx.LastMatchedTime = log.LogTime;
                        matchedAny = true;
                        break; // 한 로그 당 하나의 패턴만 소비
                    }
                }

                if (matchedAny)
                {
                    // 그룹 내 모든 패턴 수신 완료 → 다음 스텝으로
                    if (ctx.PendingGroupPatterns.Count == 0)
                    {
                        ctx.ActiveGroupId = 0;
                        ctx.Master.CurrentStepIndex++;
                        if (ctx.Master.CurrentStepIndex >= ctx.Master.Steps.Count)
                            DumpSuccessCycle(ctx, log.LineNo, log.SourceFileName);
                    }
                    return;
                }

                // AND 그룹 진행 중 첫 스텝이 재등장 → 그룹 실패, 새 사이클 시작
                string restartPat2 = BuildRegexPattern(ctx.Master.Steps[0].MaskingPattern);
                if (Regex.IsMatch(log.LogMessage, restartPat2))
                {
                    DumpFailedCycle(ctx, log.LineNo - 1, log.SourceFileName);
                    ctx.CurrentCycleStartLine = log.LineNo;
                    ctx.CurrentCycleStartSource = log.SourceFileName;
                    ctx.LastMatchedTime = log.LogTime;
                    ctx.Master.CurrentStepIndex = 1;
                    ctx.ActiveGroupId = 0;
                    ctx.PendingGroupPatterns.Clear();
                }
                return;
            }

            // ── 일반 스텝 처리 ────────────────────────────────────────
            string regexPattern = BuildRegexPattern(targetStep.MaskingPattern);

            if (Regex.IsMatch(log.LogMessage, regexPattern))
            {
                if (ctx.Master.CurrentStepIndex == 0)
                {
                    ctx.CurrentCycleStartLine = log.LineNo;
                    ctx.CurrentCycleStartSource = log.SourceFileName;
                    ctx.ActiveMatchedLines.Clear();
                }

                ctx.ActiveMatchedLines.Add((log.LineNo, log.SourceFileName));
                ctx.LastMatchedTime = log.LogTime;
                ctx.Master.CurrentStepIndex++;

                // 💡 다음 스텝이 AND 그룹이면 그룹 모드 진입
                if (ctx.Master.CurrentStepIndex < ctx.Master.Steps.Count)
                {
                    var nextStep = ctx.Master.Steps[ctx.Master.CurrentStepIndex];
                    if (nextStep.GroupId > 0)
                    {
                        ctx.ActiveGroupId = nextStep.GroupId;
                        ctx.PendingGroupPatterns = ctx.Master.Steps
                            .Where(s => s.GroupId == nextStep.GroupId)
                            .ToList();
                        // AND 그룹의 스텝 인덱스는 그룹 마지막 스텝 다음으로 건너뜀
                        int lastGroupIdx = ctx.Master.Steps.FindLastIndex(s => s.GroupId == nextStep.GroupId);
                        ctx.Master.CurrentStepIndex = lastGroupIdx + 1;
                        return;
                    }
                }

                if (ctx.Master.CurrentStepIndex >= ctx.Master.Steps.Count)
                    DumpSuccessCycle(ctx, log.LineNo, log.SourceFileName);
            }
            else if (ctx.Master.CurrentStepIndex > 0)
            {
                // 💡 Optional 스텝 처리
                while (ctx.Master.CurrentStepIndex < ctx.Master.Steps.Count &&
                       ctx.Master.Steps[ctx.Master.CurrentStepIndex].IsOptional)
                {
                    ctx.Master.CurrentStepIndex++;

                    if (ctx.Master.CurrentStepIndex >= ctx.Master.Steps.Count)
                    {
                        DumpSuccessCycle(ctx, log.LineNo, log.SourceFileName);
                        return;
                    }

                    string nextPat = BuildRegexPattern(ctx.Master.Steps[ctx.Master.CurrentStepIndex].MaskingPattern);
                    if (Regex.IsMatch(log.LogMessage, nextPat))
                    {
                        ctx.ActiveMatchedLines.Add((log.LineNo, log.SourceFileName));
                        ctx.LastMatchedTime = log.LogTime;
                        ctx.Master.CurrentStepIndex++;

                        if (ctx.Master.CurrentStepIndex >= ctx.Master.Steps.Count)
                            DumpSuccessCycle(ctx, log.LineNo, log.SourceFileName);
                        return;
                    }
                }

                // 첫 스텝 재등장 → 기존 사이클 실패, 새 사이클 시작
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
            ctx.ActiveGroupId = 0;
            ctx.PendingGroupPatterns.Clear();
            ctx.Master.CurrentStepIndex = 0;
        }

        private void DumpFailedCycle(ScenarioBuildContext ctx, int endLineNo, string endSource)
        {
            ctx.TotalCount++;

            string failMsg;
            if (ctx.ActiveGroupId > 0 && ctx.PendingGroupPatterns.Count > 0)
            {
                // AND 그룹 실패 — 어떤 신호가 미수신인지 표시
                var missing = string.Join(", ", ctx.PendingGroupPatterns.Select(s => s.EventName));
                failMsg = $"[AND GROUP] Missing signal(s) in group {ctx.ActiveGroupId}: {missing}";
            }
            else
            {
                var missingStep = ctx.Master.Steps[Math.Min(ctx.Master.CurrentStepIndex, ctx.Master.Steps.Count - 1)];
                failMsg = $"Step {ctx.Master.CurrentStepIndex + 1} ({missingStep.EventName}) missing or out of order.";
            }

            ctx.Master.StepReports.Add(new StepValidationReport
            {
                StepDisplayHeader = $"❌ Cycle {ctx.TotalCount} (Line {ctx.CurrentCycleStartLine} ~ {endLineNo})",
                StepStatus = "FAILED",
                StepProgress = $"{ctx.Master.CurrentStepIndex} / {ctx.Master.Steps.Count}",
                StepMessage = failMsg,
                StartLineNo = ctx.CurrentCycleStartLine,
                StartSourceFileName = ctx.CurrentCycleStartSource,
                MatchedLineNumbers = new List<(int, string)>(ctx.ActiveMatchedLines)
            });

            ctx.ActiveMatchedLines.Clear();
            ctx.LastMatchedTime = DateTime.MinValue;
            ctx.ActiveGroupId = 0;
            ctx.PendingGroupPatterns.Clear();
            ctx.Master.CurrentStepIndex = 0;
        }

        private void DumpTimeoutCycle(ScenarioBuildContext ctx, int endLineNo, string endSource,
            ScenarioStepModel timedOutStep, double elapsedSeconds)
        {
            ctx.TotalCount++;
            var nextStep = ctx.Master.Steps[Math.Min(ctx.Master.CurrentStepIndex, ctx.Master.Steps.Count - 1)];

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
            ctx.ActiveGroupId = 0;
            ctx.PendingGroupPatterns.Clear();
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
            public DateTime LastMatchedTime { get; set; } = DateTime.MinValue;

            // 💡 AND 그룹 처리용 상태
            // ActiveGroupId > 0이면 해당 그룹 내 신호를 순서 무관 대기 중
            public int ActiveGroupId { get; set; } = 0;
            public List<ScenarioStepModel> PendingGroupPatterns { get; set; } = new List<ScenarioStepModel>();

            public List<(int LineNo, string SourceFileName)> ActiveMatchedLines { get; }
                = new List<(int, string)>();

            public ScenarioBuildContext(ScenarioEvaluator master) { Master = master; }
        }
    }
}
