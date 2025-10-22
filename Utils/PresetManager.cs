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
                MessageBox.Show("config 값이 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            string id = preset == Preset.A ? config.GateID_A : config.GateID_B;
            string pw = preset == Preset.A ? config.GatePW_A : config.GatePW_B;

            // A는 필수: 값이 없으면 설정 안내
            if (preset == Preset.A && (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(pw)))
            {
                MessageBox.Show("Preset A가 설정되지 않았습니다. 설정에서 A의 ID/PW를 입력해 주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // B는 선택: 값이 없을 때 드라이버 상태에 따라 안내 분기
            if (preset == Preset.B && (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(pw)))
            {
                bool driverAlive = driver != null && chromeDriverManager.IsDriverAlive(driver);
                if (driverAlive)
                {
                    MessageBox.Show("선택한 프리셋의 ID/PW가 설정되어 있지 않습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                else
                {
                    // 드라이버가 없으면 Start 안내 (skipDriverCheck로 제어)
                    if (!skipDriverCheck)
                    {
                        // IsDriverReady(showMessage: true) 가 메시지 출력
                        chromeDriverManager.IsDriverReady(driver);
                    }
                    return false;
                }
            }

            // 값은 있는데 드라이버 준비가 필요하면 검사 (Start 경로에서는 skipDriverCheck=true로 호출)
            if (!skipDriverCheck)
            {
                if (!chromeDriverManager.IsDriverReady(driver))
                {
                    // IsDriverReady가 메시지 처리함
                    return false;
                }
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