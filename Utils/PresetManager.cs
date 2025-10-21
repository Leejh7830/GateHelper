using System;
using System.Windows.Forms;
using OpenQA.Selenium;
using static GateHelper.LogManager;

namespace GateHelper
{
    public static class PresetManager
    {
        public enum Preset { None, A, B }

        // skipDriverCheck: true�� ����̹� �غ� ���� �˻縦 �ǳʶ�(��: Start ȣ�� ���)
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
                MessageBox.Show("������ �����ϴ�.", "�˸�", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            string id = preset == Preset.A ? config.GateID_A : config.GateID_B;
            string pw = preset == Preset.A ? config.GatePW_A : config.GatePW_B;

            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(pw) || (!skipDriverCheck && !chromeDriverManager.IsDriverReady(driver)))
            {
                MessageBox.Show("���� Start ��ư�� �����ּ���. ���� ID/PW ���� �����ϴ�.", "�˸�", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            gateId = id;
            gatePw = pw;

            // UI ������ Util_Control�� ����
            Util_Control.ApplyPresetSelection(btnA, btnB, preset == Preset.A, preset == Preset.B);

            LogMessage($"GateID/PW {preset} set.", Level.Info);
            return true;
        }
    }
}