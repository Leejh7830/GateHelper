using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
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
 

        private Timer _gameTimer;
        private int _playTimeSeconds;
        private string _currentDifficulty;
        private bool _isGameActive = false;

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
            { "Hard", new DifficultyConfig(8, 6) }
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

            // 4. [Z-Order 정리] 누가 누구 위에 보일지 명확히 정하기
            pnlDifficulty.BringToFront(); // 일단 패널을 가장 앞으로

            // 패널 "내부"에서 버튼들을 앞으로 가져오기
            btnEasy.BringToFront();
            btnNormal.BringToFront();
            btnHard.BringToFront();

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

            pnlDifficulty.Visible = false;
            this.Invalidate();
        }


        private void btnEasy_Click(object sender, EventArgs e) => StartGame("Easy");
        private void btnNormal_Click(object sender, EventArgs e) => StartGame("Normal");
        private void btnHard_Click(object sender, EventArgs e) => StartGame("Hard");






        protected override void OnMouseDown(MouseEventArgs e)
        {
            // 1. 클릭한 위치가 노드(시작점)인지 확인
            Point gridPos = GetGridPosition(e.Location);
            if (IsValidPos(gridPos) && _manager.Grid[gridPos.X, gridPos.Y].Type == NodeType.Node)
            {
                _isDragging = true;
                _activeColor = _manager.Grid[gridPos.X, gridPos.Y].ColorID;
                _currentPath.Clear();
                _currentPath.Add(gridPos);
                this.Invalidate(); // 화면 다시 그리기
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
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // 1. 그리드 격자 그리기 [코드 복구]
            using (Pen gridPen = new Pen(Color.FromArgb(60, 60, 60), 1))
            {
                for (int i = 0; i <= _gridSize; i++)
                {
                    g.DrawLine(gridPen, _offsetX, _offsetY + i * _cellSize, _offsetX + _gridSize * _cellSize, _offsetY + i * _cellSize);
                    g.DrawLine(gridPen, _offsetX + i * _cellSize, _offsetY, _offsetX + i * _cellSize, _offsetY + _gridSize * _cellSize);
                }
            }

            // 2. 배치된 노드(점) 그리기 [코드 복구]
            foreach (var node in _manager.Grid)
            {
                if (node.Type == NodeType.Node)
                {
                    Color nodeColor = GetSignalColor(node.ColorID);
                    using (Brush b = new SolidBrush(nodeColor))
                    {
                        int margin = _cellSize / 4;
                        int size = _cellSize / 2;
                        g.FillEllipse(b, _offsetX + node.Pos.X * _cellSize + margin, _offsetY + node.Pos.Y * _cellSize + margin, size, size);
                    }
                }
            }

            // 3. 이미 완성된 선들 모두 그리기
            foreach (var pathEntry in _completedPaths)
            {
                DrawSignalPath(g, pathEntry.Value, GetSignalColor(pathEntry.Key));
            }

            // 4. 현재 드래그 중인 선 그리기
            if (_isDragging && _currentPath.Count > 1)
            {
                DrawSignalPath(g, _currentPath, GetSignalColor(_activeColor));
            }
        }

        // 그리기 로직 공통화 (코드 중복 방지)
        private void DrawSignalPath(Graphics g, List<Point> path, Color color)
        {
            using (Pen pathPen = new Pen(color, 12)) // 선 굵기
            {
                pathPen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                pathPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                pathPen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;

                for (int i = 0; i < path.Count - 1; i++)
                {
                    Point p1 = new Point(_offsetX + path[i].X * _cellSize + _cellSize / 2, _offsetY + path[i].Y * _cellSize + _cellSize / 2);
                    Point p2 = new Point(_offsetX + path[i + 1].X * _cellSize + _cellSize / 2, _offsetY + path[i + 1].Y * _cellSize + _cellSize / 2);
                    g.DrawLine(pathPen, p1, p2);
                }
            }
        }

        // ID별 색상 반환 유틸리티
        private Color GetSignalColor(int id)
        {
            switch (id)
            {
                case 1: return Color.FromArgb(255, 80, 80);  // Red
                case 2: return Color.FromArgb(80, 150, 255); // Blue
                case 3: return Color.FromArgb(80, 255, 150); // Green
                case 4: return Color.FromArgb(255, 255, 80); // Yellow
                default: return Color.Gray;
            }
        }



        private void CheckVictory()
        {
            if (_completedPaths.Count == _manager.PairCount)
            {
                _isGameActive = false;
                _gameTimer.Stop();

                // 1. 선이 끝까지 연결된 것을 보여주기 위해 강제 새로고침
                this.Refresh();
                System.Threading.Thread.Sleep(200); // 인지할 시간 부여

                // 2. Bit Flip 스타일의 승리 화면 호출
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

            // (중요) 나중에 추가할 '완료된 선들의 리스트'도 여기서 Clear 해줘야 합니다.
            // 예: _completedPaths.Clear(); 

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
    }
}