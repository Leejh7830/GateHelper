using System;
using System.Collections.Generic;

namespace GateHelper.LogValidator.Models
{
    /// <summary>
    /// 단일 패스 순회 중 각 시나리오의 진행 상태를 독립적으로 추적하는 상태 머신 래퍼
    /// </summary>
    public class ScenarioEvaluator
    {
        public string ScenarioName { get; set; }
        public List<ScenarioStepModel> Steps { get; set; }

        // 현재 매칭을 대기 중인 스텝의 인덱스
        public int CurrentStepIndex { get; set; } = 0;

        // 이전 스텝이 매칭 완료된 시점의 타임스탬프 (TimeoutMs 검증용)
        public DateTime? LastMatchedTimestamp { get; set; }

        // 최종 검증 상태 결과
        public EvaluationResultStatus Status { get; set; } = EvaluationResultStatus.Ready;
        public string FailureReason { get; set; } = string.Empty;
        public int ErrorLineNo { get; set; } = -1;

        public bool IsCompleted => CurrentStepIndex >= Steps.Count;
    }

    public enum EvaluationResultStatus
    {
        Ready,
        Processing,
        Success,
        FailedTimeout,
        FailedMissingStep
    }

    /// <summary>
    /// UI 리포트 그리드에 최종 출력할 결과 행 모델
    /// </summary>
    public class ScenarioReportModel
    {
        public string ScenarioName { get; set; }
        public string Status { get; set; } // SUCCESS, TIMEOUT_FAIL, MISSING_FAIL 등
        public string Progress { get; set; } // 예: "4 / 4" 또는 "2 / 5 (Line: 140)"
        public string Message { get; set; }
    }
}