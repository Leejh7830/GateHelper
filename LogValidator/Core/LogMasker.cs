using System.Text.RegularExpressions;

namespace GateHelper.LogValidator.Core
{
    public class LogMasker
    {
        // 마스킹/하이라이트 대상 고정 정규식 패턴 어레이
        private readonly string[] _patterns = new string[]
        {
            @"\d{4}-\d{2}-\d{2}",           // 날짜
            @"\d{2}:\d{2}:\d{2}(\.\d{3})?", // 시간
            @"\[INFO\]|\[ERROR\]|\[WARN\]", // 로그 레벨
            @"\b\d+ms\b",                    // ms 단위 변수
            @"\b\d+\b"                       // 순수 가변 숫자 변수
        };

        public MatchCollection GetMaskingMatches(string text)
        {
            // 전체 룰셋을 하나의 정규식 논리합 구조로 통합 매칭 처리
            string combinedPattern = string.Join("|", _patterns);
            return Regex.Matches(text, combinedPattern);
        }
    }
}