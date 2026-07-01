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

        // 💡 AND Group: 같은 GroupId를 가진 스텝들은 순서 무관하게 모두 수신되어야 다음으로 진행
        // 0이면 일반 스텝 (그룹 없음), 양수면 해당 그룹 ID
        // 예: A, B가 GroupId=1이면 A→B 또는 B→A 어느 순서로 와도 SUCCESS
        public int GroupId { get; set; } = 0;
    }
}
