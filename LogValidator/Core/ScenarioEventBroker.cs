using System;

namespace GateHelper.LogValidator.Core
{
    /// <summary>
    /// 시나리오 편집기와 검증기 간의 데이터 변경 신호를 무결하게 중계하는 글로벌 이벤트 브로커
    /// </summary>
    public static class ScenarioEventBroker
    {
        // 시나리오 자산이 저장 완료되었을 때 발행할 전역 이벤트
        public static event Action OnScenarioSaved;

        /// <summary>
        /// 시나리오 편집기단에서 저장이 완료되었을 때 호출하여 검증기 화면에 리로드 신호를 바이패스합니다.
        /// </summary>
        public static void PublishScenarioSaved()
        {
            OnScenarioSaved?.Invoke();
        }
    }
}