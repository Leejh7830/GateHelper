using System;
using System.Windows.Forms;
using OpenQA.Selenium;
using static GateHelper.LogManager;

namespace GateHelper
{
    public static class PresetManager
    {
        public enum Preset { None, A, B }

        // ����̹� üũ ���� config ���� Ȯ��
        public static bool TryApplyPreset(
            Config config,
            Preset preset,
            Control btnA,
            Control btnB,
            out string gateId,
            out string gatePw)
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

            // A�� �ʼ�: ���� ������ �ȳ�
            if (preset == Preset.A && (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(pw)))
            {
                MessageBox.Show("Preset A�� �������� �ʾҽ��ϴ�. �������� ID/PW�� �Է��� �ּ���.", "�˸�", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // B�� ����: ���� ������ �ȳ�
            if (preset == Preset.B && (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(pw)))
            {
                MessageBox.Show("Preset B�� ID/PW�� �����Ǿ� ���� �ʽ��ϴ�.", "�˸�", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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