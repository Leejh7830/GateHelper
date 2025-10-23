using System;
using System.Windows.Forms;
using OpenQA.Selenium;
using static GateHelper.LogManager;

namespace GateHelper
{
    public static class PresetManager
    {
        public enum Preset { None, A, B }

        // 드라이버 체크 없이 config 값만 확인
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
                MessageBox.Show("config 값이 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            string id = preset == Preset.A ? config.GateID_A : config.GateID_B;
            string pw = preset == Preset.A ? config.GatePW_A : config.GatePW_B;

            // A는 필수: 값이 없으면 안내
            if (preset == Preset.A && (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(pw)))
            {
                MessageBox.Show("Preset A가 설정되지 않았습니다. 설정에서 ID/PW를 입력해 주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // B는 선택: 값이 없으면 안내
            if (preset == Preset.B && (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(pw)))
            {
                MessageBox.Show("Preset B의 ID/PW가 설정되어 있지 않습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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