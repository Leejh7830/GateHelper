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

            // 각 시나리오별 통계 관리를 위한 백엔드 가드 사전 빌드
            var engineContexts = new List<ScenarioBuildContext>();
            foreach (var eval in evaluators)
            {
                eval.CurrentStepIndex = 0;
                eval.StepReports.Clear();

                engineContexts.Add(new ScenarioBuildContext(eval));
            }

            // 💡 [단일 패스 루프 가동] 로그 파일 전체 영역 1회 하향 탐색 ($O(N)$)
            foreach (var log in rawLogs)
            {
                string currentLineText = log.LogMessage;
                int currentLineNo = log.LineNo;

                foreach (var ctx in engineContexts)
                {
                    // 1단계 규칙 매칭 검사
                    var targetStep = ctx.Master.Steps[ctx.Master.CurrentStepIndex];
                    string regexPattern = BuildRegexPattern(targetStep.MaskingPattern);

                    // 현재 읽은 로그 라인이 대기 중인 시나리오 스텝 규칙과 일치하는지 판정
                    if (Regex.IsMatch(currentLineText, regexPattern))
                    {
                        // 💡 [무결성 가드 1] 최초 첫 스텝 진입 시 시작 라인 선점
                        if (ctx.Master.CurrentStepIndex == 0)
                        {
                            ctx.CurrentCycleStartLine = currentLineNo;
                            ctx.ActiveMatchedLines.Clear(); // 👈 새 바구니 준비
                        }

                        // 💡 [무결성 가드 2: 위치 고정]
                        // 포인터 전진이나 다른 서브 루프 연산에 간섭받지 않도록 
                        // 성공한 라인 번호를 바구니에 인입하는 코드를 최상단에 완전 고정합니다.
                        ctx.ActiveMatchedLines.Add(currentLineNo);

                        // 라인 번호 저장이 안전하게 끝난 후 포인터 전진
                        ctx.Master.CurrentStepIndex++;

                        // [성공 사이클 포착] 시나리오의 최종 단계까지 누락 없이 도달한 경우
                        if (ctx.Master.CurrentStepIndex >= ctx.Master.Steps.Count)
                        {
                            ctx.TotalCount++;
                            ctx.SuccessCount++;

                            ctx.Master.StepReports.Add(new StepValidationReport
                            {
                                StepDisplayHeader = $"🔄 Cycle {ctx.TotalCount} (Line {ctx.CurrentCycleStartLine} ~ {currentLineNo})",
                                StepStatus = "SUCCESS",
                                StepProgress = $"{ctx.Master.Steps.Count} / {ctx.Master.Steps.Count}",
                                StepMessage = "해당 회차의 모든 통신 시퀀스가 무결하게 검측정 완료되었습니다.",

                                StartLineNo = ctx.CurrentCycleStartLine,
                                // 현재까지 수집된 라인 번호 컬렉션을 무결하게 이관
                                MatchedLineNumbers = new List<int>(ctx.ActiveMatchedLines)
                            });

                            ctx.ActiveMatchedLines.Clear(); // 마감 클리어
                            ctx.Master.CurrentStepIndex = 0;
                        }
                    }
                    else
                    {
                        // 💡 [불량 사이클 포착 - 시퀀스 순서 이탈 가드]
                        // 첫 번째 스텝을 찾아서 달리고 있는 상태가 아니라 이미 공정이 진행 중인데, 
                        // 다음 필수 스텝 규칙이 와야 할 타이틀에 엉뚱한 로그가 섞여서 시퀀스가 깨진 경우 처리
                        if (ctx.Master.CurrentStepIndex > 0)
                        {
                            // 만약 이 엉뚱한 로그가 하필이면 새로운 1번째 스텝 규칙과 매치된다면?
                            // 기존 진행하던 사이클은 실패 처리하고, 이 라인부터 새로운 사이클로 전환시킵니다.
                            string restartPattern = BuildRegexPattern(ctx.Master.Steps[0].MaskingPattern);
                            if (Regex.IsMatch(currentLineText, restartPattern))
                            {
                                DumpFailedCycle(ctx, currentLineNo - 1); // 직전 줄까지 실패 처리 덤프

                                // 새 사이클 즉시 개시
                                ctx.CurrentCycleStartLine = currentLineNo;
                                ctx.Master.CurrentStepIndex = 1;
                            }
                        }
                    }
                }
            }

            // 💡 [로그 종료 시점 가드] 끝까지 읽었는데 중간 단계에 멈춰있는 잔여 사이클 최종 실패 처리
            foreach (var ctx in engineContexts)
            {
                if (ctx.Master.CurrentStepIndex > 0)
                {
                    DumpFailedCycle(ctx, rawLogs[rawLogs.Count - 1].LineNo);
                }

                // 💡 [최종 부모 노드 마스터 통계 데이터 셋업]
                if (ctx.TotalCount > 0 && ctx.SuccessCount == ctx.TotalCount)
                {
                    ctx.Master.Status = EvaluationResultStatus.SUCCESS;
                }
                else
                {
                    ctx.Master.Status = EvaluationResultStatus.FAILED;
                }

                ctx.Master.Progress = $"성공 {ctx.SuccessCount}건 / 총 {ctx.TotalCount}건";
                ctx.Master.Message = $"로그 전체 영역 내에서 총 {ctx.TotalCount}회 발생 시퀀스가 검측정되었습니다.";
            }

            return evaluators;
        }

        private void DumpFailedCycle(ScenarioBuildContext ctx, int endLineNo)
        {
            ctx.TotalCount++;
            var missingStep = ctx.Master.Steps[ctx.Master.CurrentStepIndex];

            ctx.Master.StepReports.Add(new StepValidationReport
            {
                StepDisplayHeader = $"❌ Cycle {ctx.TotalCount} (Line {ctx.CurrentCycleStartLine} ~ {endLineNo})",
                StepStatus = "FAILED",
                StepProgress = $"{ctx.Master.CurrentStepIndex} / {ctx.Master.Steps.Count}",
                StepMessage = $"스텝 {ctx.Master.CurrentStepIndex + 1} ({missingStep.EventName}) 누락 혹은 타임아웃 순서 이탈 불량 발생.",

                StartLineNo = ctx.CurrentCycleStartLine,
                // 💡 [리스트 복사 주입] 비록 실패했으나 중간 단계까지 성공했던 라인 번호 컬렉션을 무결하게 이관
                MatchedLineNumbers = new List<int>(ctx.ActiveMatchedLines)
            });

            ctx.ActiveMatchedLines.Clear();
            ctx.Master.CurrentStepIndex = 0;
        }

        private string BuildRegexPattern(string scenarioPattern)
        {
            if (string.IsNullOrEmpty(scenarioPattern)) return string.Empty;
            string escaped = Regex.Escape(scenarioPattern);
            string regexPattern = escaped.Replace(@"\*", "(.*?)");
            return $"^{regexPattern}$";
        }

        /// <summary>
        /// 엔진 내부 전수 카운팅 연산 속도 최적화를 위한 격리 컨텍스트 클래스
        /// </summary>
        private class ScenarioBuildContext
        {
            public ScenarioEvaluator Master { get; }
            public int TotalCount { get; set; } = 0;
            public int SuccessCount { get; set; } = 0;
            public int CurrentCycleStartLine { get; set; } = 0;

            // 💡 [엔진 인프라 추가] 현재 추적 중인 사이클 안에서 성공한 라인 번호들을 실시간 수집하는 내부 컨테이너
            public List<int> ActiveMatchedLines { get; set; } = new List<int>();

            public ScenarioBuildContext(ScenarioEvaluator master)
            {
                Master = master;
            }
        }
    }
}