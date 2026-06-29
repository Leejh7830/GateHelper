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
        // 예: 특정 조건에서만 발생하는 ACK 신호처럼 있어도 되고 없어도 되는 스텝
        public bool IsOptional { get; set; } = false;
    }
}
