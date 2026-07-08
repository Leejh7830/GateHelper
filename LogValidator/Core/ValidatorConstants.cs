namespace GateHelper.LogValidator.Core
{
    /// <summary>
    /// LogValidator 시스템 전역에서 사용되는 매직 스트링(상수)을 중앙 관리합니다.
    /// 오타로 인한 런타임 에러를 방지하고 유지보수성을 극대화합니다.
    /// </summary>
    public static class ValidatorConstants
    {
        // 논리 연산 그룹
        public const string GROUP_AND = "AND";
        public const string GROUP_OR = "OR";

        // 기본 유닛 식별자
        public const string UNIT_SYSTEM = "SYSTEM";

        // 평가 결과 상태
        public const string STATUS_SUCCESS = "SUCCESS";
        public const string STATUS_FAILED = "FAILED";
        public const string STATUS_READY = "Ready";
    }
}