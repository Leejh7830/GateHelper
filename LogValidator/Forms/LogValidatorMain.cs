using GateHelper.LogValidator;
using GateHelper.LogValidator.Core;
using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GateHelper.LogValidator
{
    public partial class LogValidatorMain : MaterialForm
    {
        // 메인 프레임워크의 테마 매니저 인스턴스를 동기화
        private readonly MaterialSkinManager _skinManager = MaterialSkinManager.Instance;

        public LogValidatorMain()
        {
            InitializeComponent();

            // [테마 동기화 엔진] 메인 프로그램의 다크/라이트 상태를 그대로 복제
            _skinManager.AddFormToManage(this);
        }

        private void LogValidatorMain_Load(object sender, EventArgs e)
        {

        }

        // 분석창 (LVForm) 호출
        private void btnOpenValidator_Click(object sender, EventArgs e)
        {
            // 💡 [수정] using 블록 제거
            // using은 ShowDialog 종료 즉시 Dispose()를 호출하는데, 이 시점이
            // LogValidatorForm의 FormClosing 이벤트 처리(ObjectListView 내부 정리)와
            // 거의 동시에 발생하면서 "TrueType 폰트만 지원됩니다" 예외가 간헐적으로 발생했음.
            // ShowDialog로 띄운 모달 폼은 닫힐 때 WinForms가 적절한 시점에 자동으로 Dispose하므로
            // 수동 using이 불필요하며, 오히려 타이밍 경쟁을 유발함.
            LogValidatorForm lvForm = new LogValidatorForm();
            lvForm.StartPosition = FormStartPosition.CenterParent;
            lvForm.ShowDialog(this);
        }

        // 시나리오 편집기 (LSForm) 호출
        private void btnOpenEditor_Click(object sender, EventArgs e)
        {
            LogScenarioForm lsForm = new LogScenarioForm();
            lsForm.StartPosition = FormStartPosition.CenterParent;
            lsForm.ShowDialog(this);
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            // 💡 [입출력 동기화 가드] 설정 창을 열기 직전, 디스크에서 최신 JSON 설정을 불러와 동기화합니다.
            LogValidatorConfigManager.Load();

            LogValidatorSettingForm settingForm = new LogValidatorSettingForm();
            if (settingForm.ShowDialog() == DialogResult.OK)
            {
                ShowToast("✓ Settings saved successfully.");
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // ─────────────────────────────────────────────
        // 💡 토스트 알림 - LogScenarioForm, LogValidatorForm과 동일한 페이드인/아웃 방식
        // ─────────────────────────────────────────────
        private Form _toastForm;
        private System.Windows.Forms.Timer _toastFadeTimer;

        private void ShowToast(string message, int durationMs = 3000)
        {
            _toastFadeTimer?.Stop();
            _toastFadeTimer?.Dispose();
            _toastForm?.Dispose();

            var lbl = new Label
            {
                Text = message,
                Font = new System.Drawing.Font("Malgun Gothic", 9.5f, System.Drawing.FontStyle.Regular),
                ForeColor = System.Drawing.Color.White,
                AutoSize = true,
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
            };

            _toastForm = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.Manual,
                ShowInTaskbar = false,
                TopMost = true,
                BackColor = System.Drawing.Color.FromArgb(45, 45, 48),
                Opacity = 0,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(18, 10, 18, 10),
            };
            _toastForm.Controls.Add(lbl);

            _toastForm.Load += (s, e) =>
            {
                int x = this.Left + (this.Width - _toastForm.Width) / 2;
                int y = this.Top + this.Height - _toastForm.Height - 80;
                _toastForm.Location = new System.Drawing.Point(x, y);
            };

            _toastForm.Show(this);

            FadeToast(1.0, () =>
            {
                var wait = new System.Windows.Forms.Timer { Interval = durationMs };
                wait.Tick += (s, e) =>
                {
                    wait.Stop();
                    wait.Dispose();
                    FadeToast(0.0, () => { _toastForm?.Dispose(); _toastForm = null; });
                };
                wait.Start();
            });
        }

        private void FadeToast(double target, Action onComplete)
        {
            if (_toastForm == null) { onComplete?.Invoke(); return; }
            const double STEP = 0.08;
            _toastFadeTimer = new System.Windows.Forms.Timer { Interval = 20 };
            _toastFadeTimer.Tick += (s, e) =>
            {
                if (_toastForm == null) { _toastFadeTimer.Stop(); _toastFadeTimer.Dispose(); return; }
                double next = _toastForm.Opacity < target
                    ? Math.Min(_toastForm.Opacity + STEP, target)
                    : Math.Max(_toastForm.Opacity - STEP, target);
                _toastForm.Opacity = next;
                if (Math.Abs(next - target) < 0.001)
                {
                    _toastFadeTimer.Stop();
                    _toastFadeTimer.Dispose();
                    onComplete?.Invoke();
                }
            };
            _toastFadeTimer.Start();
        }
    }
}
