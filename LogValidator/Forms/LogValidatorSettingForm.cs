using System;
using System.Windows.Forms;
using System.Collections.Generic;
using GateHelper.LogValidator.Core;
using GateHelper.LogValidator.Models;
using MaterialSkin.Controls;

namespace GateHelper.LogValidator
{
    public partial class LogValidatorSettingForm : MaterialForm // 💡 만약 MaterialForm이면 MaterialForm으로 유지
    {
        public LogValidatorSettingForm()
        {
            InitializeComponent();
        }

        // 💡 [폼 로드 락인] 창이 열릴 때 현재 저장된 설정을 텍스트박스에 줄바꿈 상태로 바인딩
        private void LogValidatorSettingForm_Load(object sender, EventArgs e)
        {
            // 전역 세팅 데이터 강제 로드
            LogValidatorConfigManager.Load();

            var config = LogValidatorConfigManager.Current;

            // List<string> 데이터를 MultiLine 텍스트박스에 엔터(\r\n) 구분으로 파이프라인 수송
            txtFactoryPrefixes.Text = string.Join(Environment.NewLine, config.FactoryPrefixes);
            txtLineZones.Text = string.Join(Environment.NewLine, config.LineZones);
            txtEquipmentTypes.Text = string.Join(Environment.NewLine, config.EquipmentTypes);
        }

        // 💡 [SAVE 버튼 인터락] 사용자가 입력한 엔터 기반 문자열을 쪼개어 다시 데이터셋으로 갱신
        private void btnSave_Click(object sender, EventArgs e)
        {
            var config = LogValidatorConfigManager.Current;

            // 줄바꿈 단위로 쪼갠 뒤, 빈 공백은 지우고 무조건 대문자 처리하여 데이터 정합성 가드 가동
            config.FactoryPrefixes = TextToCleanList(txtFactoryPrefixes.Text);
            config.LineZones = TextToCleanList(txtLineZones.Text);
            config.EquipmentTypes = TextToCleanList(txtEquipmentTypes.Text);

            // 디스크 파일 동기화 저장
            LogValidatorConfigManager.Save();

            // 부모 폼(메인)에 성공 신호 송신 후 창 닫기
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // 💡 [CANCEL 버튼 방어] 아무것도 저장하지 않고 즉시 격리 종료
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        /// <summary>
        /// 멀티라인 텍스트 내용을 안전한 문자열 리스트로 파싱하는 내부 필터 유틸리티
        /// </summary>
        private List<string> TextToCleanList(string rawText)
        {
            var list = new List<string>();
            if (string.IsNullOrWhiteSpace(rawText)) return list;

            // 엔터(\n, \r) 및 쉼표(,)까지 전부 고려하여 유연하게 쪼갬
            string[] tokens = rawText.Split(new[] { "\r\n", "\r", "\n", "," }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var token in tokens)
            {
                string clean = token.Trim().ToUpper();
                if (!string.IsNullOrEmpty(clean) && !list.Contains(clean))
                {
                    list.Add(clean);
                }
            }
            return list;
        }
    }
}