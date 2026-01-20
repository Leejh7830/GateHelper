using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

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

        // 기믹용 (잠금)
        public List<Point> lockedPoints = new List<Point>();

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

            if (!DesignMode)
            {
                if (cmbDifficulty.Items.Count > 0) cmbDifficulty.SelectedIndex = 1;
                StartNewGame("Normal");
            }
        }

        // --- 게임 흐름 제어 ---

        private async void StartNewGame(string levelName)
        {
            // 이미 처리 중이면 아무것도 하지 않음
            if (_isProcessing) return;

            _isProcessing = true;
            _isGameActive = false; // 셔플 중에는 게임 활성 상태가 아님

            gameTimer.Stop(); // 이전 타이머가 돌고 있다면 정지

            var config = GetConfig(levelName);
            CreateGrid(config.GridSize);

            // 상태 초기화 로직 (잠금 리스트 비우기 등)
            lockedPoints.Clear();
            UpdateUI();

            if (levelName == "Extreme")
            {
                GimmickHandler.ResetAllGimmickStates();
                await GimmickHandler.ApplyRandomGimmicksAsync();
            }

            // 3. 셔플 애니메이션 (이동안 _isProcessing은 계속 true임)
            await ShuffleAnimationAsync(config.ShuffleCount);

            if (levelName == "Extreme") GimmickHandler.ManageProgression(0);

            playTimeSeconds = 0;
            lblTimer.Text = "00:00";

            _isGameActive = true;  // 이제 플레이 가능
            _isProcessing = false; // 작업 완료
            gameTimer.Start();
        }

        private void CheckVictory()
        {
            // 이미 승리 처리가 시작되었다면 중복 실행 방지
            if (!_isGameActive || _isProcessing) return;

            bool isVictory = true;
            for (int r = 0; r < currentGridSize; r++)
            {
                for (int c = 0; c < currentGridSize; c++)
                {
                    if (gridStates[c, r]) { isVictory = false; break; }
                }
            }

            if (isVictory)
            {
                _isGameActive = false; // 게임 종료
                _isProcessing = true;  // 승리 메시지 처리 시작
                gameTimer.Stop();

                MessageBox.Show($"Clear! Time: {lblTimer.Text}");

                _isProcessing = false; // 메시지 닫은 후 상태 해제
            }
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
            for (int r = 0; r < currentGridSize; r++)
            {
                for (int c = 0; c < currentGridSize; c++)
                {
                    var btn = gridButtons[c, r];

                    // 1. [중요] 기믹이 입힌 네온 색상을 지우고 투명하게 리셋합니다.
                    // 이 코드가 있어야 데이터 노이즈 효과가 끝난 후 다시 원래대로 돌아옵니다.
                    btn.BackColor = Color.Transparent;

                    bool isLocked = lockedPoints.Contains(new Point(c, r));

                    if (isLocked)
                    {
                        btn.Enabled = false;
                        btn.Text = "LOCK";
                        btn.Type = MaterialButton.MaterialButtonType.Contained;
                    }
                    else
                    {
                        btn.Enabled = true;
                        btn.Text = "";
                        bool state = gridStates[c, r];

                        // 2. 버튼의 데이터 상태에 따라 스타일을 결정합니다.
                        btn.Type = state ? MaterialButton.MaterialButtonType.Contained : MaterialButton.MaterialButtonType.Outlined;
                        btn.HighEmphasis = state;
                        btn.UseAccentColor = state; // 여기서 다시 붉은색 테마 색상이 적용됩니다.
                    }

                    // 변경사항을 즉시 화면에 반영합니다.
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
            if (!tableLayoutPanel1.Enabled) return;

            playTimeSeconds++;
            TimeSpan t = TimeSpan.FromSeconds(playTimeSeconds);
            string timeStr = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
            lblTimer.Text = timeStr;

            if (cmbDifficulty.Text == "Extreme")
            {
                // [추가] 매니저에게 시간을 전달하여 기믹 단계 관리
                GimmickHandler.ManageProgression(playTimeSeconds);

                await GimmickHandler.UpdateTickAsync();

                // 경고 연출 (기존 동일)
                bool isAnyGimmickNear = GimmickHandler.ActiveGimmicks.Any(g => g.ElapsedSeconds == g.Interval - 1);
                lblTimer.ForeColor = isAnyGimmickNear ? Color.Red : Color.White;

                CheckVictory();
            }
            else
            {
                lblTimer.ForeColor = Color.White;
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