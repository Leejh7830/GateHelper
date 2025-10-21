using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateHelper
{
    public class AppSettings
    {
        public bool RemoveDuplicates { get; set; }
        public bool AutoLogin { get; set; }
        public bool DisablePopup { get; set; }
        public bool TestMode { get; set; }
        public bool ServerClickConnect { get; set; }
        public int PopupGraceMs { get; set; }

        public bool FavOneClickConnect { get; set; }

        public AppSettings() // 옵션 변수들
        {
            RemoveDuplicates = true;
            AutoLogin = false;
            DisablePopup = false;
            TestMode = false;
            ServerClickConnect = true;
            PopupGraceMs = 5000;
            FavOneClickConnect = false;
        }
    }
}
