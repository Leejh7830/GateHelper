using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GateHelper
{
    public partial class SignalLinkControl : UserControl
    {
        private SignalManager _manager = new SignalManager();

        private List<Point> _currentPath = new List<Point>();
        private Dictionary<int, List<Point>> _completedPaths = new Dictionary<int, List<Point>>();

        private bool _isDragging = false;
        private int _activeColor = 0;
        private Panel pnlDifficulty;

        private int _gridSize = 5;      // 현재 그리드 크기 (5x5 등)
        private int _cellSize = 60;     // 한 칸의 크기 (픽셀)
        private int _offsetX = 50;      // 그리드가 그려질 시작 X 좌표
        private int _offsetY = 50;      // 그리드가 그려질 시작 Y 좌표
 

        private Timer _gameTimer; // 타이머
        private int _playTimeSeconds;
        private string _currentDifficulty;
        private bool _isGameActive = false;

        private bool _showHint = false; // 힌트 표시 여부
        private int _hintColorID = -1; // 현재 힌트로 보여줄 색상의 ID (-1은 없음)
        private System.Threading.CancellationTokenSource _hintCts; // 힌트 취소용 토큰

        // 난이도 조절용
        public struct DifficultyConfig
        {
            public int GridSize;
            public int PairCount;

            public DifficultyConfig(int size, int pairs)
            {
                GridSize = size;
                PairCount = pairs;
            }
        }

        private Dictionary<string, DifficultyConfig> _configs = new Dictionary<string, DifficultyConfig>
        {
            { "Easy", new DifficultyConfig(5, 3) },
            { "Normal", new DifficultyConfig(6, 4) },
            { "Hard", new DifficultyConfig(8, 6) },
            { "Extreme", new DifficultyConfig(10, 8) }
        };



        public SignalLinkControl()
        {
            InitializeComponent();

            // 1. 기본 스타일 설정
            this.DoubleBuffered = true;
            this.BackColor = Color.FromArgb(30, 30, 30);

            // 2. 가리기용 패널 설정 (이게 버튼들을 가리고 있을 수 있음)
            pnlDifficulty = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(35, 35, 35),
                Name = "pnlDifficulty"
            };
            this.Controls.Add(pnlDifficulty);

            // 3. [핵심] 난이도 버튼들을 패널의 자식으로 즉시 편입
            // 부모가 바뀌면 Location은 패널 기준으로 자동 계산됩니다.
            btnEasy.Parent = pnlDifficulty;
            btnNormal.Parent = pnlDifficulty;
            btnHard.Parent = pnlDifficulty;
            btnExtreme.Parent = pnlDifficulty;

            // 4. [Z-Order 정리] 누가 누구 위에 보일지 명확히 정하기
            pnlDifficulty.BringToFront(); // 일단 패널을 가장 앞으로

            // 패널 "내부"에서 버튼들을 앞으로 가져오기
            btnEasy.BringToFront();
            btnNormal.BringToFront();
            btnHard.BringToFront();
            btnExtreme.BringToFront();

            // 패널 "외부"에서 리셋/백 버튼을 가장 앞으로 (패널을 뚫고 보여야 하니까요)
            btnReset.BringToFront();
            btnBack.BringToFront();

            // 5. 타이머 및 기타 로직 (기존과 동일)
            _gameTimer = new Timer { Interval = 1000 };
            _gameTimer.Tick += (s, e) => { if (_isGameActive) _playTimeSeconds++; };
        }



        private void StartGame(string difficulty)
        {
            _currentDifficulty = difficulty;
            

            var config = _configs[difficulty];
            _gridSize = config.GridSize;

            _cellSize = Math.Min(this.Width, this.Height - 100) / _gridSize;
            _offsetX = (this.Width - (_gridSize * _cellSize)) / 2;
            _offsetY = 50;

            _manager.InitializeGrid(_gridSize, config.PairCount);

            _completedPaths.Clear();
            _currentPath.Clear();

            // 게임 상태 초기화 및 타이머 시작
            _playTimeSeconds = 0;
            _isGameActive = true;
            _gameTimer.Start();
            ResetHintState(); // 힌트 초기화

            pnlDifficulty.Visible = false;
            this.Invalidate();
        }


        private void btnEasy_Click(object sender, EventArgs e) => StartGame("Easy");
        private void btnNormal_Click(object sender, EventArgs e) => StartGame("Normal");
        private void btnHard_Click(object sender, EventArgs e) => StartGame("Hard");
        private void btnExtreme_Click(object sender, EventArgs e) => StartGame("Extreme");






        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (!_isGameActive) return;

            int x = (e.X - _offsetX) / _cellSize;
            int y = (e.Y - _offsetY) / _cellSize;

            // 클릭한 위치가 그리드 범위 안인지 확인
            if (x >= 0 && x < _gridSize && y >= 0 && y < _gridSize)
            {
                var node = _manager.Grid[x, y];

                // 1. 시작점이나 끝점을 클릭했을 때
                if (node.Type == NodeType.Node)
                {
                    // [추가된 로직] 이미 연결된 선이 있다면 초기화
                    if (_completedPaths.ContainsKey(node.ColorID))
                    {
                        _completedPaths.Remove(node.ColorID);
                    }

                    // 2. 드래그 시작 준비
                    _isDragging = true;
                    _activeColor = node.ColorID;
                    _currentPath.Clear();
                    _currentPath.Add(new Point(x, y));

                    this.Invalidate(); // 화면 갱신
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (!_isDragging) return;

            Point gridPos = GetGridPosition(e.Location);

            if (IsValidPos(gridPos))
            {
                Point lastPos = _currentPath[_currentPath.Count - 1];
                if (gridPos == lastPos) return;

                // 1. [되돌리기] 이전 칸으로 돌아가면 마지막 칸 삭제
                if (_currentPath.Count > 1 && gridPos == _currentPath[_currentPath.Count - 2])
                {
                    _currentPath.RemoveAt(_currentPath.Count - 1);
                    this.Invalidate();
                    return;
                }

                // 2. [직각 이동 제한] Manhattan Distance가 1이어야 함
                int diffX = Math.Abs(lastPos.X - gridPos.X);
                int diffY = Math.Abs(lastPos.Y - gridPos.Y);

                if (diffX + diffY == 1)
                {
                    // 3. [충돌 방지] 자기 자신 혹은 이미 완성된 다른 선과 겹치는지 확인
                    if (IsPathBlocked(gridPos)) return;

                    // 4. [점 통과 제한] 다른 색 점은 밟을 수 없음
                    var targetNode = _manager.Grid[gridPos.X, gridPos.Y];
                    if (targetNode.Type == NodeType.Node && targetNode.ColorID != _activeColor) return;

                    _currentPath.Add(gridPos);

                    // 끝점에 도달하면 즉시 연결 시도
                    if (targetNode.Type == NodeType.Node && targetNode.ColorID == _activeColor)
                    {
                        CheckConnection();
                        _isDragging = false;
                    }

                    this.Invalidate();
                }
            }
        }

        // 다른 선에 의해 막혀있는지 확인하는 헬퍼 메서드
        private bool IsPathBlocked(Point pos)
        {
            // 현재 그리고 있는 선에 포함되었는가?
            if (_currentPath.Contains(pos)) return true;

            // 이미 완성된 다른 색상의 선들에 포함되었는가?
            foreach (var entry in _completedPaths)
            {
                if (entry.Key == _activeColor) continue; // 내 색상의 기존 선은 무시 (덮어쓰기 위해)
                if (entry.Value.Contains(pos)) return true;
            }
            return false;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            _isDragging = false;
            // 3. 마지막 지점이 같은 색상의 노드인지 확인하여 연결 확정
            CheckConnection();
        }

        // 1. 마우스 좌표를 그리드 좌표(0,0 ~ n,n)로 변환
        private Point GetGridPosition(Point mouseLocation)
        {
            int x = (mouseLocation.X - _offsetX) / _cellSize;
            int y = (mouseLocation.Y - _offsetY) / _cellSize;
            return new Point(x, y);
        }

        // 2. 좌표가 그리드 범위 안에 있는지 확인
        private bool IsValidPos(Point p)
        {
            return p.X >= 0 && p.X < _gridSize && p.Y >= 0 && p.Y < _gridSize;
        }

        // 3. 드래그 종료 시 연결 성공 여부 확인
        private void CheckConnection()
        {
            if (_currentPath.Count < 2) return;

            Point start = _currentPath[0];
            Point end = _currentPath[_currentPath.Count - 1];

            var startNode = _manager.Grid[start.X, start.Y];
            var endNode = _manager.Grid[end.X, end.Y];

            if (startNode.ColorID == endNode.ColorID && start != end)
            {
                _completedPaths[_activeColor] = new List<Point>(_currentPath);

                // 연결 성공할 때마다 승리 여부 확인
                CheckVictory();
            }

            _currentPath.Clear();
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_manager.Grid == null) return;

            base.OnPaint(e);
            Graphics g = e.Graphics;

            // 배경을 어두운 검은색으로 초기화 (해커 분위기)
            g.Clear(Color.Black);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // 1. 격자 그리기
            DrawGridLines(g);

            // 2. [HINT] 힌트 레이어 (가장 바닥)
            if (_showHint && _isGameActive && _hintColorID != -1)
            {
                if (_manager.SolutionPaths.ContainsKey(_hintColorID))
                {
                    var solPath = _manager.SolutionPaths[_hintColorID];
                    // 힌트는 아주 연하게 번지는 효과만 줌 (Glow 없이 단일 선)
                    Color hintColor = Color.FromArgb(40, GetSignalColor(_hintColorID, true));
                    DrawSignalPath(g, solPath, hintColor, 10);
                }
            }

            // 3. [COMPLETED] 연결 완료된 선들
            foreach (var pathEntry in _completedPaths)
            {
                // 익스트림 규칙: 마우스를 떼면 회색, 누르고 있는 색상만 정체 공개
                bool showRealColor = (_isDragging && _activeColor == pathEntry.Key);
                Color baseColor = GetSignalColor(pathEntry.Key, showRealColor);

                // 연결된 선에 글로우 효과 적용
                DrawGlowPath(g, pathEntry.Value, baseColor);
            }

            // 4. [ACTIVE] 현재 드래그 중인 선
            if (_isDragging && _currentPath.Count > 1)
            {
                Color activeColor = GetSignalColor(_activeColor, true); // 드래그 중엔 항상 진짜 색상
                DrawGlowPath(g, _currentPath, activeColor);
            }

            // 5. [NODES] 종착점 (가장 위 레이어)
            foreach (var node in _manager.Grid)
            {
                if (node.Type == NodeType.Node)
                {
                    // 마우스 누를 때만 색상 피킹
                    bool isPeeking = (_isDragging && _activeColor == node.ColorID);
                    Color nodeColor = GetSignalColor(node.ColorID, isPeeking);

                    DrawNode(g, node.Pos.X, node.Pos.Y, nodeColor);
                }
            }

            // 6. [STATUS] 하단 게임 상태 정보
            DrawGameStatus(g);

            // 7. [POST-PROCESS] CRT 스캔라인 효과 (모든 것 위에 덮음)
            DrawScanlines(g);
        }

        private void DrawGridLines(Graphics g)
        {
            // 아주 어두운 청록색으로 격자 생성
            using (Pen gridPen = new Pen(Color.FromArgb(30, 0, 255, 255), 1))
            {
                for (int i = 0; i <= _gridSize; i++)
                {
                    g.DrawLine(gridPen, _offsetX, _offsetY + i * _cellSize, _offsetX + _gridSize * _cellSize, _offsetY + i * _cellSize);
                    g.DrawLine(gridPen, _offsetX + i * _cellSize, _offsetY, _offsetX + i * _cellSize, _offsetY + _gridSize * _cellSize);
                }
            }
        }

        private void DrawGlowPath(Graphics g, List<Point> path, Color color)
        {
            // 1. 바깥쪽 번짐 (Glow)
            DrawSignalPath(g, path, Color.FromArgb(40, color), 22);
            // 2. 중간 번짐
            DrawSignalPath(g, path, Color.FromArgb(80, color), 18);
            // 3. 중심선 (핵심 신호선)
            DrawSignalPath(g, path, color, 14);
        }

        // 그리기 로직 공통화 (코드 중복 방지)
        private void DrawSignalPath(Graphics g, List<Point> path, Color color, int thickness)
        {
            if (path.Count < 2) return;

            // [해커 효과] 신호가 흐르는 듯한 미세한 플리커링 (힌트가 아닐 때만)
            int alpha = color.A;
            if (_isGameActive && alpha > 50)
            {
                Random rand = new Random();
                alpha = rand.Next(Math.Max(0, alpha - 40), Math.Min(255, alpha + 10));
            }
            Color flickerColor = Color.FromArgb(alpha, color.R, color.G, color.B);

            using (Pen p = new Pen(flickerColor, thickness))
            {
                p.StartCap = p.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                p.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;

                for (int i = 0; i < path.Count - 1; i++)
                {
                    Point p1 = new Point(_offsetX + path[i].X * _cellSize + _cellSize / 2, _offsetY + path[i].Y * _cellSize + _cellSize / 2);
                    Point p2 = new Point(_offsetX + path[i + 1].X * _cellSize + _cellSize / 2, _offsetY + path[i + 1].Y * _cellSize + _cellSize / 2);
                    g.DrawLine(p, p1, p2);
                }
            }
        }

        private void DrawNode(Graphics g, int x, int y, Color color)
        {
            int margin = _cellSize / 4;
            int size = _cellSize / 2;
            Rectangle rect = new Rectangle(_offsetX + x * _cellSize + margin, _offsetY + y * _cellSize + margin, size, size);

            using (Brush b = new SolidBrush(color))
            {
                g.FillEllipse(b, rect);
            }

            // 노드 테두리에 밝은 광택 효과 추가
            using (Pen p = new Pen(Color.FromArgb(150, Color.White), 2))
            {
                g.DrawEllipse(p, rect);
            }
        }

        private void DrawScanlines(Graphics g)
        {
            // 화면 전체에 가로 스캔라인 덧씌우기
            using (Pen scanlinePen = new Pen(Color.FromArgb(15, 0, 0, 0), 1))
            {
                for (int y = _offsetY; y < _offsetY + (_gridSize * _cellSize); y += 3)
                {
                    g.DrawLine(scanlinePen, _offsetX, y, _offsetX + (_gridSize * _cellSize), y);
                }
            }
        }

        private void DrawGameStatus(Graphics g)
        {
            int filled = 0;
            foreach (var p in _completedPaths.Values) filled += p.Count;
            int remaining = (_gridSize * _gridSize) - filled;

            string status = remaining == 0 ? ">>> CORE LINKED <<<" : $"SCANNING... EMPTY NODES: {remaining}";
            if (!_isGameActive && remaining == 0) status = "ACCESS GRANTED [SUCCESS]";

            using (Font f = new Font("Consolas", 11, FontStyle.Bold))
            {
                g.DrawString(status, f, Brushes.Lime, _offsetX, _offsetY + (_gridSize * _cellSize) + 15);
            }
        }

        // ID별 색상 반환 유틸리티
        private Color GetSignalColor(int id, bool isPeeking = false)
        {
            // 익스트림 모드용 회색 쌍 (7번, 8번)
            if ((id == 7 || id == 8) && !isPeeking)
            {
                return Color.FromArgb(120, 120, 120); // 평상시엔 어두운 회색
            }

            switch (id)
            {
                case 1: return Color.FromArgb(255, 80, 80);   // Red
                case 2: return Color.FromArgb(80, 150, 255);  // Blue
                case 3: return Color.FromArgb(80, 255, 150);  // Green
                case 4: return Color.FromArgb(255, 255, 80);  // Yellow
                case 5: return Color.FromArgb(255, 150, 50);  // Orange
                case 6: return Color.FromArgb(180, 100, 255); // Purple
                case 7: return Color.FromArgb(255, 100, 200); // Pink (Extreme)
                case 8: return Color.FromArgb(100, 255, 255); // Cyan (Extreme)
                default: return Color.Gray;
            }
        }



        private void CheckVictory()
        {
            // 1. 모든 신호 쌍 연결 확인
            if (_completedPaths.Count != _manager.PairCount) return;

            // 2. 모든 칸 점유 확인 (Fill All Cells)
            int filledCount = 0;
            foreach (var path in _completedPaths.Values) filledCount += path.Count;

            // 현재 드래그 중인 선은 포함되지 않으므로, 모든 선이 '완료' 상태여야 함
            if (filledCount == _gridSize * _gridSize)
            {
                _isGameActive = false;
                ResetHintState(); // 클리어 시 힌트 끄기
                _gameTimer.Stop(); // 클리어 시 타이머 정지
                this.Refresh();
                System.Threading.Thread.Sleep(200);

                string timeStr = string.Format("{0:D2}:{1:D2}", _playTimeSeconds / 60, _playTimeSeconds % 60);
                ShowVictoryOverlay(timeStr, _currentDifficulty);
            }
        }

        private void ShowVictoryOverlay(string time, string difficulty)
        {
            // 1. Dim 레이어 생성
            Panel dimLayer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(180, 0, 0, 0),
                Location = new Point(0, 0),
                Name = "DimLayer"
            };

            // 2. 결과 카드 생성
            var resultCard = new MaterialSkin.Controls.MaterialCard
            {
                Size = new Size(380, 400),
                Padding = new Padding(24),
                BackColor = Color.FromArgb(40, 40, 40)
            };

            var lblTitle = new MaterialSkin.Controls.MaterialLabel
            {
                Text = "SIGNAL ESTABLISHED",
                FontType = MaterialSkin.MaterialSkinManager.fontType.H4,
                HighEmphasis = true,
                UseAccent = true,
                Location = new Point(24, 24),
                AutoSize = true
            };

            var lblStats = new MaterialSkin.Controls.MaterialLabel
            {
                Text = $"TIME: {time}\nLEVEL: {difficulty.ToUpper()}\nSTATUS: ALL SIGNALS SYNCED",
                FontType = MaterialSkin.MaterialSkinManager.fontType.Subtitle1,
                Location = new Point(24, 85),
                AutoSize = true
            };

            // 3. 확인 버튼 (클릭 시 리셋)
            var btnClose = new MaterialSkin.Controls.MaterialButton
            {
                Text = "TERMINATE CONNECTION",
                Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained,
                UseAccentColor = true,
                Size = new Size(330, 40),
                Location = new Point(24, 320)
            };

            btnClose.Click += (s, e) => {
                this.Controls.Remove(dimLayer);
                dimLayer.Dispose();
                btnReset_Click(null, null); // 초기 화면으로 돌아가기
            };

            resultCard.Controls.Add(lblTitle);
            resultCard.Controls.Add(lblStats);
            resultCard.Controls.Add(btnClose);

            resultCard.Location = new Point((this.Width - resultCard.Width) / 2, (this.Height - resultCard.Height) / 2);
            dimLayer.Controls.Add(resultCard);
            this.Controls.Add(dimLayer);
            dimLayer.BringToFront();
        }


        private void btnReset_Click(object sender, EventArgs e)
        {
            // 1. 현재 진행 중인 데이터들 초기화
            _currentPath.Clear();
            _completedPaths.Clear();
            ResetHintState(); // 리셋 시 힌트 끄기

            // 2. 난이도 선택 패널 다시 표시
            pnlDifficulty.Visible = true;
            pnlDifficulty.BringToFront();

            // 3. 뒤로가기/리셋 버튼은 항상 패널보다 위에 있어야 하므로 다시 정렬 (필요시)
            btnBack.BringToFront();
            btnReset.BringToFront();

            // 4. 배경 화면 갱신
            this.Invalidate();
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            if (this.ParentForm is SandBox sb)
            {
                sb.BackToList();
            }
        }

        private async void btnHint_Click(object sender, EventArgs e)
        {
            if (!_isGameActive) return;

            // 1. 이미 진행 중인 힌트 타이머가 있다면 즉시 취소
            _hintCts?.Cancel();
            _hintCts = new System.Threading.CancellationTokenSource();
            var token = _hintCts.Token;

            // 2. 새로운 랜덤 색상 선택 (현재 표시 중인 색상과 겹치지 않게)
            var availableColors = _manager.SolutionPaths.Keys.ToList();
            if (availableColors.Count > 0)
            {
                Random rand = new Random();
                int nextColorID;

                // 색상이 2개 이상이면 현재 보여주는 색상과 다른 색상이 나올 때까지 무작위 선택
                if (availableColors.Count > 1)
                {
                    do
                    {
                        nextColorID = availableColors[rand.Next(availableColors.Count)];
                    } while (nextColorID == _hintColorID);
                }
                else
                {
                    nextColorID = availableColors[0];
                }

                _hintColorID = nextColorID;
                _showHint = true;
                this.Invalidate(); // 새 힌트 표시

                try
                {
                    await Task.Delay(3000, token); // 3. 3초(3000ms) 대기 (취소 토큰 전달)
                    ResetHintState(); // 4. 3초가 무사히 지나면 힌트 자동 종료
                    this.Invalidate();
                }
                catch (TaskCanceledException)
                {
                    // 3초가 지나기 전에 버튼을 다시 누르면 이 위치로 오게 됨
                    // 새로운 클릭이 이미 _hintColorID를 갱신했으므로 여기서는 아무것도 하지 않음
                }
            }
        }

        private void ResetHintState()
        {
            _showHint = false;
            _hintColorID = -1;
            _hintCts?.Cancel();
            // 버튼 스타일 등을 초기화하는 코드가 있다면 여기에 추가
        }
    }
}