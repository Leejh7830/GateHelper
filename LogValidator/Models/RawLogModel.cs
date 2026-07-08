using System;

namespace GateHelper.LogValidator.Models
{
    public class RawLogModel
    {
        // 💡 [캡슐화] 외부에서 마음대로 값을 바꿀 수 없도록 private set 적용 (불변성 확보)
        public int LineNo { get; private set; }
        public string LogType { get; private set; }
        public string LogMessage { get; private set; }
        public string SourceFileName { get; private set; }

        // 💡 시간은 파싱 시 Fallback 보정이 일어날 수 있어 열어둠
        public DateTime LogTime { get; set; }

        // 💡 UI 그리드 렌더링 최적화(IsMatch) 상태값 추가 (이전 치명적 버그 수정용)
        public bool IsMatched { get; set; }

        private string _unitId = "SYSTEM";
        public string UnitID
        {
            get => _unitId;
            set => _unitId = string.IsNullOrWhiteSpace(value) ? "SYSTEM" : value.Trim().ToUpper();
        }

        // 💡 [생성자 주입] 객체가 태어날 때 무조건 필수 데이터를 받아오도록 강제함
        public RawLogModel(int lineNo, string logMessage, string logType, string sourceFileName)
        {
            LineNo = lineNo;
            LogMessage = logMessage;
            LogType = logType;
            SourceFileName = sourceFileName;
        }
    }
}