using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace GateHelper
{
    /// <summary>
    /// 메일 본문이나 긴 문서 캡처의 작은 글씨까지 100% 선명하게 읽을 수 있도록
    /// 마우스 휠 딥 줌(10%~500%) 및 마우스 드래그 패닝(Pan)을 지원하는 고화질 이미지 뷰어
    /// </summary>
    public class WorkLogDeepZoomViewer : UserControl
    {
        private Image _image;
        private string _filePath;

        private float _zoom = 1.0f;
        private PointF _pan = new PointF(0, 0);

        private bool _isDragging = false;
        private Point _dragStartPoint;
        private PointF _panStartPoint;

        public event EventHandler ZoomChanged;

        // GDI+ 캐시 브러시 & 폰트 (제로 리소스 준수)
        private static readonly Brush _bgBrush = new SolidBrush(Color.FromArgb(20, 24, 30));
        private static readonly Brush _textDimBrush = new SolidBrush(Color.FromArgb(130, 145, 160));
        private static readonly Brush _badgeBgBrush = new SolidBrush(Color.FromArgb(180, 15, 20, 25));
        private static readonly Brush _badgeTextBrush = new SolidBrush(Color.FromArgb(0, 229, 255));
        private static readonly Pen _borderPen = new Pen(Color.FromArgb(40, 48, 60), 1f);
        private static readonly Font _hudFont = new Font("맑은 고딕", 9f, FontStyle.Bold);
        private static readonly Font _guideFont = new Font("맑은 고딕", 9.5f, FontStyle.Regular);

        public Image CurrentImage => _image;
        public string CurrentFilePath => _filePath;
        public float Zoom => _zoom;

        public WorkLogDeepZoomViewer()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                           ControlStyles.UserPaint |
                           ControlStyles.OptimizedDoubleBuffer |
                           ControlStyles.ResizeRedraw, true);

            this.BackColor = Color.FromArgb(20, 24, 30);
            this.Cursor = Cursors.Default;
        }

        public void SetImage(Image image, string filePath, bool autoFit = true)
        {
            _image = image;
            _filePath = filePath;

            if (_image != null && autoFit)
            {
                FitToScreen();
            }
            else
            {
                _zoom = 1.0f;
                _pan = new PointF(0, 0);
            }

            Invalidate();
            ZoomChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ClearImage()
        {
            _image = null;
            _filePath = null;
            _zoom = 1.0f;
            _pan = new PointF(0, 0);
            Invalidate();
            ZoomChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 1:1 원본 픽셀 크기(100%)로 보기 (메일 작은 글씨 판독용)
        /// </summary>
        public void ZoomToActualSize()
        {
            if (_image == null) return;
            _zoom = 1.0f;
            CenterImage();
            Invalidate();
            ZoomChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 뷰어 화면 크기에 맞춤
        /// </summary>
        public void FitToScreen()
        {
            if (_image == null || Width <= 0 || Height <= 0) return;

            float scaleX = (float)(Width - 16) / _image.Width;
            float scaleY = (float)(Height - 16) / _image.Height;
            _zoom = Math.Min(scaleX, scaleY);

            // 100%보다 큰 작은 이미지라면 1:1 유지
            if (_zoom > 1.0f) _zoom = 1.0f;
            if (_zoom < 0.05f) _zoom = 0.05f;

            CenterImage();
            Invalidate();
            ZoomChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ZoomIn()
        {
            ApplyZoom(1.25f, new Point(Width / 2, Height / 2));
        }

        public void ZoomOut()
        {
            ApplyZoom(0.8f, new Point(Width / 2, Height / 2));
        }

        private void CenterImage()
        {
            if (_image == null) return;
            float scaledW = _image.Width * _zoom;
            float scaledH = _image.Height * _zoom;
            _pan.X = (Width - scaledW) / 2f;
            _pan.Y = (Height - scaledH) / 2f;
        }

        private void ApplyZoom(float factor, Point mousePos)
        {
            if (_image == null) return;

            float oldZoom = _zoom;
            float newZoom = Math.Max(0.05f, Math.Min(5.0f, _zoom * factor));
            if (Math.Abs(oldZoom - newZoom) < 0.001f) return;

            // 마우스 커서가 가리키던 이미지 상대 위치 계산
            float imgX = (mousePos.X - _pan.X) / oldZoom;
            float imgY = (mousePos.Y - _pan.Y) / oldZoom;

            _zoom = newZoom;

            // 마우스 위치가 동일한 이미지 지점을 유지하도록 pan 보정
            _pan.X = mousePos.X - (imgX * _zoom);
            _pan.Y = mousePos.Y - (imgY * _zoom);

            Invalidate();
            ZoomChanged?.Invoke(this, EventArgs.Empty);
        }

        // --- 마우스 이벤트 ---

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (_image == null) return;

            float factor = e.Delta > 0 ? 1.2f : 0.8333f;
            ApplyZoom(factor, e.Location);
            ((HandledMouseEventArgs)e).Handled = true;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left && _image != null)
            {
                _isDragging = true;
                _dragStartPoint = e.Location;
                _panStartPoint = _pan;
                this.Cursor = Cursors.SizeAll;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_isDragging && _image != null)
            {
                float dx = e.X - _dragStartPoint.X;
                float dy = e.Y - _dragStartPoint.Y;
                _pan.X = _panStartPoint.X + dx;
                _pan.Y = _panStartPoint.Y + dy;
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Left)
            {
                _isDragging = false;
                this.Cursor = _image != null ? Cursors.Hand : Cursors.Default;
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            if (_image == null) return;

            // 더블클릭 시 1:1(100%)과 화면 맞춤을 토글
            if (Math.Abs(_zoom - 1.0f) < 0.08f)
            {
                FitToScreen();
            }
            else
            {
                _zoom = 1.0f;
                // 더블클릭한 지점을 중심으로 1:1 확대
                _pan.X = e.X - (_image.Width / 2f);
                _pan.Y = e.Y - (_image.Height / 2f);
                Invalidate();
                ZoomChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (_image != null && !_isDragging)
            {
                // 크기 변경 시 화면 밖으로 크게 벗어나지 않도록 유지
                Invalidate();
            }
        }

        // --- 렌더링 ---

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.FillRectangle(_bgBrush, ClientRectangle);
            g.DrawRectangle(_borderPen, 0, 0, Width - 1, Height - 1);

            if (_image == null)
            {
                // 안내 가이드 표시
                string guide = "첨부된 스크린샷이 없습니다.\r\n\r\n📋 이 영역이나 목록에서 [ Ctrl+V ] 를 누르면\r\n클립보드 스크린샷이 즉시 고화질로 저장됩니다.";
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                {
                    g.DrawString(guide, _guideFont, _textDimBrush, ClientRectangle, sf);
                }
                return;
            }

            // 고화질 보간 설정 (작은 글씨 폰트 가독성 최적화)
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.SmoothingMode = SmoothingMode.HighQuality;

            float destW = _image.Width * _zoom;
            float destH = _image.Height * _zoom;
            var destRect = new RectangleF(_pan.X, _pan.Y, destW, destH);

            g.DrawImage(_image, destRect);

            // 우측 상단 줌 배율 HUD 뱃지
            int pct = (int)Math.Round(_zoom * 100);
            string badgeText = $"배율: {pct}%  (휠: 줌 | 드래그: 이동 | 더블클릭: 1:1/맞춤)";
            var sz = g.MeasureString(badgeText, _hudFont);
            var badgeRect = new Rectangle(Width - (int)sz.Width - 16, 8, (int)sz.Width + 10, (int)sz.Height + 4);

            g.FillRectangle(_badgeBgBrush, badgeRect);
            g.DrawRectangle(_borderPen, badgeRect);
            g.DrawString(badgeText, _hudFont, _badgeTextBrush, badgeRect.X + 5, badgeRect.Y + 2);
        }

        // --- 편의 기능 ---

        public void CopyToClipboard()
        {
            if (_image != null)
            {
                try
                {
                    Clipboard.SetImage(_image);
                    MessageBox.Show("이미지가 클립보드에 복사되었습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("클립보드 복사 실패: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public void OpenInExternalViewer()
        {
            if (!string.IsNullOrEmpty(_filePath) && File.Exists(_filePath))
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(_filePath) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show("외부 뷰어 실행 실패: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
