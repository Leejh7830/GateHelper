using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace GateHelper.Dashboard
{
    #region [ 0. 대시보드 커스텀 컨트롤 전용 GDI+ 정적 캐시 ]
    /// <summary>
    /// 대시보드 커스텀 컨트롤 6종 전용 GDI+ 정적 캐시 (프레임당 GC 할당 0 보장)
    /// </summary>
    internal static class DashboardGdiCache
    {
        // 폰트 캐시
        public static readonly Font FontValueLarge = new Font("맑은 고딕", 13.5F, FontStyle.Bold);
        public static readonly Font FontBannerTitle = new Font("맑은 고딕", 11.5F, FontStyle.Bold);
        public static readonly Font FontTitleBold = new Font("맑은 고딕", 9.5F, FontStyle.Bold);
        public static readonly Font FontMainBold = new Font("맑은 고딕", 9.0F, FontStyle.Bold);
        public static readonly Font FontMainRegular = new Font("맑은 고딕", 9.0F, FontStyle.Regular);
        public static readonly Font FontSubBold = new Font("맑은 고딕", 8.5F, FontStyle.Bold);
        public static readonly Font FontSubRegular = new Font("맑은 고딕", 8.5F, FontStyle.Regular);
        public static readonly Font FontCatBold = new Font("맑은 고딕", 8.0F, FontStyle.Bold);
        public static readonly Font FontSmallBold = new Font("맑은 고딕", 7.5F, FontStyle.Bold);
        public static readonly Font FontSmallRegular = new Font("맑은 고딕", 7.5F, FontStyle.Regular);
        public static readonly Font FontHourRegular = new Font("맑은 고딕", 7.0F, FontStyle.Regular);
        public static readonly Font FontMicroRegular = new Font("맑은 고딕", 6.0F, FontStyle.Regular);

        // 기본 배경 브러시
        public static readonly SolidBrush BrushBgCard = new SolidBrush(Color.FromArgb(40, 40, 44));
        public static readonly SolidBrush BrushBgDark = new SolidBrush(Color.FromArgb(32, 32, 35));
        public static readonly SolidBrush BrushBgContent = new SolidBrush(Color.FromArgb(37, 37, 40));
        public static readonly SolidBrush BrushTrendBg = new SolidBrush(Color.FromArgb(32, 32, 36));
        public static readonly SolidBrush BrushOffSegment = new SolidBrush(Color.FromArgb(60, 60, 65));
        public static readonly SolidBrush BrushTitleGainsboro = new SolidBrush(Color.Gainsboro);
        public static readonly SolidBrush BrushWhite = new SolidBrush(Color.White);
        public static readonly SolidBrush BrushSilver = new SolidBrush(Color.Silver);
        public static readonly SolidBrush BrushDarkGray = new SolidBrush(Color.DarkGray);
        public static readonly SolidBrush BrushDimGray = new SolidBrush(Color.DimGray);
        public static readonly SolidBrush BrushLightGray = new SolidBrush(Color.LightGray);
        public static readonly SolidBrush BrushCyanText = new SolidBrush(Color.FromArgb(0, 195, 255));
        public static readonly SolidBrush BrushGoldAmber = new SolidBrush(Color.FromArgb(241, 196, 15));
        public static readonly SolidBrush BrushTrack50 = new SolidBrush(Color.FromArgb(50, 50, 55));
        public static readonly SolidBrush BrushCountFail = new SolidBrush(Color.FromArgb(240, 140, 130));

        // 고정 세그먼트 활성 브러시
        public static readonly SolidBrush BrushSegGreen = new SolidBrush(Color.FromArgb(46, 204, 113));
        public static readonly SolidBrush BrushSegYellow = new SolidBrush(Color.FromArgb(241, 196, 15));
        public static readonly SolidBrush BrushSegRed = new SolidBrush(Color.FromArgb(231, 76, 60));

        // 펜 캐시
        public static readonly Pen PenBorder60 = new Pen(Color.FromArgb(60, 60, 65), 1);
        public static readonly Pen PenBorder65 = new Pen(Color.FromArgb(65, 65, 70), 1);
        public static readonly Pen PenBorderCard = new Pen(Color.FromArgb(62, 62, 68), 1);
        public static readonly Pen PenDivider = new Pen(Color.FromArgb(48, 48, 52), 1);
        public static readonly Pen PenGridDot = new Pen(Color.FromArgb(48, 48, 54), 1) { DashStyle = DashStyle.Dot };
        public static readonly Pen PenGoldHour = new Pen(Color.Gold, 1.5f);
        public static readonly Pen PenTrack13 = new Pen(Color.FromArgb(45, 45, 50), 13);
        public static readonly Pen PenTrackNoData = new Pen(Color.FromArgb(55, 55, 60), 13);
    }
    #endregion

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
            g.FillRectangle(DashboardGdiCache.BrushBgCard, ClientRectangle);

            // 상단 타이틀 & 수치
            g.DrawString(_title, DashboardGdiCache.FontTitleBold, DashboardGdiCache.BrushTitleGainsboro, new PointF(10, 8));

            var valSize = g.MeasureString(_valueText, DashboardGdiCache.FontValueLarge);
            g.DrawString(_valueText, DashboardGdiCache.FontValueLarge, DashboardGdiCache.BrushWhite, new PointF(Width - valSize.Width - 10, 5));

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
                Brush segBrush;
                if (segRatio <= 0.60)
                {
                    segBrush = DashboardGdiCache.BrushSegGreen;
                }
                else if (segRatio <= 0.85)
                {
                    segBrush = DashboardGdiCache.BrushSegYellow;
                }
                else
                {
                    segBrush = DashboardGdiCache.BrushSegRed;
                }

                if (i < activeSegments)
                {
                    g.FillRectangle(segBrush, segRect);
                }
                else
                {
                    // 비활성 칸
                    g.FillRectangle(DashboardGdiCache.BrushOffSegment, segRect);
                }
            }

            // 하단 상태 문구 (동적 상태 색상)
            using (var statusBrush = new SolidBrush(_statusColor))
            {
                g.DrawString($"상태: {_statusText}", DashboardGdiCache.FontMainRegular, statusBrush, new PointF(10, 68));
            }

            // 하단 최대치 라벨
            string maxStr = $"기준 한도: {_maximum:N0} MB";
            var sz = g.MeasureString(maxStr, DashboardGdiCache.FontMainRegular);
            g.DrawString(maxStr, DashboardGdiCache.FontMainRegular, DashboardGdiCache.BrushDarkGray, new PointF(Width - sz.Width - 10, 68));

            // 테두리
            g.DrawRectangle(DashboardGdiCache.PenBorder65, 0, 0, Width - 1, Height - 1);
        }
    }
    #endregion

    #region [ 2. 실시간 CPU & RAM 꺾은선 트렌드 차트 (Sparkline Trend Chart) ]
    /// <summary>
    /// 최근 40개 샘플 동안의 CPU 점유율과 RAM 변화를 실시간 네온 곡선으로 그리는 고성능 차트
    /// </summary>
    public class RealtimeTrendChartControl : Control
    {
        private static readonly Pen PenRamCurve = new Pen(Color.FromArgb(46, 204, 113), 2f) { LineJoin = LineJoin.Round };
        private static readonly Pen PenCpuCurve = new Pen(Color.FromArgb(0, 210, 255), 2f) { LineJoin = LineJoin.Round };
        private static readonly SolidBrush BrushRamDot = new SolidBrush(Color.FromArgb(46, 204, 113));
        private static readonly SolidBrush BrushCpuDot = new SolidBrush(Color.FromArgb(0, 210, 255));

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
            g.FillRectangle(DashboardGdiCache.BrushTrendBg, ClientRectangle);

            // 2. 모안 눈금 격자선 (Grid lines)
            int gridSteps = 4;
            for (int i = 1; i < gridSteps; i++)
            {
                int y = Height * i / gridSteps;
                g.DrawLine(DashboardGdiCache.PenGridDot, 10, y, Width - 10, y);
            }

            // 차트 플롯 영역
            int padLeft = 12;
            int padRight = 12;
            int padTop = 32;
            int padBottom = 24;
            int plotW = Width - padLeft - padRight;
            int plotH = Height - padTop - padBottom;

            if (plotW <= 0 || plotH <= 0 || _cpuHistory.Count < 2) return;

            // 3. RAM 및 CPU 곡선 그리기 (초록 RAM 배경 곡선 + 파랑 CPU 전경 곡선)
            DrawCurve(g, _ramHistory, 0f, 512f, PenRamCurve, BrushRamDot, Color.FromArgb(18, 46, 204, 113), padLeft, padTop, plotW, plotH);
            DrawCurve(g, _cpuHistory, 0f, 30f, PenCpuCurve, BrushCpuDot, Color.FromArgb(32, 0, 210, 255), padLeft, padTop, plotW, plotH);

            // 4. 상단 헤더 & 범례 (Legend)
            g.DrawString("▲ 리소스 트렌드", DashboardGdiCache.FontTitleBold, DashboardGdiCache.BrushWhite, new PointF(10, 8));

            // 지표 텍스트
            string ramStr = $"RAM: {_currentRamMb:F0} MB";
            string cpuStr = $"CPU: {_currentCpu:F1}%";

            var ramSz = g.MeasureString(ramStr, DashboardGdiCache.FontMainBold);
            var cpuSz = g.MeasureString(cpuStr, DashboardGdiCache.FontMainBold);

            // 우측 끝에서부터 동적으로 안전 배치하여 절대 겹치지 않도록 처리
            float ramTextX = Width - ramSz.Width - 10;
            float ramDotX = ramTextX - 11;

            float cpuTextX = ramDotX - cpuSz.Width - 14;
            float cpuDotX = cpuTextX - 11;

            // CPU 지표 (시안)
            g.FillEllipse(BrushCpuDot, cpuDotX, 12, 7, 7);
            g.DrawString(cpuStr, DashboardGdiCache.FontMainBold, BrushCpuDot, new PointF(cpuTextX, 8));

            // RAM 지표 (그린)
            g.FillEllipse(BrushRamDot, ramDotX, 12, 7, 7);
            g.DrawString(ramStr, DashboardGdiCache.FontMainBold, BrushRamDot, new PointF(ramTextX, 8));

            // 하단 축 라벨
            g.DrawString("-40초", DashboardGdiCache.FontMainRegular, Brushes.Gray, new PointF(padLeft, Height - 18));
            g.DrawString("현재 (0초)", DashboardGdiCache.FontMainRegular, Brushes.Gray, new PointF(Width - padRight - 55, Height - 18));

            // 외곽선
            g.DrawRectangle(DashboardGdiCache.PenBorder65, 0, 0, Width - 1, Height - 1);
        }

        private void DrawCurve(Graphics g, List<float> data, float minVal, float maxVal, Pen linePen, Brush dotBrush, Color fillGradientColor, int x0, int y0, int w, int h)
        {
            if (data.Count < 2) return;

            PointF[] points = new PointF[data.Count];
            float stepX = (float)w / (data.Count - 1);
            float range = Math.Max(0.001f, maxVal - minVal);

            for (int i = 0; i < data.Count; i++)
            {
                float val = data[i];
                float norm = Math.Max(0f, Math.Min(1f, (val - minVal) / range));
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
            g.DrawLines(linePen, points);

            // 마지막 현재 지점 네온 포인트
            var lastPt = points[points.Length - 1];
            g.FillEllipse(dotBrush, lastPt.X - 3.5f, lastPt.Y - 3.5f, 7f, 7f);
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

        private readonly List<DonutSlice> _slices = new List<DonutSlice>();
        private string _centerTitle = "총 접속 횟수";
        private string _centerSub = "0회";
        private string _bottomTitle = "서버별 접속 분포";
        private string _bottomHighlight = "● 접속 이력 없음";

        public static readonly Color[] Palette = new[]
        {
            Color.FromArgb(0, 195, 255),   // 1위: 네온 시안
            Color.FromArgb(46, 204, 113),  // 2위: 에메랄드 그린
            Color.FromArgb(241, 196, 15),  // 3위: 앰버 옐로우
            Color.FromArgb(155, 89, 182),  // 4위: 퍼플
            Color.FromArgb(230, 126, 34),  // 5위: 오렌지
            Color.FromArgb(127, 140, 141)  // 6위+: 슬레이트 그레이
        };

        private static readonly Pen[] SlicePens = new[]
        {
            new Pen(Palette[0], 13) { StartCap = LineCap.Flat, EndCap = LineCap.Flat },
            new Pen(Palette[1], 13) { StartCap = LineCap.Flat, EndCap = LineCap.Flat },
            new Pen(Palette[2], 13) { StartCap = LineCap.Flat, EndCap = LineCap.Flat },
            new Pen(Palette[3], 13) { StartCap = LineCap.Flat, EndCap = LineCap.Flat },
            new Pen(Palette[4], 13) { StartCap = LineCap.Flat, EndCap = LineCap.Flat },
            new Pen(Palette[5], 13) { StartCap = LineCap.Flat, EndCap = LineCap.Flat }
        };

        public DonutChartControl()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.ResizeRedraw, true);
            this.DoubleBuffered = true;
            this.Size = new Size(190, 255);
        }

        public void SetData(List<KeyValuePair<string, int>> serverCounts)
        {
            _slices.Clear();
            if (serverCounts == null || serverCounts.Count == 0)
            {
                _centerSub = "0회";
                _centerTitle = "총 접속 횟수";
                _bottomTitle = "서버별 접속 분포";
                _bottomHighlight = "● 접속 이력 없음";
                Invalidate();
                return;
            }

            int total = serverCounts.Sum(x => x.Value);
            _centerSub = $"{total:N0}회";
            _centerTitle = "총 접속 횟수";
            _bottomTitle = "서버별 접속 분포";

            var top1 = serverCounts[0];
            string topServerName = top1.Key;
            _bottomHighlight = $"● 점유 1위: {topServerName}";

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
            g.FillRectangle(DashboardGdiCache.BrushBgContent, ClientRectangle);

            int total = _slices.Sum(s => s.Value);
            int ringSize = Math.Min(Width, Height) - 34;
            if (ringSize > 156) ringSize = 156;
            int ringX = (Width - ringSize) / 2;
            int ringY = 16;
            int thickness = 13;

            // 1. 도넛 링 그리기
            if (total == 0)
            {
                // 데이터 없음 원형 플레이스홀더 (트랙 Pen)
                g.DrawEllipse(DashboardGdiCache.PenTrackNoData, ringX + thickness / 2, ringY + thickness / 2, ringSize - thickness, ringSize - thickness);
            }
            else
            {
                // 트랙 배경 링
                g.DrawEllipse(DashboardGdiCache.PenTrack13, ringX + thickness / 2, ringY + thickness / 2, ringSize - thickness, ringSize - thickness);

                // 슬라이스 그리기 (-90도 상단부터 시계 방향 회전)
                float startAngle = -90f;
                for (int i = 0; i < _slices.Count; i++)
                {
                    var slice = _slices[i];
                    float sweepAngle = ((float)slice.Value / total) * 360f;
                    Pen pen = (i < SlicePens.Length) ? SlicePens[i] : SlicePens[SlicePens.Length - 1];
                    g.DrawArc(pen, ringX + thickness / 2, ringY + thickness / 2, ringSize - thickness, ringSize - thickness, startAngle, sweepAngle);
                    startAngle += sweepAngle;
                }
            }

            float cx = Width / 2f;
            float cy = ringY + ringSize / 2f;

            // 2. 링 내부: 총 접속수 & 설명 텍스트 (오른쪽 차트와 완벽 대칭)
            var numSz = g.MeasureString(_centerSub, DashboardGdiCache.FontValueLarge);
            var labelSz = g.MeasureString(_centerTitle, DashboardGdiCache.FontSubRegular);

            // 중앙 위: 숫자 (예: 15회)
            g.DrawString(_centerSub, DashboardGdiCache.FontValueLarge, DashboardGdiCache.BrushWhite, cx - numSz.Width / 2f, cy - numSz.Height + 2);

            // 중앙 아래: 설명 (예: 총 접속 횟수)
            g.DrawString(_centerTitle, DashboardGdiCache.FontSubRegular, DashboardGdiCache.BrushSilver, cx - labelSz.Width / 2f, cy + 5);

            // 3. 링 하단: 기능 명칭 & 최다 점유 서버 하이라이트
            string titleStr = _bottomTitle;
            string highlightStr = _bottomHighlight;

            var titleSz = g.MeasureString(titleStr, DashboardGdiCache.FontTitleBold);

            // 긴 서버명 말줄임 처리
            string displayHighlight = highlightStr;
            float maxTextW = Width - 16;
            if (g.MeasureString(displayHighlight, DashboardGdiCache.FontMainBold).Width > maxTextW)
            {
                while (displayHighlight.Length > 8 && g.MeasureString(displayHighlight + "...", DashboardGdiCache.FontMainBold).Width > maxTextW)
                {
                    displayHighlight = displayHighlight.Substring(0, displayHighlight.Length - 1);
                }
                displayHighlight += "...";
            }
            var hlSz = g.MeasureString(displayHighlight, DashboardGdiCache.FontMainBold);

            int bottomBaseY = ringY + ringSize + 10;

            // 타이틀 (서버별 접속 분포)
            g.DrawString(titleStr, DashboardGdiCache.FontTitleBold, DashboardGdiCache.BrushTitleGainsboro, cx - titleSz.Width / 2f, bottomBaseY);

            // 하이라이트 (1위 색상인 네온 시안)
            g.DrawString(displayHighlight, DashboardGdiCache.FontMainBold, DashboardGdiCache.BrushCyanText, cx - hlSz.Width / 2f, bottomBaseY + 22);

            // 테두리
            g.DrawRectangle(DashboardGdiCache.PenBorder65, 0, 0, Width - 1, Height - 1);
        }
    }
    #endregion

    #region [ 4. 원형 프로그레스 게이지 (Circular Ring Gauge) ]
    /// <summary>
    /// 자동 로그인 성공률이나 시스템 헬스를 원형 링 게이지로 그리는 컨트롤
    /// </summary>
    public class CircularProgressControl : Control
    {
        private static readonly Pen PenArcGreen = new Pen(Color.FromArgb(46, 204, 113), 13) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        private static readonly Pen PenArcYellow = new Pen(Color.FromArgb(241, 196, 15), 13) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        private static readonly Pen PenArcRed = new Pen(Color.FromArgb(231, 76, 60), 13) { StartCap = LineCap.Round, EndCap = LineCap.Round };

        private double _value = 0.0;
        private double _maximum = 100.0;
        private int _successCount = 0;
        private int _failCount = 0;
        private string _savedTimeText = "0분";
        private string _titleText = "GateOne 자동 로그인";
        private Color _ringColor = Color.FromArgb(46, 204, 113);

        public CircularProgressControl()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.ResizeRedraw, true);
            this.DoubleBuffered = true;
            this.Size = new Size(190, 255);
        }

        public double Value
        {
            get => _value;
            set { _value = Math.Max(0.0, Math.Min(_maximum, value)); Invalidate(); }
        }

        public Color RingColor
        {
            get => _ringColor;
            set { _ringColor = value; Invalidate(); }
        }

        public string SubTitle
        {
            get => _savedTimeText;
            set { _savedTimeText = value; Invalidate(); }
        }

        public string TitleText
        {
            get => _titleText;
            set { _titleText = value; Invalidate(); }
        }

        public void SetProgress(double rate, int successCount, int failCount, string savedTimeFormatted, string title = "GateOne 자동 로그인")
        {
            _value = Math.Max(0.0, Math.Min(_maximum, rate));
            _successCount = successCount;
            _failCount = failCount;
            _savedTimeText = string.IsNullOrWhiteSpace(savedTimeFormatted) ? "0분" : savedTimeFormatted;
            _titleText = string.IsNullOrWhiteSpace(title) ? "GateOne 자동 로그인" : title;

            if (_value >= 90.0) _ringColor = Color.FromArgb(46, 204, 113);
            else if (_value >= 70.0) _ringColor = Color.FromArgb(241, 196, 15);
            else _ringColor = Color.FromArgb(231, 76, 60);

            Invalidate();
        }

        public void SetProgress(double rate, string mainText, string title, string subText)
        {
            _value = Math.Max(0.0, Math.Min(_maximum, rate));
            _titleText = title ?? "GateOne 자동 로그인";
            _savedTimeText = subText ?? "";
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 배경
            g.FillRectangle(DashboardGdiCache.BrushBgContent, ClientRectangle);

            int ringSize = Math.Min(Width, Height) - 34;
            if (ringSize > 156) ringSize = 156;
            int ringX = (Width - ringSize) / 2;
            int ringY = 16;
            int thickness = 13;

            // 1. 트랙 배경 링
            g.DrawEllipse(DashboardGdiCache.PenTrackNoData, ringX + thickness / 2, ringY + thickness / 2, ringSize - thickness, ringSize - thickness);

            // 2. 활성 아크
            float sweep = (float)(_value / _maximum * 360.0);
            if (sweep > 0.5f)
            {
                Pen arcPen = (_value >= 90.0) ? PenArcGreen : ((_value >= 70.0) ? PenArcYellow : PenArcRed);
                g.DrawArc(arcPen, ringX + thickness / 2, ringY + thickness / 2, ringSize - thickness, ringSize - thickness, -90f, sweep);
            }

            float cx = Width / 2f;
            float cy = ringY + ringSize / 2f;

            // 3. 링 내부: 성공률 & 분리된 카운트
            string percentStr = $"{_value:F1}%";
            string countStr = $"성공 {_successCount}회 · 실패 {_failCount}회";

            var numSz = g.MeasureString(percentStr, DashboardGdiCache.FontValueLarge);
            var countSz = g.MeasureString(countStr, DashboardGdiCache.FontSubRegular);

            // 중앙 위: 퍼센트 (100.0%)
            g.DrawString(percentStr, DashboardGdiCache.FontValueLarge, DashboardGdiCache.BrushWhite, cx - numSz.Width / 2f, cy - numSz.Height + 2);

            // 중앙 아래: 명확한 카운트 (성공 3회 · 실패 0회)
            Brush countBrush = _failCount > 0 ? DashboardGdiCache.BrushCountFail : DashboardGdiCache.BrushSilver;
            g.DrawString(countStr, DashboardGdiCache.FontSubRegular, countBrush, cx - countSz.Width / 2f, cy + 5);

            // 4. 링 하단: 기능 명칭 & 절약 시간 연동 (후보 A)
            string titleStr = _titleText;
            string savedStr = $"● 누적 절약: {_savedTimeText}";

            var titleSz = g.MeasureString(titleStr, DashboardGdiCache.FontTitleBold);
            var savedSz = g.MeasureString(savedStr, DashboardGdiCache.FontMainBold);

            int bottomBaseY = ringY + ringSize + 10;

            // 타이틀 (GateOne 자동 로그인)
            g.DrawString(titleStr, DashboardGdiCache.FontTitleBold, DashboardGdiCache.BrushTitleGainsboro, cx - titleSz.Width / 2f, bottomBaseY);

            // 절약 시간 하이라이트 (골드 앰버)
            g.DrawString(savedStr, DashboardGdiCache.FontMainBold, DashboardGdiCache.BrushGoldAmber, cx - savedSz.Width / 2f, bottomBaseY + 22);

            // 테두리
            g.DrawRectangle(DashboardGdiCache.PenBorder65, 0, 0, Width - 1, Height - 1);
        }
    }
    #endregion

    #region [ 5. 접속 활동 리듬 위젯: 최근 7일 주간 잔디 & 오늘 24시간 타임라인 (HourlyHeatmapControl) ]
    public enum RhythmMode
    {
        Weekly7Days,
        Today24Hours
    }

    /// <summary>
    /// 최근 7일 주간 접속 활동 리듬(GitHub 스타일 풍성한 잔디)과 오늘 24시간 타임라인을 원클릭으로 토글 전환하는 위젯
    /// </summary>
    public class HourlyHeatmapControl : Control
    {
        private static readonly SolidBrush BrushToggleBgHover = new SolidBrush(Color.FromArgb(52, 54, 62));
        private static readonly SolidBrush BrushToggleBgNormal = new SolidBrush(Color.FromArgb(42, 43, 49));
        private static readonly Pen PenToggleBorderHover = new Pen(Color.FromArgb(0, 220, 255), 1f);
        private static readonly Pen PenToggleBorderNormal = new Pen(Color.FromArgb(70, 72, 80), 1f);
        private static readonly SolidBrush BrushToggleForeHover = new SolidBrush(Color.FromArgb(0, 240, 255));
        private static readonly SolidBrush BrushToggleForeNormal = new SolidBrush(Color.FromArgb(175, 185, 200));

        private static readonly SolidBrush BrushGrass0 = new SolidBrush(Color.FromArgb(40, 41, 46));
        private static readonly SolidBrush BrushGrass1 = new SolidBrush(Color.FromArgb(23, 76, 44));
        private static readonly SolidBrush BrushGrass2 = new SolidBrush(Color.FromArgb(30, 117, 59));
        private static readonly SolidBrush BrushGrass3 = new SolidBrush(Color.FromArgb(39, 163, 79));
        private static readonly SolidBrush BrushGrass4 = new SolidBrush(Color.FromArgb(50, 200, 96));

        private static readonly Pen PenCardToday = new Pen(Color.FromArgb(0, 220, 255), 1.5f);
        private static readonly Pen PenCardHover = new Pen(Color.FromArgb(140, 160, 180), 1f);
        private static readonly Pen PenCardNormal = new Pen(Color.FromArgb(58, 60, 66), 1f);

        private static readonly SolidBrush BrushMicroActive = new SolidBrush(Color.FromArgb(46, 204, 113));
        private static readonly SolidBrush BrushMicroOff = new SolidBrush(Color.FromArgb(30, 31, 35));
        private static readonly Pen PenMicroActive = new Pen(Color.FromArgb(30, 140, 75), 1f);
        private static readonly Pen PenMicroOff = new Pen(Color.FromArgb(48, 49, 54), 1f);

        private static readonly SolidBrush BrushBlock0 = new SolidBrush(Color.FromArgb(42, 43, 48));
        private static readonly SolidBrush BrushBlock1 = new SolidBrush(Color.FromArgb(0, 110, 140));
        private static readonly SolidBrush BrushBlock2 = new SolidBrush(Color.FromArgb(0, 180, 210));
        private static readonly SolidBrush BrushBlock3 = new SolidBrush(Color.FromArgb(0, 240, 255));

        private static readonly StringFormat SfCenter = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        private static readonly StringFormat SfRight = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Near };

        public RhythmMode CurrentMode { get; set; } = RhythmMode.Weekly7Days;

        private List<DailyActivityItem> _weeklyItems = new List<DailyActivityItem>();
        private int[] _todayHourly = new int[24];
        private int _screenUnlockCount = 0;
        private int _hoveredIndex = -1;
        private bool _isHoverToggle = false;
        private Rectangle _toggleBtnRect = Rectangle.Empty;
        private readonly ToolTip _tip = new ToolTip();

        private const int ToggleBtnY = 4;
        private const int ToggleBtnH = 18;
        private const int ContentStartY = 30; // 토글 버튼과 요일 카드 간의 세로 여백을 8px로 확보
        private const int CardH = 38;
        private const int BlockH = 22;

        public HourlyHeatmapControl()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.ResizeRedraw, true);
            this.DoubleBuffered = true;
            this.Size = new Size(755, 74);
            this.Font = new Font("맑은 고딕", 8.5F, FontStyle.Regular);

            _tip.InitialDelay = 250;
            _tip.ReshowDelay = 100;
            _tip.AutoPopDelay = 6000;

            this.MouseMove += HourlyHeatmapControl_MouseMove;
            this.MouseClick += HourlyHeatmapControl_MouseClick;
            this.MouseLeave += (s, e) =>
            {
                _hoveredIndex = -1;
                _isHoverToggle = false;
                this.Cursor = Cursors.Default;
                _tip.Hide(this);
                Invalidate();
            };
        }

        public void SetData(List<DailyActivityItem> weeklyItems, int[] todayHourly, int screenUnlockCount)
        {
            if (weeklyItems != null)
            {
                _weeklyItems = weeklyItems;
            }
            if (todayHourly != null && todayHourly.Length == 24)
            {
                _todayHourly = (int[])todayHourly.Clone();
            }
            _screenUnlockCount = screenUnlockCount;
            Invalidate();
        }

        public void SetData(int[] hourlyCounts, int screenUnlockCount)
        {
            if (hourlyCounts != null && hourlyCounts.Length == 24)
            {
                _todayHourly = (int[])hourlyCounts.Clone();
            }
            _screenUnlockCount = screenUnlockCount;
            Invalidate();
        }

        private void HourlyHeatmapControl_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && _toggleBtnRect.Contains(e.Location))
            {
                CurrentMode = (CurrentMode == RhythmMode.Weekly7Days)
                    ? RhythmMode.Today24Hours
                    : RhythmMode.Weekly7Days;
                _hoveredIndex = -1;
                _tip.Hide(this);
                Invalidate();
            }
        }

        private void HourlyHeatmapControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (_toggleBtnRect.Contains(e.Location))
            {
                if (!_isHoverToggle)
                {
                    _isHoverToggle = true;
                    this.Cursor = Cursors.Hand;
                    _hoveredIndex = -1;
                    string hint = (CurrentMode == RhythmMode.Weekly7Days)
                        ? "오늘 24시간 타임라인으로 화면을 전환합니다."
                        : "최근 7일 주간 활동 잔디로 화면을 전환합니다.";
                    _tip.SetToolTip(this, hint);
                    Invalidate();
                }
                return;
            }
            else
            {
                if (_isHoverToggle)
                {
                    _isHoverToggle = false;
                    this.Cursor = Cursors.Default;
                    _tip.Hide(this);
                    Invalidate();
                }
            }

            int newHover = -1;

            if (CurrentMode == RhythmMode.Weekly7Days)
            {
                int startX = 12;
                int gap = 6;
                int count = Math.Min(7, _weeklyItems.Count);
                if (count > 0)
                {
                    int totalCardW = Width - startX - 12 - (Math.Max(0, count - 1) * gap);
                    int cardW = Math.Max(30, totalCardW / count);
                    int startY = ContentStartY;
                    int cardH = CardH;

                    for (int i = 0; i < count; i++)
                    {
                        int cx = startX + i * (cardW + gap);
                        var rect = new Rectangle(cx, startY, cardW, cardH);
                        if (rect.Contains(e.Location))
                        {
                            newHover = i;
                            break;
                        }
                    }
                }
            }
            else
            {
                int blockW = (Width - 28) / 24;
                int startX = 14;
                int startY = ContentStartY;
                int blockH = BlockH;

                for (int h = 0; h < 24; h++)
                {
                    int bx = startX + (h * blockW);
                    var rect = new Rectangle(bx, startY, blockW - 2, blockH);
                    if (rect.Contains(e.Location))
                    {
                        newHover = h;
                        break;
                    }
                }
            }

            if (newHover != _hoveredIndex)
            {
                _hoveredIndex = newHover;
                if (_hoveredIndex >= 0)
                {
                    if (CurrentMode == RhythmMode.Weekly7Days && _hoveredIndex < _weeklyItems.Count)
                    {
                        var item = _weeklyItems[_hoveredIndex];
                        int dawn = 0, morn = 0, aft = 0, night = 0;
                        if (item.HourlyCounts != null && item.HourlyCounts.Length == 24)
                        {
                            for (int h = 0; h < 6; h++) dawn += item.HourlyCounts[h];
                            for (int h = 6; h < 12; h++) morn += item.HourlyCounts[h];
                            for (int h = 12; h < 18; h++) aft += item.HourlyCounts[h];
                            for (int h = 18; h < 24; h++) night += item.HourlyCounts[h];
                        }
                        string tipText = $"{item.Date:yyyy-MM-dd} ({item.Date:ddd}){(item.IsToday ? " [오늘]" : "")}\n" +
                                         $"서버 접속 합계: {item.TotalCount:N0}회\n" +
                                         $"● 새벽(00~06): {dawn}회  ● 오전(06~12): {morn}회\n" +
                                         $"● 오후(12~18): {aft}회  ● 야간(18~24): {night}회";
                        _tip.SetToolTip(this, tipText);
                    }
                    else if (CurrentMode == RhythmMode.Today24Hours && _hoveredIndex < 24)
                    {
                        int cnt = _todayHourly[_hoveredIndex];
                        _tip.SetToolTip(this, $"{_hoveredIndex:D2}:00 ~ {_hoveredIndex:D2}:59\n오늘 서버 접속: {cnt:N0}회");
                    }
                }
                else
                {
                    _tip.Hide(this);
                }
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 1. 배경
            g.FillRectangle(DashboardGdiCache.BrushBgDark, ClientRectangle);

            // 2. 우측 상단 모드 전환 토글 버튼
            int btnW = 142;
            _toggleBtnRect = new Rectangle(Width - btnW - 12, ToggleBtnY, btnW, ToggleBtnH);

            Brush btnBrush = _isHoverToggle ? BrushToggleBgHover : BrushToggleBgNormal;
            Pen pen = _isHoverToggle ? PenToggleBorderHover : PenToggleBorderNormal;
            Brush foreBrush = _isHoverToggle ? BrushToggleForeHover : BrushToggleForeNormal;

            g.FillRectangle(btnBrush, _toggleBtnRect);
            g.DrawRectangle(pen, _toggleBtnRect);
            string toggleText = (CurrentMode == RhythmMode.Weekly7Days)
                ? "◈ 오늘 24시간 보기 ❯"
                : "◈ 최근 7일 주간 보기 ❯";
            g.DrawString(toggleText, DashboardGdiCache.FontSmallBold, foreBrush, _toggleBtnRect, SfCenter);

            // 3. 모드별 렌더링
            if (CurrentMode == RhythmMode.Weekly7Days)
            {
                PaintWeeklyMode(g);
            }
            else
            {
                PaintToday24HoursMode(g);
            }

            // 4. 컨트롤 외곽 테두리
            g.DrawRectangle(DashboardGdiCache.PenBorder60, 0, 0, Width - 1, Height - 1);
        }

        private void PaintWeeklyMode(Graphics g)
        {
            int totalWeek = _weeklyItems.Sum(x => x.TotalCount);
            string title = _screenUnlockCount > 0
                ? $"◈ 최근 7일 주간 활동 리듬 (주간 접속 {totalWeek:N0}회 · 잠금 해제 {_screenUnlockCount:N0}회)"
                : $"◈ 최근 7일 주간 활동 리듬 (주간 총 {totalWeek:N0}회 접속)";

            g.DrawString(title, DashboardGdiCache.FontSubBold, DashboardGdiCache.BrushLightGray, 14, 5);

            int count = Math.Min(7, _weeklyItems.Count);
            if (count == 0) return;

            int startX = 12;
            int gap = 6;
            int totalCardW = Width - startX - 12 - (Math.Max(0, count - 1) * gap);
            int cardW = Math.Max(30, totalCardW / count);
            int startY = ContentStartY;
            int cardH = CardH;

            for (int i = 0; i < count; i++)
            {
                var item = _weeklyItems[i];
                int cx = startX + i * (cardW + gap);
                var cardRect = new Rectangle(cx, startY, cardW, cardH);

                // 카드 배경색 (잔디 농도)
                int cnt = item.TotalCount;
                Brush cardBrush;
                if (cnt == 0) cardBrush = BrushGrass0;
                else if (cnt <= 3) cardBrush = BrushGrass1;
                else if (cnt <= 8) cardBrush = BrushGrass2;
                else if (cnt <= 15) cardBrush = BrushGrass3;
                else cardBrush = BrushGrass4;

                g.FillRectangle(cardBrush, cardRect);

                // 카드 테두리
                Pen borderPen;
                if (item.IsToday)
                {
                    borderPen = PenCardToday;
                }
                else if (i == _hoveredIndex)
                {
                    borderPen = PenCardHover;
                }
                else
                {
                    borderPen = PenCardNormal;
                }

                g.DrawRectangle(borderPen, cardRect);

                // 날짜 텍스트 (좌측 상단)
                Brush dateBrush = item.IsToday ? Brushes.Cyan : Brushes.WhiteSmoke;
                g.DrawString(item.DateLabel ?? "", DashboardGdiCache.FontSmallBold, dateBrush, cx + 5, startY + 3);

                // 접속 횟수 (우측 상단)
                string countStr = $"{cnt}회";
                Brush countBrush;
                if (cnt == 0) countBrush = Brushes.Gray;
                else if (item.IsToday) countBrush = Brushes.Gold;
                else countBrush = Brushes.LightGreen;

                g.DrawString(countStr, DashboardGdiCache.FontSmallBold, countBrush, new RectangleF(cx + 5, startY + 3, cardW - 10, 14), SfRight);

                // 하단 4대 시간대 마이크로 큐브 (새벽, 오전, 오후, 야간)
                int microW = Math.Max(12, (cardW - 10 - 6) / 4);
                int microH = 12;
                int microY = startY + 22;
                string[] quarterLabels = { "새", "오", "후", "야" };

                for (int q = 0; q < 4; q++)
                {
                    int qCnt = 0;
                    if (item.HourlyCounts != null && item.HourlyCounts.Length == 24)
                    {
                        for (int h = q * 6; h < (q + 1) * 6; h++)
                        {
                            qCnt += item.HourlyCounts[h];
                        }
                    }

                    int qx = cx + 5 + q * (microW + 2);
                    var qRect = new Rectangle(qx, microY, microW, microH);

                    Brush qBrush = qCnt > 0 ? BrushMicroActive : BrushMicroOff;
                    Pen qPen = qCnt > 0 ? PenMicroActive : PenMicroOff;

                    g.FillRectangle(qBrush, qRect);
                    g.DrawRectangle(qPen, qRect);

                    Brush qTextBrush = qCnt > 0 ? Brushes.White : Brushes.DimGray;
                    g.DrawString(quarterLabels[q], DashboardGdiCache.FontMicroRegular, qTextBrush, qRect, SfCenter);
                }
            }
        }

        private void PaintToday24HoursMode(Graphics g)
        {
            int todayTotal = _todayHourly.Sum();
            int currentHour = DateTime.Now.Hour;
            string title = _screenUnlockCount > 0
                ? $"◈ 오늘 24시간 타임라인 (오늘 접속 {todayTotal:N0}회 · 잠금 해제 {_screenUnlockCount:N0}회 · 현재 {currentHour:D2}시)"
                : $"◈ 오늘 24시간 타임라인 (오늘 총 {todayTotal:N0}회 접속 · 현재 {currentHour:D2}시)";

            g.DrawString(title, DashboardGdiCache.FontSubBold, DashboardGdiCache.BrushLightGray, 14, 5);

            int blockW = (Width - 28) / 24;
            int startX = 14;
            int startY = ContentStartY;
            int blockH = BlockH;
            int maxVal = _todayHourly.Max();

            for (int h = 0; h < 24; h++)
            {
                int bx = startX + (h * blockW);
                int count = _todayHourly[h];

                Brush blockBrush;
                if (count == 0)
                {
                    blockBrush = BrushBlock0;
                }
                else
                {
                    double ratio = (double)count / Math.Max(2, maxVal);
                    if (ratio <= 0.33) blockBrush = BrushBlock1;
                    else if (ratio <= 0.66) blockBrush = BrushBlock2;
                    else blockBrush = BrushBlock3;
                }

                g.FillRectangle(blockBrush, bx, startY, blockW - 2, blockH);

                if (h == currentHour)
                {
                    g.DrawRectangle(DashboardGdiCache.PenGoldHour, bx - 1, startY - 1, blockW - 1, blockH + 1);
                }

                if (h % 3 == 0 || h == currentHour)
                {
                    string hStr = $"{h:D2}";
                    g.DrawString(hStr, DashboardGdiCache.FontHourRegular, (h == currentHour ? Brushes.Gold : Brushes.Gray), new PointF(bx - 1, startY + blockH + 2));
                }
            }
        }
    }
    #endregion

    #region [ 6. 종합 마스터리 랭크 2단 와이드 HUD 배너 (MasteryBannerControl) ]
    /// <summary>
    /// 1층: 전체 업적 진행도 (종합 랭크 + 풀 와이드 네온 AP 게이지 바)
    /// 2층: 4대 분야별 레벨 & 칭호 박스 (174px 너비 4등분으로 긴 칭호도 잘림 없이 완벽 노출)
    /// </summary>
    public class MasteryBannerControl : Control
    {
        private MasteryTierInfo _masteryInfo;
        private List<CategoryTierInfo> _categoryTiers;

        public MasteryBannerControl()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.ResizeRedraw, true);
            this.DoubleBuffered = true;
            this.Size = new Size(755, 116);
            this.Font = new Font("맑은 고딕", 9F, FontStyle.Regular);
        }

        public void SetData(MasteryTierInfo mastery, List<CategoryTierInfo> categoryTiers)
        {
            _masteryInfo = mastery;
            _categoryTiers = categoryTiers;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // 전체 배경
            g.FillRectangle(DashboardGdiCache.BrushBgDark, 0, 0, Width, Height);

            var tier = _masteryInfo ?? new MasteryTierInfo
            {
                TierName = "★ Tier 1: Novice",
                TierTitle = "샌드박스 입문 요원",
                AccentColor = Color.FromArgb(205, 127, 50),
                TotalEarnedPoints = 0,
                MaxPoints = 520,
                UnlockedCount = 0,
                TotalCount = 15
            };

            // 1. 상단 3px 네온 액센트 탑 라인
            using (var lineBrush = new LinearGradientBrush(
                new Point(0, 0), new Point(Width, 0),
                tier.AccentColor, Color.FromArgb(30, tier.AccentColor)))
            {
                g.FillRectangle(lineBrush, 0, 0, Width, 3);
            }

            // -----------------------------------------------------------------
            // [ 1층: 전체 업적 진행도 (Y: 4 ~ 48) ]
            // -----------------------------------------------------------------
            // 좌측: 종합 랭크 이름 + 칭호 설명
            float titleRightX = 250f;
            using (var titleBrush = new SolidBrush(tier.AccentColor))
            {
                g.DrawString(tier.TierName, DashboardGdiCache.FontBannerTitle, titleBrush, 14, 7);
                var titleSz = g.MeasureString(tier.TierName, DashboardGdiCache.FontBannerTitle);
                titleRightX = 14 + titleSz.Width + 10;
            }

            g.DrawString($"({tier.TierTitle})", DashboardGdiCache.FontSubRegular, DashboardGdiCache.BrushDarkGray, titleRightX, 11);

            // 우측: 점수 및 달성 개수
            string scoreText = $"진척도: {tier.TotalEarnedPoints} / {tier.MaxPoints} AP ({tier.ProgressPercent:F1}%)  •  도전과제 달성: {tier.UnlockedCount} / {tier.TotalCount}개";
            var scoreSz = g.MeasureString(scoreText, DashboardGdiCache.FontSubRegular);
            g.DrawString(scoreText, DashboardGdiCache.FontSubRegular, DashboardGdiCache.BrushTitleGainsboro, Width - scoreSz.Width - 16, 11);

            // 풀 와이드 네온 게이지 바 (X: 16, Y: 33, W: Width - 32, H: 6)
            int barX = 16;
            int barY = 33;
            int barW = Width - 32;
            int barH = 6;

            g.FillRectangle(DashboardGdiCache.BrushTrack50, barX, barY, barW, barH);

            float fillW = barW * (tier.ProgressPercent / 100f);
            if (fillW > 0)
            {
                using (var fillBrush = new LinearGradientBrush(
                    new Point(barX, barY), new Point(barX + barW, barY),
                    tier.AccentColor, Color.FromArgb(Math.Min(255, tier.AccentColor.R + 60),
                                                    Math.Min(255, tier.AccentColor.G + 60),
                                                    Math.Min(255, tier.AccentColor.B + 60))))
                {
                    g.FillRectangle(fillBrush, barX, barY, fillW, barH);
                }
            }

            // 구분선 (1층과 2층 사이)
            g.DrawLine(DashboardGdiCache.PenDivider, 16, 47, Width - 16, 47);

            // -----------------------------------------------------------------
            // [ 2층: 4대 분야별 레벨 & 칭호 카드 4개 가로 분할 (Y: 53 ~ 107) ]
            // -----------------------------------------------------------------
            if (_categoryTiers != null && _categoryTiers.Count > 0)
            {
                int cardStartX = 16;
                int cardGap = 8;
                int totalAvailW = Width - 32;
                int catCount = Math.Max(1, Math.Min(_categoryTiers.Count, 5));
                int cardW = (totalAvailW - (cardGap * (catCount - 1))) / catCount;
                int cardH = 55;
                int cardY = 52;

                string[] icons = { "🧩", "⚡", "🔍", "⭐", "🚀" };

                for (int i = 0; i < _categoryTiers.Count && i < 5; i++)
                {
                    var cat = _categoryTiers[i];
                    int cx = cardStartX + (i * (cardW + cardGap));

                    // 카드 배경 및 얇은 테두리
                    g.FillRectangle(DashboardGdiCache.BrushBgCard, cx, cardY, cardW, cardH);
                    g.DrawRectangle(DashboardGdiCache.PenBorderCard, cx, cardY, cardW, cardH);

                    // 좌측 세로 3px 티어 컬러 액센트 바
                    using (var accentBrush = new SolidBrush(cat.TierColor))
                    {
                        g.FillRectangle(accentBrush, cx, cardY, 3, cardH);
                    }

                    // 1줄: 카테고리 헤더
                    string icon = (i < icons.Length) ? icons[i] : "🏆";
                    g.DrawString($"{icon} {cat.CategoryName}", DashboardGdiCache.FontCatBold, DashboardGdiCache.BrushLightGray, cx + 8, cardY + 5);

                    // 2줄: 티어 명칭
                    using (var tierBrush = new SolidBrush(cat.TierColor))
                    {
                        g.DrawString(cat.CurrentTierName, DashboardGdiCache.FontMainBold, tierBrush, cx + 8, cardY + 20);
                    }

                    // 3줄: 진행 실적
                    string countText;
                    if (cat.Category == AchievementCategory.AiTrend)
                        countText = $"찜 등록 {cat.CurrentCount:N0}개";
                    else if (cat.Category == AchievementCategory.CoreFront)
                        countText = $"작전 승리 {cat.CurrentCount:N0}회";
                    else
                        countText = $"퍼즐 클리어 {cat.CurrentCount:N0}회";
                    g.DrawString(countText, DashboardGdiCache.FontSmallRegular, DashboardGdiCache.BrushDarkGray, cx + 8, cardY + 37);
                }
            }

            // 바깥 테두리
            g.DrawRectangle(DashboardGdiCache.PenBorder60, 0, 0, Width - 1, Height - 1);
        }
    }
    #endregion
}
