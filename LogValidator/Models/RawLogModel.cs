using System;

namespace GateHelper.LogValidator.Models
{
    public class RawLogModel
    {
        public int LineNo { get; set; }
        public DateTime LogTime { get; set; }
        public string LogType { get; set; }
        public string LogMessage { get; set; }

        // 다중 로그 병합 시 어느 파일에서 온 로그인지 추적하기 위한 필드
        public string SourceFileName { get; set; }

        private string _unitId = "SYSTEM";
        public string UnitID
        {
            get => _unitId;
            set => _unitId = string.IsNullOrWhiteSpace(value) ? "SYSTEM" : value.Trim().ToUpper();
        }

        // 💡 향후 확장용 (현재 미사용이므로 주석 처리)
        // public string TrayID { get; set; }
    }
}
