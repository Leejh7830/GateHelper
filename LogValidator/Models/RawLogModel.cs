using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateHelper.LogValidator.Models
{
    public class RawLogModel
    {
        public int LineNo { get; set; }        // 로그 라인 번호
        public string Timestamp { get; set; }  // 분리 추출된 시분초 데이터
        public string LogMessage { get; set; } // 원본 로그 텍스트 전체
    }
}