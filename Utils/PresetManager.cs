using System;
using System.Windows.Forms;
using OpenQA.Selenium;
using static GateHelper.LogManager;

namespace GateHelper
{
    public static class PresetManager
    {
        public enum Preset { None, A, B }

        // skipDriverCheck: true면 드라이버 준비 여부 검사를 건너뜀(예: Start 호출 경로)
        public static bool TryApplyPreset(
            Config config,
            ChromeDriverManager chromeDriverManager,
            IWebDriver driver,
            Preset preset,
            Control btnA,
            Control btnB,
            out string gateId,
            out string gatePw,
            bool skipDriverCheck = false)
        {
            gateId = null;
            gatePw = null;

            if (config == null)
            {
                MessageBox.Show("설정이 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            string id = preset == Preset.A ? config.GateID_A : config.GateID_B;
            string pw = preset == Preset.A ? config.GatePW_A : config.GatePW_B;

            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(pw) || (!skipDriverCheck && !chromeDriverManager.IsDriverReady(driver)))
            {
                MessageBox.Show("먼저 Start 버튼을 눌러주세요. 현재 ID/PW 값이 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            gateId = id;
            gatePw = pw;

            // UI 갱신은 Util_Control에 위임
            Util_Control.ApplyPresetSelection(btnA, btnB, preset == Preset.A, preset == Preset.B);

            LogMessage($"GateID/PW {preset} set.", Level.Info);
            return true;
        }
    }
}