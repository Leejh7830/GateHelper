using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace GateHelper.Dashboard
{
    #region [ 1. 세그먼트 게이지 (Segmented RAM Gauge) ]
    /// <summary>
    /// 초록(안전) -> 노랑(주의) -> 빨강(경고) 칸으로 구성된 모던 세그먼트 게이지
    /// </summary>
    public class SegmentedGaugeControl : Control
    {
        private int _totalSegments = 16;
        private double _value = 0;
        private double _maximum = 512;
        private string _title = "RAM USAGE";
        private string _valueText = "0 MB";
        private string _statusText = "쾌적";
        private Color _statusColor = Color.LightGreen;

        public SegmentedGaugeControl()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.ResizeRedraw, true);
            this.DoubleBuffered = true;
            this.Size = new Size(320, 95);
            this.Font = new Font("맑은 고딕", 9F, FontStyle.Regular);
        }

        public double Value
        {
            get => _value;
            set { _value = Math.Max(0, value); Invalidate(); }
        }

        public double Maximum
        {
            get => _maximum;
            set { _maximum = Math.Max(1, value); Invalidate(); }
        }

        public string Title
        {
            get => _title;
            set { _title = value; Invalidate(); }
        }

        public string ValueText
        {
            get => _valueText;
            set { _valueText = value; Invalidate(); }
        }

        public void SetGaugeData(double value, double maximum, string valueText, string statusText, Color statusColor)
        {
            _value = value;
            _maximum = maximum;
            _valueText = valueText;
            _statusText = statusText;
            _statusColor = statusColor;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 배경
            Color bgCard = Color.FromArgb(40, 40, 44);
            using (var brush = new SolidBrush(bgCard))
            {
                g.FillRectangle(brush, ClientRectangle);
            }

            // 상단 타이틀 & 수치
            using (var titleBrush = new SolidBrush(Color.Gainsboro))
            using (var titleFont = new Font("맑은 고딕", 9.5F, FontStyle.Bold))
            {
                g.DrawString(_title, titleFont, titleBrush, new PointF(10, 8));
            }

            using (var valBrush = new SolidBrush(Color.White))
            using (var valFont = new Font("맑은 고딕", 13.5F, FontStyle.Bold))
            {
                var valSize = g.MeasureString(_valueText, valFont);
                g.DrawString(_valueText, valFont, valBrush, new PointF(Width - valSize.Width - 10, 5));
            }

            // 세그먼트 바 영역
            int barY = 36;
            int barHeight = 22;
            int barWidth = Width - 20;
            int barX = 10;
            int spacing = 3;

            double ratio = Math.Min(1.0, Math.Max(0.0, _value / _maximum));
            int activeSegments = (int)Math.Round(ratio * _totalSegments);

            int segWidth = (barWidth - (spacing * (_totalSegments - 1))) / _totalSegments;

            for (int i = 0; i < _totalSegments; i++)
            {
                int segX = barX + i * (segWidth + spacing);
                Rectangle segRect = new Rectangle(segX, barY, segWidth, barHeight);

                // 세그먼트 색상 결정 (초록 60% -> 노랑 85% -> 빨강 100%)
                double segRatio = (double)(i + 1) / _totalSegments;
                Color activeColor;
                if (segRatio <= 0.60)
                {
                    activeColor = Color.FromArgb(46, 204, 113); // 네온 그린
                }
                else if (segRatio <= 0.85)
                {
                    activeColor = Color.FromArgb(241, 196, 15); // 앰버 옐로우
                }
                else
                {
                    activeColor = Color.FromArgb(231, 76, 60);  // 크림슨 레드
                }

                if (i < activeSegments)
                {
                    using (var segBrush = new SolidBrush(activeColor))
                    {
                        g.FillRectangle(segBrush, segRect);
                    }
                }
                else
                {
                    // 비활성 칸
                    using (var offBrush = new SolidBrush(Color.FromArgb(60, 60, 65)))
                    {
                        g.FillRectangle(offBrush, segRect);
                    }
                }
            }

            // 하단 상태 문구
            using (var statusBrush = new SolidBrush(_statusColor))
            using (var statusFont = new Font("맑은 고딕", 9F, FontStyle.Regular))
            {
                g.DrawString($"상태: {_statusText}", statusFont, statusBrush, new PointF(10, 68));
            }

            // 하단 최대치 라벨
            using (var maxBrush = new SolidBrush(Color.DarkGray))
            using (var maxFont = new Font("맑은 고딕", 9F, FontStyle.Regular))
            {
                string maxStr = $"기준 한도: {_maximum:N0} MB";
                var sz = g.MeasureString(maxStr, maxFont);
                g.DrawString(maxStr, maxFont, maxBrush, new PointF(Width - sz.Width - 10, 68));
            }

            // 테두리
            using (var pen = new Pen(Color.FromArgb(65, 65, 70), 1))
            {
                g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
        }
    }
    #endregion

    #region [ 2. 실시간 CPU & RAM 꺾은선 트렌드 차트 (Sparkline Trend Chart) ]
    /// <summary>
    /// 최근 40개 샘플 동안의 CPU 점유율과 RAM 변화를 실시간 네온 곡선으로 그리는 고성능 차트
    /// </summary>
    public class RealtimeTrendChartControl : Control
    {
        private readonly List<float> _cpuHistory = new List<float>();
        private readonly List<float> _ramHistory = new List<float>();
        private const int MaxSamples = 40;

        private float _currentCpu = 0f;
        private float _currentRamMb = 0f;

        public RealtimeTrendChartControl()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.ResizeRedraw, true);
            this.DoubleBuffered = true;
            this.Size = new Size(380, 150);

            // 초기 더미 데이터 채우기 (0으로 평평하게 시작)
            for (int i = 0; i < MaxSamples; i++)
            {
                _cpuHistory.Add(0f);
                _ramHistory.Add(0f);
            }
        }

        public void AddSample(float cpuPercent, float ramMb)
        {
            _currentCpu = cpuPercent;
            _currentRamMb = ramMb;

            _cpuHistory.Add(cpuPercent);
            if (_cpuHistory.Count > MaxSamples) _cpuHistory.RemoveAt(0);

            _ramHistory.Add(ramMb);
            if (_ramHistory.Count > MaxSamples) _ramHistory.RemoveAt(0);

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 1. 배경
            using (var brush = new SolidBrush(Color.FromArgb(32, 32, 36)))
            {
                g.FillRectangle(brush, ClientRectangle);
            }

            // 2. 모안 눈금 격자선 (Grid lines)
            using (var gridPen = new Pen(Color.FromArgb(48, 48, 54), 1) { DashStyle = DashStyle.Dot })
            {
                int gridSteps = 4;
                for (int i = 1; i < gridSteps; i++)
                {
                    int y = Height * i / gridSteps;
                    g.DrawLine(gridPen, 10, y, Width - 10, y);
                }
            }

            // 차트 플롯 영역
            int padLeft = 12;
            int padRight = 12;
            int padTop = 32;
            int padBottom = 24;
            int plotW = Width - padLeft - padRight;
            int plotH = Height - padTop - padBottom;

            if (plotW <= 0 || plotH <= 0 || _cpuHistory.Count < 2) return;

            // 3. CPU 곡선 그리기 (네온 시안 컬러)
            DrawCurve(g, _cpuHistory, 0f, 30f, Color.FromArgb(0, 210, 255), Color.FromArgb(35, 0, 210, 255), padLeft, padTop, plotW, plotH);

            // 4. 상단 헤더 & 범례 (Legend)
            using (var titleFont = new Font("맑은 고딕", 9.5F, FontStyle.Bold))
            using (var numFont = new Font("맑은 고딕", 9F, FontStyle.Bold))
            {
                // 타이틀 (간결하고 직관적으로 정돈)
                g.DrawString("📈 리소스 트렌드", titleFont, Brushes.White, new PointF(10, 8));

                // 지표 텍스트
                string ramStr = $"RAM: {_currentRamMb:F0} MB";
                string cpuStr = $"CPU: {_currentCpu:F1}%";

                var ramSz = g.MeasureString(ramStr, numFont);
                var cpuSz = g.MeasureString(cpuStr, numFont);

                // 우측 끝에서부터 동적으로 안전 배치하여 절대 겹치지 않도록 처리
                float ramTextX = Width - ramSz.Width - 10;
                float ramDotX = ramTextX - 11;

                float cpuTextX = ramDotX - cpuSz.Width - 14;
                float cpuDotX = cpuTextX - 11;

                // CPU 지표 (시안)
                using (var cyanBrush = new SolidBrush(Color.FromArgb(0, 220, 255)))
                {
                    g.FillEllipse(cyanBrush, cpuDotX, 12, 7, 7);
                    g.DrawString(cpuStr, numFont, cyanBrush, new PointF(cpuTextX, 8));
                }

                // RAM 지표 (그린)
                using (var greenBrush = new SolidBrush(Color.FromArgb(46, 204, 113)))
                {
                    g.FillEllipse(greenBrush, ramDotX, 12, 7, 7);
                    g.DrawString(ramStr, numFont, greenBrush, new PointF(ramTextX, 8));
                }
            }

            // 하단 축 라벨
            using (var axisFont = new Font("맑은 고딕", 9F, FontStyle.Regular))
            {
                g.DrawString("-40초", axisFont, Brushes.Gray, new PointF(padLeft, Height - 18));
                g.DrawString("현재 (0초)", axisFont, Brushes.Gray, new PointF(Width - padRight - 55, Height - 18));
            }

            // 외곽선
            using (var pen = new Pen(Color.FromArgb(65, 65, 70), 1))
            {
                g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
        }

        private void DrawCurve(Graphics g, List<float> data, float minVal, float maxVal, Color lineColor, Color fillGradientColor, int x0, int y0, int w, int h)
        {
            if (data.Count < 2) return;

            PointF[] points = new PointF[data.Count];
            float stepX = (float)w / (data.Count - 1);

            for (int i = 0; i < data.Count; i++)
            {
                float val = data[i];
                float norm = Math.Max(0f, Math.Min(1f, (val - minVal) / (maxVal - minVal)));
                float y = y0 + h - (norm * h);
                points[i] = new PointF(x0 + i * stepX, y);
            }

            // 영역 그라데이션 채우기
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddLines(points);
                path.AddLine(points[points.Length - 1], new PointF(points[points.Length - 1].X, y0 + h));
                path.AddLine(new PointF(points[points.Length - 1].X, y0 + h), new PointF(points[0].X, y0 + h));
                path.CloseFigure();

                using (var fillBrush = new LinearGradientBrush(
                    new Point(0, y0), new Point(0, y0 + h),
                    fillGradientColor, Color.FromArgb(0, fillGradientColor)))
                {
                    g.FillPath(fillBrush, path);
                }
            }

            // 네온 곡선 그리기
            using (var linePen = new Pen(lineColor, 2f))
            {
                linePen.LineJoin = LineJoin.Round;
                g.DrawLines(linePen, points);
            }

            // 마지막 현재 지점 네온 포인트
            var lastPt = points[points.Length - 1];
            using (var dotBrush = new SolidBrush(lineColor))
            {
                g.FillEllipse(dotBrush, lastPt.X - 3.5f, lastPt.Y - 3.5f, 7f, 7f);
            }
        }
    }
    #endregion

    #region [ 3. 서버 접속 비중 도넛 원형 차트 (Donut Chart) ]
    /// <summary>
    /// 최다 접속 서버들의 비중을 시각화하는 모던 네온 도넛 차트
    /// </summary>
    public class DonutChartControl : Control
    {
        public class DonutSlice
        {
            public string Label { get; set; }
            public int Value { get; set; }
            public Color Color { get; set; }
        }

        private List<DonutSlice> _slices = new List<DonutSlice>();
        private string _centerTitle = "서버 접속";
        private string _centerSub = "0회";

        private static readonly Color[] Palette = new[]
        {
            Color.FromArgb(0, 195, 255),   // 네온 시안
            Color.FromArgb(46, 204, 113),  // 에메랄드 그린
            Color.FromArgb(241, 196, 15),  // 앰버 옐로우
            Color.FromArgb(155, 89, 182),  // 퍼플
            Color.FromArgb(230, 126, 34),  // 오렌지
            Color.FromArgb(127, 140, 141)  // 슬레이트 그레이
        };

        public DonutChartControl()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.ResizeRedraw, true);
            this.DoubleBuffered = true;
            this.Size = new Size(240, 240);
        }

        public void SetData(List<KeyValuePair<string, int>> serverCounts)
        {
            _slices.Clear();
            if (serverCounts == null || serverCounts.Count == 0)
            {
                _centerTitle = "접속 기록";
                _centerSub = "없음";
                Invalidate();
                return;
            }

            int total = serverCounts.Sum(x => x.Value);
            _centerTitle = "총 접속";
            _centerSub = $"{total}회";

            int colorIdx = 0;
            foreach (var kv in serverCounts.Take(5))
            {
                _slices.Add(new DonutSlice
                {
                    Label = kv.Key,
                    Value = kv.Value,
                    Color = Palette[colorIdx % Palette.Length]
                });
                colorIdx++;
            }

            int top5Sum = _slices.Sum(x => x.Value);
            int remain = total - top5Sum;
            if (remain > 0)
            {
                _slices.Add(new DonutSlice
                {
                    Label = "기타 서버",
                    Value = remain,
                    Color = Palette[5]
                });
            }

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 배경
            using (var brush = new SolidBrush(Color.FromArgb(37, 37, 40)))
            {
                g.FillRectangle(brush, ClientRectangle);
            }

            int total = _slices.Sum(s => s.Value);
            int size = Math.Min(Width, Height) - 30;
            int x = (Width - size) / 2;
            int y = (Height - size) / 2;
            Rectangle donutRect = new Rectangle(x, y, size, size);

            if (total == 0)
            {
                // 데이터 없음 원형 플레이스홀더
                using (var pen = new Pen(Color.FromArgb(60, 60, 65), 18))
                {
                    g.DrawEllipse(pen, x + 9, y + 9, size - 18, size - 18);
                }
            }
            else
            {
                // 슬라이스 그리기
                float startAngle = -90f;
                int thickness = 22;

                foreach (var slice in _slices)
                {
                    float sweepAngle = ((float)slice.Value / total) * 360f;
                    using (var pen = new Pen(slice.Color, thickness))
                    {
                        pen.StartCap = LineCap.Flat;
                        pen.EndCap = LineCap.Flat;
                        g.DrawArc(pen, x + thickness / 2, y + thickness / 2, size - thickness, size - thickness, startAngle, sweepAngle);
                    }
                    startAngle += sweepAngle;
                }
            }

            // 중앙 텍스트 영역
            using (var titleFont = new Font("맑은 고딕", 9F, FontStyle.Regular))
            using (var subFont = new Font("맑은 고딕", 13.5F, FontStyle.Bold))
            {
                var sz1 = g.MeasureString(_centerTitle, titleFont);
                var sz2 = g.MeasureString(_centerSub, subFont);

                float cx = Width / 2f;
                float cy = Height / 2f;

                g.DrawString(_centerTitle, titleFont, Brushes.DarkGray, cx - sz1.Width / 2f, cy - sz1.Height + 2);
                g.DrawString(_centerSub, subFont, Brushes.White, cx - sz2.Width / 2f, cy + 2);
            }

            // 테두리
            using (var pen = new Pen(Color.FromArgb(65, 65, 70), 1))
            {
                g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
        }
    }
    #endregion

    #region [ 4. 원형 프로그레스 게이지 (Circular Ring Gauge) ]
    /// <summary>
    /// 자동 로그인 성공률이나 시스템 헬스를 원형 링 게이지로 그리는 컨트롤
    /// </summary>
    public class CircularProgressControl : Control
    {
        private double _value = 0.0;
        private double _maximum = 100.0;
        private string _subTitle = "자동 로그인 성공률";
        private Color _ringColor = Color.FromArgb(46, 204, 113);

        public CircularProgressControl()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.ResizeRedraw, true);
            this.DoubleBuffered = true;
            this.Size = new Size(180, 180);
        }

        public double Value
        {
            get => _value;
            set { _value = Math.Max(0.0, Math.Min(_maximum, value)); Invalidate(); }
        }

        public string SubTitle
        {
            get => _subTitle;
            set { _subTitle = value; Invalidate(); }
        }

        public Color RingColor
        {
            get => _ringColor;
            set { _ringColor = value; Invalidate(); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 배경
            using (var brush = new SolidBrush(Color.FromArgb(37, 37, 40)))
            {
                g.FillRectangle(brush, ClientRectangle);
            }

            int size = Math.Min(Width, Height) - 24;
            int x = (Width - size) / 2;
            int y = (Height - size) / 2 - 8;
            int thickness = 14;

            // 트랙 배경 링
            using (var trackPen = new Pen(Color.FromArgb(55, 55, 60), thickness))
            {
                g.DrawEllipse(trackPen, x + thickness / 2, y + thickness / 2, size - thickness, size - thickness);
            }

            // 활성 아크
            float sweep = (float)(_value / _maximum * 360.0);
            if (sweep > 0.5f)
            {
                using (var arcPen = new Pen(_ringColor, thickness))
                {
                    arcPen.StartCap = LineCap.Round;
                    arcPen.EndCap = LineCap.Round;
                    g.DrawArc(arcPen, x + thickness / 2, y + thickness / 2, size - thickness, size - thickness, -90f, sweep);
                }
            }

            // 중앙 숫자 (예: 98.2%)
            string percentStr = $"{_value:F1}%";
            using (var numFont = new Font("맑은 고딕", 13.5F, FontStyle.Bold))
            using (var subFont = new Font("맑은 고딕", 9F, FontStyle.Regular))
            {
                var numSz = g.MeasureString(percentStr, numFont);
                var subSz = g.MeasureString(_subTitle, subFont);

                float cx = Width / 2f;
                float cy = y + size / 2f;

                using (var brush = new SolidBrush(Color.White))
                {
                    g.DrawString(percentStr, numFont, brush, cx - numSz.Width / 2f, cy - numSz.Height / 2f - 2);
                }

                using (var subBrush = new SolidBrush(Color.DarkGray))
                {
                    g.DrawString(_subTitle, subFont, subBrush, cx - subSz.Width / 2f, Height - 22);
                }
            }

            // 테두리
            using (var pen = new Pen(Color.FromArgb(65, 65, 70), 1))
            {
                g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
        }
    }
    #endregion
}
