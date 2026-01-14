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
        internal Point lockedPoint = new Point(-1, -1);

        // 로직 관리자
        internal GimmickManager GimmickHandler;

        private Timer gameTimer;
        private int playTimeSeconds;
        private int extremeTickCount;

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
            var config = GetConfig(levelName);
            CreateGrid(config.GridSize);
            gameTimer.Stop();

            // 1. 공통 상태 초기화
            lockedPoint = new Point(-1, -1);
            UpdateUI();

            // 2. 익스트림 모드 설정
            if (levelName == "Extreme")
            {
                // 새로운 기믹 3개를 랜덤으로 뽑습니다. (내부에서 새 객체가 생성되므로 시간은 0임)
                await GimmickHandler.ApplyRandomGimmicksAsync();
            }
            else
            {
                // 익스트림이 아니면 활성화된 기믹 목록을 비워줍니다.
                GimmickHandler.ActiveGimmicks.Clear();
            }

            // 3. 셔플 애니메이션
            await ShuffleAnimationAsync(config.ShuffleCount);

            // 4. 게임 시작 데이터 설정
            playTimeSeconds = 0;
            // extremeTickCount = 0; // <-- [삭제] 이제 필요 없습니다!

            lblTimer.Text = "00:00";
            lblTimer.ForeColor = Color.White;
            gameTimer.Start();
        }

        private async void CheckVictory()
        {
            bool isAllClear = gridStates.Cast<bool>().All(state => state);

            if (isAllClear)
            {
                gameTimer.Stop();
                TimeSpan t = TimeSpan.FromSeconds(playTimeSeconds);
                string finalTime = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
                lblTimer.Text = finalTime;

                if (cmbDifficulty.Text == "Extreme") await ShakeScreen(800, 10);

                MessageBox.Show($"Clear Time: {finalTime}\nLogic Synchronized!", "System Victory");
                StartNewGame(cmbDifficulty.Text);
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
                    bool isLocked = (c == lockedPoint.X && r == lockedPoint.Y);

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
                        btn.Type = state ? MaterialButton.MaterialButtonType.Contained : MaterialButton.MaterialButtonType.Outlined;
                        btn.HighEmphasis = state;
                        btn.UseAccentColor = state;
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
            if (!tableLayoutPanel1.Enabled) return;

            playTimeSeconds++;
            TimeSpan t = TimeSpan.FromSeconds(playTimeSeconds);
            string timeStr = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
            lblTimer.Text = timeStr;

            if (cmbDifficulty.Text == "Extreme")
            {
                // 매초마다 각 기믹의 내부 카운터를 업데이트하고 필요시 실행
                await GimmickHandler.UpdateTickAsync();

                // [경고 연출] 아무 기믹이나 곧 터질 것 같을 때(주기 1초 전) 빨간색 표시
                bool isAnyGimmickNear = GimmickHandler.ActiveGimmicks.Any(g => g.ElapsedSeconds == g.Interval - 1);
                lblTimer.ForeColor = isAnyGimmickNear ? Color.Red : Color.White;

                CheckVictory();
            }
            else
            {
                lblTimer.ForeColor = Color.White;
            }
        }

        private async void OnGridClick(object sender, EventArgs e)
        {
            if (!(sender is MaterialButton btn)) return;
            Point pos = (Point)btn.Tag;

            if (pos == lockedPoint) { await ShakeScreen(100, 2); return; }

            GimmickHandler.FlipSwitch(pos.X, pos.Y);
            UpdateUI();
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