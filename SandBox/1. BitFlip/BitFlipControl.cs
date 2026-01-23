using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace GateHelper
{
    public partial class BitFlipControl : UserControl
    {
        // 핵심 데이터 (GimmickManager가 접근할 수 있도록 internal 설정)
        internal Random rand = new Random();
        internal MaterialButton[,] gridButtons;
        internal bool[,] gridStates;
        internal int currentGridSize;

        private bool _isProcessing = false; // 셔플, 초기화, 승리 처리 등 '작업 중'임을 나타냄
        private bool _isGameActive = false; // 실제 플레이 가능한 상태인지 여부

        // 기믹용
        public List<Point> lockedPoints = new List<Point>();
        public Dictionary<Point, int> ActiveBombs = new Dictionary<Point, int>();

        // 로직 관리자
        internal GimmickManager GimmickHandler;

        private Timer gameTimer;
        private int playTimeSeconds;

        public struct DifficultyConfig
        {
            public int GridSize;
            public int ShuffleCount;
        }

        public BitFlipControl()
        {
            InitializeComponent();
            GimmickHandler = new GimmickManager(this); // 매니저 초기화

            gameTimer = new Timer();
            gameTimer.Interval = 1000;
            gameTimer.Tick += GameTimer_Tick;

            lblSolvability.Font = new Font("Consolas", 14f, FontStyle.Bold);
            lblSolvability.BackColor = Color.Transparent;


            if (!DesignMode)
            {
                if (cmbDifficulty.Items.Count > 0) cmbDifficulty.SelectedIndex = 1;
                StartNewGame("Normal");
            }
        }

        // --- 게임 흐름 제어 ---

        private async void StartNewGame(string levelName)
        {
            if (_isProcessing) return;
            _isProcessing = true;
            _isGameActive = false;

            gameTimer.Stop();
            var config = GetConfig(levelName);
            CreateGrid(config.GridSize);
            lockedPoints.Clear();
            UpdateUI();

            GimmickHandler.ResetAllGimmickStates();

            // 1. 기믹 데이터만 먼저 설정 (알림 안 뜸)
            if (levelName == "Hard") GimmickHandler.SetStaticGimmicks((1, 2));
            else if (levelName == "Insane") GimmickHandler.SetStaticGimmicks((1, 2), (1, 3));
            else if (levelName == "Extreme") await GimmickHandler.ApplyRandomGimmicksAsync();

            // 2. 셔플 애니메이션 실행 (lblTimer에 % 표시)
            await ShuffleAnimationAsync(config.ShuffleCount);

            // 3. [핵심] 셔플이 완전히 끝난 후 기믹 알림 표시
            if (levelName == "Hard" || levelName == "Insane" || levelName == "Extreme")
            {
                GimmickHandler.ManageProgression(0, levelName);
            }

            playTimeSeconds = 0;
            lblTimer.Text = "00:00";
            _isGameActive = true;
            _isProcessing = false;
            gameTimer.Start();
        }

        // [BitFlipControl.cs - CheckVictory 메서드 내부]

        private void CheckVictory()
        {
            if (!_isGameActive || _isProcessing) return;

            bool isVictory = true;
            for (int r = 0; r < currentGridSize; r++)
            {
                for (int c = 0; c < currentGridSize; c++)
                {
                    if (!gridStates[c, r]) { isVictory = false; break; }
                }
            }

            if (isVictory)
            {
                _isGameActive = false;
                _isProcessing = true;
                gameTimer.Stop();

                // [수정] MaterialSkin 스타일의 승리 화면 호출
                ShowVictoryOverlay(lblTimer.Text, cmbDifficulty.Text, GimmickHandler.UsedGimmicks);

                _isProcessing = false;
            }
        }

        /// <summary>
        /// 게임 클리어 시 Dim 효과와 함께 Material 디자인 결과창을 표시합니다.
        /// </summary>
        private void ShowVictoryOverlay(string time, string difficulty, HashSet<string> gimmicks)
        {
            // 1. Dim 레이어 생성 (배경을 어둡게 만듭니다)
            Panel dimLayer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(180, 0, 0, 0), // 180: 투명도 (0~255)
                Location = new Point(0, 0),
                Name = "DimLayer"
            };

            // 2. 결과 카드 생성 (중앙에 배치될 흰색/회색 박스)
            var resultCard = new MaterialSkin.Controls.MaterialCard
            {
                Size = new Size(380, 500),
                Padding = new Padding(24),
                Depth = 0,
                BackColor = Color.FromArgb(40, 40, 40) // 다크 테마 배경색
            };

            // 3. 제목 라벨 (lblTitle: 승리 문구)
            var lblTitle = new MaterialSkin.Controls.MaterialLabel
            {
                Text = "SYSTEM CRACKED",
                FontType = MaterialSkin.MaterialSkinManager.fontType.H4,
                HighEmphasis = true,
                UseAccent = true, // 강조색(Accent) 적용
                Location = new Point(24, 24),
                AutoSize = true
            };

            // 4. 기록 정보 라벨 (lblStats: 시간 및 난이도)
            var lblStats = new MaterialSkin.Controls.MaterialLabel
            {
                Text = $"TIME: {time}\nLEVEL: {difficulty.ToUpper()}",
                FontType = MaterialSkin.MaterialSkinManager.fontType.Subtitle1,
                Location = new Point(24, 85),
                AutoSize = true
            };

            // 5. 기믹 목록 제목
            var lblGimmickTitle = new MaterialSkin.Controls.MaterialLabel
            {
                Text = "OVERCOMED SECURITY",
                FontType = MaterialSkin.MaterialSkinManager.fontType.Caption,
                Location = new Point(24, 155),
                AutoSize = true
            };

            // 6. 기믹 목록 텍스트 상자 (txtGimmicks)
            // 해커 텍스트 느낌을 주기 위해 Consolas 폰트와 어두운 배경을 사용합니다.
            var txtGimmicks = new TextBox
            {
                Text = gimmicks.Count > 0 ? "• " + string.Join("\r\n• ", gimmicks.Select(g => g.ToUpper())) : "NO GIMMICKS DETECTED",
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.LimeGreen, // 해커 느낌의 초록색 글씨
                Font = new Font("Consolas", 10f, FontStyle.Bold),
                Location = new Point(24, 185),
                Size = new Size(330, 220)
            };

            // 7. 확인 버튼 (btnClose: 클릭 시 닫고 새 게임)
            var btnClose = new MaterialSkin.Controls.MaterialButton
            {
                Text = "COMPLETED",
                Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained,
                UseAccentColor = true,
                Size = new Size(330, 40),
                Location = new Point(24, 430)
            };

            // 버튼 클릭 이벤트: Dim 레이어 전체를 제거하고 새 게임 시작
            btnClose.Click += (s, e) =>
            {
                this.Controls.Remove(dimLayer);
                dimLayer.Dispose();
                StartNewGame(difficulty);
            };

            // 8. 컨트롤 조립 (Hierarchy 구성)
            resultCard.Controls.Add(lblTitle);
            resultCard.Controls.Add(lblStats);
            resultCard.Controls.Add(lblGimmickTitle);
            resultCard.Controls.Add(txtGimmicks);
            resultCard.Controls.Add(btnClose);

            // 카드를 Dim 레이어 정중앙에 배치
            resultCard.Location = new Point(
                (this.Width - resultCard.Width) / 2,
                (this.Height - resultCard.Height) / 2
            );

            dimLayer.Controls.Add(resultCard);

            // 최종적으로 폼에 추가
            this.Controls.Add(dimLayer);
            dimLayer.BringToFront(); // 최상단으로 가져오기
        }

        // --- UI 생성 및 업데이트 ---

        private void CreateGrid(int size)
        {
            currentGridSize = size;
            gridButtons = new MaterialButton[size, size];
            gridStates = new bool[size, size];

            tableLayoutPanel1.Controls.Clear();
            tableLayoutPanel1.ColumnCount = size;
            tableLayoutPanel1.RowCount = size;
            tableLayoutPanel1.ColumnStyles.Clear();
            tableLayoutPanel1.RowStyles.Clear();

            float percent = 100f / size;
            for (int i = 0; i < size; i++)
            {
                tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, percent));
                tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, percent));
            }

            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    MaterialButton btn = new MaterialButton
                    {
                        AutoSize = false,
                        Size = new Size(400 / size, 400 / size),
                        Tag = new Point(c, r),
                        TabStop = false,
                        Text = ""
                    };
                    btn.Click += OnGridClick;
                    gridButtons[c, r] = btn;
                    tableLayoutPanel1.Controls.Add(btn, c, r);
                }
            }
        }

        internal void UpdateUI()
        {
            // 1. 가장 먼저 컨트롤의 파기 상태를 확인하여 에러 방지
            if (this.IsDisposed || this.Disposing) return;

            // 2. UI 스레드가 아닐 경우 Invoke를 통해 재호출 (중복 제거)
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(UpdateUI));
                return;
            }
            ////////////////////////////////////////////////////////////////
            // 1. 해결 가능성 체크 및 라벨 갱신 (반복문 밖에서 한 번만 수행)
            bool solvable = GimmickHandler.IsSolvable(gridStates, currentGridSize);

            lblSolvability.Text = solvable ? "가능" : "불가능";
            lblSolvability.ForeColor = solvable ? Color.LimeGreen : Color.OrangeRed;

            // 라벨을 다른 컨트롤보다 위로 올리고 강제 새로고침
            lblSolvability.BringToFront();
            lblSolvability.Refresh();

            Point mousePos = PointToClient(Cursor.Position);

            for (int r = 0; r < currentGridSize; r++)
            {
                for (int c = 0; c < currentGridSize; c++)
                {
                    var btn = gridButtons[c, r];
                    Point pos = new Point(c, r);
                    bool state = gridStates[c, r];

                    btn.BackColor = Color.Transparent;
                    btn.ForeColor = Color.White;

                    // 2. 안개 기믹 처리
                    bool isInFog = false;
                    if (GimmickHandler.IsFogActive)
                    {
                        Point btnCenter = new Point(btn.Left + btn.Width / 2, btn.Top + btn.Height / 2);
                        double distance = Math.Sqrt(Math.Pow(btnCenter.X - mousePos.X, 2) + Math.Pow(btnCenter.Y - mousePos.Y, 2));
                        if (distance > 140) isInFog = true;
                    }

                    if (isInFog)
                    {
                        btn.Text = "";
                        btn.BackColor = Color.FromArgb(20, 20, 20);
                        btn.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
                        btn.HighEmphasis = false;
                        btn.UseAccentColor = false;
                        btn.Invalidate();
                        continue;
                    }

                    // 3. 잠금, 폭탄, 일반 상태 처리 (기존 로직 유지)
                    if (lockedPoints.Contains(pos))
                    {
                        btn.Enabled = false;
                        btn.Text = "LOCK";
                        btn.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
                    }
                    else
                    {
                        btn.Enabled = true;
                        if (ActiveBombs.ContainsKey(pos))
                        {
                            int remaining = ActiveBombs[pos];
                            btn.Text = "💣" + remaining;
                            btn.ForeColor = (remaining <= 2) ? Color.Red : Color.Yellow;
                        }
                        else { btn.Text = ""; }

                        if (state)
                        {
                            btn.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
                            btn.HighEmphasis = true;
                            btn.UseAccentColor = true;
                        }
                        else
                        {
                            btn.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
                            btn.HighEmphasis = false;
                            btn.UseAccentColor = false;
                        }
                    }

                    btn.Invalidate();
                }
            }
        }

        // --- 애니메이션 및 효과 ---

        internal async Task ShakeScreen(int durationMs = 300, int amplitude = 6)
        {
            var originalDock = tableLayoutPanel1.Dock;
            Point originalPos = tableLayoutPanel1.Location;
            tableLayoutPanel1.Dock = DockStyle.None;

            DateTime endTime = DateTime.Now.AddMilliseconds(durationMs);
            while (DateTime.Now < endTime)
            {
                tableLayoutPanel1.Location = new Point(
                    originalPos.X + rand.Next(-amplitude, amplitude + 1),
                    originalPos.Y + rand.Next(-amplitude, amplitude + 1)
                );
                await Task.Delay(20);
            }

            tableLayoutPanel1.Location = originalPos;
            tableLayoutPanel1.Dock = originalDock;
        }

        private async Task ShuffleAnimationAsync(int count)
        {
            tableLayoutPanel1.Enabled = false;
            for (int i = 0; i < count; i++)
            {
                GimmickHandler.FlipSwitch(rand.Next(currentGridSize), rand.Next(currentGridSize));
                UpdateUI();
                await Task.Delay(20);
                if (i % 5 == 0) lblTimer.Text = $"SHUFFLING {(int)((double)i / count * 100)}%";
            }
            tableLayoutPanel1.Enabled = true;
        }

        // --- 이벤트 핸들러 ---

        // [BitFlipControl.cs]

        private async void GameTimer_Tick(object sender, EventArgs e)
        {
            // [추가] 컨트롤이 없으면 타이머 중단 및 반환
            if (this.IsDisposed || this.Disposing)
            {
                gameTimer.Stop();
                return;
            }

            if (!tableLayoutPanel1.Enabled) return;

            playTimeSeconds++;
            lblTimer.Text = string.Format("{0:D2}:{1:D2}", playTimeSeconds / 60, playTimeSeconds % 60);

            string diff = cmbDifficulty.Text;
            bool hasGimmicks = (diff == "Hard" || diff == "Insane" || diff == "Extreme");

            if (hasGimmicks)
            {
                // 20초마다 잠금 해제 등의 공통 관리 로직 실행
                GimmickHandler.ManageProgression(playTimeSeconds, diff);

                // 현재 활성화된 기믹(상시 또는 동적) 실행
                await GimmickHandler.UpdateTickAsync();

                // 경고 연출 (기존 동일)
                bool isAnyGimmickNear = GimmickHandler.ActiveGimmicks.Any(g => g.ElapsedSeconds == g.Interval - 1);
                lblTimer.ForeColor = isAnyGimmickNear ? Color.Red : Color.White;
            }
        }

        
        // 버튼 클릭 이벤트
        private async void OnGridClick(object sender, EventArgs e)
        {
            // 게임이 시작되지 않았거나, 셔플/승리 처리 중이면 클릭 무시
            if (!_isGameActive || _isProcessing) return;

            if (!(sender is MaterialButton btn)) return;
            Point pos = (Point)btn.Tag;

            if (lockedPoints.Contains(pos))
            {
                await ShakeScreen(100, 2);
                return;
            }

            GimmickHandler.FlipSwitch(pos.X, pos.Y);
            UpdateUI();

            // 승리 체크
            CheckVictory();
        }

        private DifficultyConfig GetConfig(string level)
        {
            switch (level)
            {
                case "Easy": return new DifficultyConfig { GridSize = 3, ShuffleCount = 5 };
                case "Normal": return new DifficultyConfig { GridSize = 5, ShuffleCount = 15 };
                case "Hard": return new DifficultyConfig { GridSize = 7, ShuffleCount = 30 };
                case "Insane": return new DifficultyConfig { GridSize = 9, ShuffleCount = 40 }; // 새 난이도
                case "Extreme": return new DifficultyConfig { GridSize = 9, ShuffleCount = 50 };
                default: return new DifficultyConfig { GridSize = 5, ShuffleCount = 15 };
            }
        }

        // 기믹 추가 알림을 표시하는 메서드
        internal async Task ShowGimmickNotify(string msg)
        {
            if (lblGimmickNotify == null) return;

            // 1. 텍스트 및 색상 설정
            lblGimmickNotify.Text = msg;
            lblGimmickNotify.ForeColor = Color.Red;

            // 2. 굵게(Bold) 처리 및 크기 조절 (기존 폰트 유지하면서 스타일만 변경)
            // 만약 크기를 키우고 싶다면 12f 대신 원하는 숫자를 넣으세요.
            lblGimmickNotify.Font = new Font(lblGimmickNotify.Font.FontFamily, 12f, FontStyle.Bold);

            // 3. 표시 및 최상단 배치
            lblGimmickNotify.Visible = true;
            lblGimmickNotify.BringToFront();

            // 4. 대기 후 사라짐
            await Task.Delay(2000);

            lblGimmickNotify.Visible = false;
            lblGimmickNotify.Text = "";
        }

        private void cmbDifficulty_SelectedIndexChanged(object sender, EventArgs e) => StartNewGame(cmbDifficulty.Text);
        private void btnReset_Click(object sender, EventArgs e) => StartNewGame(cmbDifficulty.Text);

        private void btnBack_Click(object sender, EventArgs e)
        {
            if (this.ParentForm is SandBox sb)
            {
                sb.BackToList();
            }
        }
    }
}