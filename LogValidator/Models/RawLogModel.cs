using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateHelper.LogValidator.Models
{
    public class RawLogModel
    {
        public int LineNo { get; set; }
        public DateTime LogTime { get; set; }     // 💡 타임라인 정렬의 절대 기준
        public string LogType { get; set; }       // "PLC", "ALARM", "TRANSFER" 등

        private string _unitId = "SYSTEM";

        public string UnitID
        {
            get => _unitId;
            // 💡 null 및 공백 방어 후 무조건적인 대문자 락인
            set => _unitId = string.IsNullOrWhiteSpace(value) ? "SYSTEM" : value.Trim().ToUpper();
        }

        public string TrayID { get; set; }        // 임시 확장용
        public string LogMessage { get; set; }    // 원본 로그 라인 텍스트
    }
}