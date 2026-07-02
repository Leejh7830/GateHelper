namespace GateHelper.LogValidator.Models
{
    public class UnitTemplateModel
    {
        public string EventName { get; set; }
        public string MaskingPattern { get; set; }
    }

    public class ScenarioStepModel
    {
        public int StepNo { get; set; }
        public string EventName { get; set; }
        public string MaskingPattern { get; set; }
        public string Direction { get; set; }

        // 💡 이 스텝 매칭 후 다음 스텝까지 허용 대기 시간 (초)
        // 0 이하면 타임아웃 미설정 (무제한 대기)
        public double TimeoutSeconds { get; set; } = 0;

        // 💡 Optional 스텝: true면 이 스텝이 없어도 사이클 계속 진행
        public bool IsOptional { get; set; } = false;

        // 💡 AND/OR Group: 같은 GroupId를 가진 스텝들의 처리 방식
        // "AND" → 모두 수신되어야 통과 (순서 무관)
        // "OR"  → 하나만 수신되면 통과
        // GroupId = 0이면 그룹 없음 (일반 스텝)
        public int GroupId { get; set; } = 0;
        public string GroupType { get; set; } = "AND"; // 기본값 AND
    }
}
