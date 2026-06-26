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
        public string UnitID { get; set; } = "SYSTEM";
        public string TrayID { get; set; }        // LHEE018053 등 임시 확장용
        public string LogMessage { get; set; }    // 원본 로그 라인 텍스트
    }
}