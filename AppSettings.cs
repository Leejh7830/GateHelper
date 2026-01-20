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
        public bool UseUDP { get; set; }

        public AppSettings() // 옵션 변수들
        {
            RemoveDuplicates = true;
            AutoLogin = false;
            DisablePopup = false;
            TestMode = false;
            ServerClickConnect = true;
            PopupGraceMs = 30000; // GraceTime Default Value
            FavOneClickConnect = false;
            UseUDP = false;
        }
    }
}
