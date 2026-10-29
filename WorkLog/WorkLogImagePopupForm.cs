using System;
using System.Drawing;
using System.Windows.Forms;

namespace GateHelper
{
    /// <summary>
    /// 메일 스크린샷 등을 서브 모니터에 독립 창으로 띄워놓고 보면서 작업할 수 있는 팝아웃 뷰어 창
    /// </summary>
    public class WorkLogImagePopupForm : Form
    {
        private WorkLogDeepZoomViewer _viewer;
        private Panel _toolPanel;
        private Button _btnActual;
        private Button _btnFit;
        private Button _btnZoomIn;
        private Button _btnZoomOut;
        private Button _btnCopy;
        private Button _btnClose;

        public WorkLogImagePopupForm(Image img, string filePath, string title)
        {
            this.Text = string.IsNullOrEmpty(title) ? "스크린샷 팝아웃 뷰어" : $"스크린샷 - {title}";
            this.Size = new Size(1000, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(20, 24, 30);
            this.ForeColor = Color.White;
            this.ShowIcon = false;

            BuildUI();
            _viewer.SetImage(img, filePath, autoFit: true);
        }

        private void BuildUI()
        {
            _toolPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 42,
                BackColor = Color.FromArgb(28, 33, 40),
                Padding = new Padding(8, 4, 8, 4)
            };

            _btnActual = CreateToolButton("1:1 원본", (s, e) => _viewer.ZoomToActualSize());
            _btnFit = CreateToolButton("화면 맞춤", (s, e) => _viewer.FitToScreen());
            _btnZoomIn = CreateToolButton("확대 (+)", (s, e) => _viewer.ZoomIn());
            _btnZoomOut = CreateToolButton("축소 (ㅡ)", (s, e) => _viewer.ZoomOut());
            _btnCopy = CreateToolButton("이미지 복사", (s, e) => _viewer.CopyToClipboard());
            _btnClose = CreateToolButton("닫기 (ESC)", (s, e) => this.Close());

            _toolPanel.Controls.Add(_btnActual);
            _toolPanel.Controls.Add(_btnFit);
            _toolPanel.Controls.Add(_btnZoomIn);
            _toolPanel.Controls.Add(_btnZoomOut);
            _toolPanel.Controls.Add(_btnCopy);
            _toolPanel.Controls.Add(_btnClose);

            // 우측에서부터 배치
            int x = 8;
            foreach (Control c in _toolPanel.Controls)
            {
                c.Location = new Point(x, 6);
                x += c.Width + 6;
            }

            _viewer = new WorkLogDeepZoomViewer
            {
                Dock = DockStyle.Fill
            };

            this.Controls.Add(_viewer);
            this.Controls.Add(_toolPanel);

            this.KeyPreview = true;
            this.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape) this.Close();
                if (e.Control && e.KeyCode == Keys.C) _viewer.CopyToClipboard();
            };
        }

        private Button CreateToolButton(string text, EventHandler onClick)
        {
            var btn = new Button
            {
                Text = text,
                AutoSize = true,
                Height = 28,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(44, 52, 64),
                ForeColor = Color.FromArgb(230, 237, 243),
                Cursor = Cursors.Hand,
                Font = new Font("맑은 고딕", 9f)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += onClick;
            return btn;
        }
    }
}
