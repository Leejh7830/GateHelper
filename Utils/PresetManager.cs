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
                MessageBox.Show("config ���� �����ϴ�.", "�˸�", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            string id = preset == Preset.A ? config.GateID_A : config.GateID_B;
            string pw = preset == Preset.A ? config.GatePW_A : config.GatePW_B;

            // A�� �ʼ�: ���� ������ ���� �ȳ�
            if (preset == Preset.A && (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(pw)))
            {
                MessageBox.Show("Preset A�� �������� �ʾҽ��ϴ�. �������� A�� ID/PW�� �Է��� �ּ���.", "�˸�", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // B�� ����: ���� ���� �� ����̹� ���¿� ���� �ȳ� �б�
            if (preset == Preset.B && (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(pw)))
            {
                bool driverAlive = driver != null && chromeDriverManager.IsDriverAlive(driver);
                if (driverAlive)
                {
                    MessageBox.Show("������ �������� ID/PW�� �����Ǿ� ���� �ʽ��ϴ�.", "�˸�", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                else
                {
                    // ����̹��� ������ Start �ȳ� (skipDriverCheck�� ����)
                    if (!skipDriverCheck)
                    {
                        // IsDriverReady(showMessage: true) �� �޽��� ���
                        chromeDriverManager.IsDriverReady(driver);
                    }
                    return false;
                }
            }

            // ���� �ִµ� ����̹� �غ� �ʿ��ϸ� �˻� (Start ��ο����� skipDriverCheck=true�� ȣ��)
            if (!skipDriverCheck)
            {
                if (!chromeDriverManager.IsDriverReady(driver))
                {
                    // IsDriverReady�� �޽��� ó����
                    return false;
                }
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