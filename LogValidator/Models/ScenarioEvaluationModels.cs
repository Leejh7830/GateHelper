using System.Collections.Generic;

namespace GateHelper.LogValidator.Models
{
    public enum EvaluationResultStatus
    {
        Ready,
        SUCCESS,
        FAILED
    }

    /// <summary>
    /// 💡 [부모 노드] TreeListView의 최상위 시나리오 마스터 통계 행 모델
    /// </summary>
    public class ScenarioEvaluator
    {
        public string ScenarioName { get; set; }
        public EvaluationResultStatus Status { get; set; }

        // 💡 [변경] 통계적 진척도 표현 (예: "성공 12건 / 총 14건")
        public string Progress { get; set; }

        // 💡 [변경] 전체 발생 횟수 카운트 매핑 컬럼용 프로퍼티
        public string Message { get; set; }

        // 원본 시나리오 세부 스텝 규칙 자산
        public List<ScenarioStepModel> Steps { get; set; } = new List<ScenarioStepModel>();

        // 런타임 연산용 가변 버퍼 포인터
        public int CurrentStepIndex { get; set; }

        /// <summary>
        /// 💡 [자식 노드 컬렉션] 더블클릭 시 아래로 펼쳐질 "각 회차별 실행 사이클 리포트"
        /// </summary>
        public List<StepValidationReport> StepReports { get; set; } = new List<StepValidationReport>();
    }

    /// <summary>
    /// 💡 [자식 노드] 각 회차별(Cycle) 실행 결과 상세 리포트 모델
    /// </summary>
    public class StepValidationReport
    {
        public string StepDisplayHeader { get; set; }
        public string StepStatus { get; set; }
        public string StepProgress { get; set; }
        public string StepMessage { get; set; }

        // 💡 매칭된 로그의 (LineNo, SourceFileName) 쌍을 보존 - 다중 파일 병합 시 같은 LineNo 충돌 방지
        public List<(int LineNo, string SourceFileName)> MatchedLineNumbers { get; set; } = new List<(int, string)>();

        // 💡 사이클 시작 로그의 (LineNo, SourceFileName) - 스크롤 점프 기준점
        public int StartLineNo { get; set; }
        public string StartSourceFileName { get; set; }
    }
}