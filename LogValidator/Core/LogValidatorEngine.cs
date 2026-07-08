using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using GateHelper.LogValidator.Models;

namespace GateHelper.LogValidator.Core
{
    public class LogValidatorEngine
    {
        // 💡 [신규] 정규식 사전 컴파일 캐시 저장소
        private Dictionary<string, Regex> _regexCache;

        public List<ScenarioEvaluator> Validate(List<RawLogModel> rawLogs, List<ScenarioEvaluator> evaluators)
        {
            if (rawLogs == null || rawLogs.Count == 0 || evaluators == null || evaluators.Count == 0)
                return evaluators;

            // 💡 [수정] 검증 시작 전, 모든 시나리오의 정규식을 1회만 사전 조립 및 컴파일 캐싱
            _regexCache = new Dictionary<string, Regex>();
            foreach (var eval in evaluators)
            {
                foreach (var step in eval.Steps)
                {
                    if (!string.IsNullOrEmpty(step.MaskingPattern) && !_regexCache.ContainsKey(step.MaskingPattern))
                    {
                        string escaped = Regex.Escape(step.MaskingPattern);
                        string pat = $"^{escaped.Replace(@"\*", "(.*?)")}$";
                        _regexCache[step.MaskingPattern] = new Regex(pat, RegexOptions.IgnoreCase);
                    }
                }
            }

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
                ScenarioStepModel timeoutRef;
                if (ctx.ActiveGroupId > 0)
                {
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

                        // 💡 캐시된 정규식으로 첫 스텝 재검사
                        if (_regexCache.TryGetValue(ctx.Master.Steps[0].MaskingPattern, out Regex restartRx) && restartRx.IsMatch(log.LogMessage))
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

            // ── AND/OR 그룹 진행 중 ────────────────────────────────────
            if (ctx.ActiveGroupId > 0)
            {
                bool matchedAny = false;
                foreach (var pattern in ctx.PendingGroupPatterns.ToList())
                {
                    // 💡 캐시된 정규식 사용
                    if (_regexCache.TryGetValue(pattern.MaskingPattern, out Regex groupRx) && groupRx.IsMatch(log.LogMessage))
                    {
                        ctx.PendingGroupPatterns.Remove(pattern);
                        ctx.ActiveMatchedLines.Add((log.LineNo, log.SourceFileName));
                        ctx.LastMatchedTime = log.LogTime;
                        matchedAny = true;
                        break;
                    }
                }

                if (matchedAny)
                {
                    bool groupDone = false;

                    if (ctx.ActiveGroupType == "OR") groupDone = true;
                    else groupDone = ctx.PendingGroupPatterns.Count == 0;

                    if (groupDone)
                    {
                        ctx.ActiveGroupId = 0;
                        ctx.PendingGroupPatterns.Clear();
                        ctx.Master.CurrentStepIndex++;
                        if (ctx.Master.CurrentStepIndex >= ctx.Master.Steps.Count)
                            DumpSuccessCycle(ctx, log.LineNo, log.SourceFileName);
                    }
                    return;
                }

                // 💡 캐시된 정규식으로 첫 스텝 재검사
                if (_regexCache.TryGetValue(ctx.Master.Steps[0].MaskingPattern, out Regex restartRx2) && restartRx2.IsMatch(log.LogMessage))
                {
                    DumpFailedCycle(ctx, log.LineNo - 1, log.SourceFileName);
                    ctx.CurrentCycleStartLine = log.LineNo;
                    ctx.CurrentCycleStartSource = log.SourceFileName;
                    ctx.LastMatchedTime = log.LogTime;
                    ctx.ActiveMatchedLines.Add((log.LineNo, log.SourceFileName));
                    ctx.Master.CurrentStepIndex = 1;
                    ctx.ActiveGroupId = 0;
                    ctx.ActiveGroupType = "AND";
                    ctx.PendingGroupPatterns.Clear();
                }
                return;
            }

            // ── 일반 스텝 처리 ────────────────────────────────────────
            // 매번 BuildRegexPattern를 호출하지 않고 캐시에서 꺼내 씀
            if (_regexCache.TryGetValue(targetStep.MaskingPattern, out Regex targetRx) && targetRx.IsMatch(log.LogMessage))
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

                if (ctx.Master.CurrentStepIndex < ctx.Master.Steps.Count)
                {
                    var nextStep = ctx.Master.Steps[ctx.Master.CurrentStepIndex];
                    if (nextStep.GroupId > 0)
                    {
                        ctx.ActiveGroupId = nextStep.GroupId;
                        ctx.ActiveGroupType = nextStep.GroupType ?? "AND";
                        ctx.PendingGroupPatterns = ctx.Master.Steps.Where(s => s.GroupId == nextStep.GroupId).ToList();
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
                while (ctx.Master.CurrentStepIndex < ctx.Master.Steps.Count &&
                       ctx.Master.Steps[ctx.Master.CurrentStepIndex].IsOptional)
                {
                    var optStep = ctx.Master.Steps[ctx.Master.CurrentStepIndex];

                    if (_regexCache.TryGetValue(optStep.MaskingPattern, out Regex optRx) && optRx.IsMatch(log.LogMessage))
                    {
                        ctx.ActiveMatchedLines.Add((log.LineNo, log.SourceFileName));
                        ctx.LastMatchedTime = log.LogTime;
                        ctx.Master.CurrentStepIndex++;

                        if (ctx.Master.CurrentStepIndex >= ctx.Master.Steps.Count)
                            DumpSuccessCycle(ctx, log.LineNo, log.SourceFileName);
                        return;
                    }

                    ctx.Master.CurrentStepIndex++;

                    if (ctx.Master.CurrentStepIndex >= ctx.Master.Steps.Count)
                    {
                        DumpSuccessCycle(ctx, log.LineNo, log.SourceFileName);
                        return;
                    }

                    var nextOptStep = ctx.Master.Steps[ctx.Master.CurrentStepIndex];
                    if (_regexCache.TryGetValue(nextOptStep.MaskingPattern, out Regex nextOptRx) && nextOptRx.IsMatch(log.LogMessage))
                    {
                        ctx.ActiveMatchedLines.Add((log.LineNo, log.SourceFileName));
                        ctx.LastMatchedTime = log.LogTime;
                        ctx.Master.CurrentStepIndex++;

                        if (ctx.Master.CurrentStepIndex >= ctx.Master.Steps.Count)
                            DumpSuccessCycle(ctx, log.LineNo, log.SourceFileName);
                        return;
                    }
                }

                if (_regexCache.TryGetValue(ctx.Master.Steps[0].MaskingPattern, out Regex restartRx3) && restartRx3.IsMatch(log.LogMessage))
                {
                    DumpFailedCycle(ctx, log.LineNo - 1, log.SourceFileName);
                    ctx.CurrentCycleStartLine = log.LineNo;
                    ctx.CurrentCycleStartSource = log.SourceFileName;
                    ctx.LastMatchedTime = log.LogTime;
                    ctx.ActiveMatchedLines.Add((log.LineNo, log.SourceFileName));
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
            ctx.ActiveGroupType = "AND";
            ctx.PendingGroupPatterns.Clear();
            ctx.Master.CurrentStepIndex = 0;
        }

        private void DumpFailedCycle(ScenarioBuildContext ctx, int endLineNo, string endSource)
        {
            ctx.TotalCount++;

            string failMsg;
            if (ctx.ActiveGroupId > 0 && ctx.PendingGroupPatterns.Count > 0)
            {
                if (ctx.ActiveGroupType == "OR")
                {
                    var candidates = string.Join(", ", ctx.PendingGroupPatterns.Select(s => s.EventName));
                    failMsg = $"[OR GROUP] None of the signals received in group {ctx.ActiveGroupId}: {candidates}";
                }
                else
                {
                    var missing = string.Join(", ", ctx.PendingGroupPatterns.Select(s => s.EventName));
                    failMsg = $"[AND GROUP] Missing signal(s) in group {ctx.ActiveGroupId}: {missing}";
                }
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
            ctx.ActiveGroupType = "AND";
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
            ctx.ActiveGroupType = "AND";
            ctx.PendingGroupPatterns.Clear();
            ctx.Master.CurrentStepIndex = 0;
        }

        private class ScenarioBuildContext
        {
            public ScenarioEvaluator Master { get; }
            public int TotalCount { get; set; }
            public int SuccessCount { get; set; }
            public int CurrentCycleStartLine { get; set; }
            public string CurrentCycleStartSource { get; set; }
            public DateTime LastMatchedTime { get; set; } = DateTime.MinValue;

            public int ActiveGroupId { get; set; } = 0;
            public string ActiveGroupType { get; set; } = "AND";
            public List<ScenarioStepModel> PendingGroupPatterns { get; set; } = new List<ScenarioStepModel>();

            public List<(int LineNo, string SourceFileName)> ActiveMatchedLines { get; }
                = new List<(int, string)>();

            public ScenarioBuildContext(ScenarioEvaluator master) { Master = master; }
        }
    }
}