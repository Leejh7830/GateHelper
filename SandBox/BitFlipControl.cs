using MaterialSkin.Controls;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GateHelper
{
    public partial class BitFlipControl : UserControl
    {
        // 1. 필드 선언
        private Random rand = new Random();
        private MaterialButton[,] gridButtons;
        private bool[,] gridStates;
        private int currentGridSize;

        private Timer gameTimer;      // 통합 타이머
        private int playTimeSeconds;  // 전체 플레이 시간 (초)
        private int extremeTickCount; // 익스트림 기믹용 카운터
        private Point lockedPoint = new Point(-1, -1); // 익스트림 기믹용

        private struct DifficultyConfig
        {
            public int GridSize;
            public int ShuffleCount;
        }

        // 2. 생성자

        public BitFlipControl()
        {
            InitializeComponent();

            // 1. 타이머를 먼저 설정하고 이벤트를 연결합니다.
            gameTimer = new Timer();
            gameTimer.Interval = 1000;
            gameTimer.Tick += GameTimer_Tick;

            if (!DesignMode)
            {
                // 2. 콤보박스 기본값을 먼저 정해야 StartNewGame 내부의 cmbDifficulty.Text 참조가 안전합니다.
                if (cmbDifficulty.Items.Count > 0) cmbDifficulty.SelectedIndex = 1;

                StartNewGame("Normal");
            }
        }


        // 3. 게임 제어 (핵심 흐름)
        // 메서드 선언에 async 추가
        private async void StartNewGame(string levelName)
        {
            var config = GetConfig(levelName);

            // 1. 기초 세팅
            CreateGrid(config.GridSize);
            gameTimer.Stop();

            // 2. 초기화
            for (int r = 0; r < config.GridSize; r++)
                for (int c = 0; c < config.GridSize; c++)
                    gridStates[c, r] = false;

            lockedPoint = new Point(-1, -1);

            UpdateUI();

            // 3. 애니메이션 메서드 호출 (비동기 대기)
            await ShuffleAnimationAsync(config.ShuffleCount, 60);

            // 4. 게임 시작
            playTimeSeconds = 0;
            extremeTickCount = 0;
            lblTimer.Text = "00:00";
            gameTimer.Start();
        }

        private async void CheckVictory()
        {
            bool isAllClear = gridStates.Cast<bool>().All(state => state);

            if (isAllClear)
            {
                gameTimer.Stop(); // 타이머 즉시 정지

                // [추가] 승리 시 경고 문구 제거 및 텍스트 정돈
                TimeSpan t = TimeSpan.FromSeconds(playTimeSeconds);
                string finalTime = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
                lblTimer.Text = finalTime; // 화면상의 타이머도 클리어 타임으로 고정
                lblTimer.ForeColor = Color.White; // 색상 복구

                if (cmbDifficulty.Text == "Extreme")
                {
                    // 익스트림 클리어 시에만 화려한 흔들기 연출
                    await ShakeScreen(800, 10);
                }

                // 정돈된 finalTime을 메시지에 사용
                MessageBox.Show($"Clear Time: {finalTime}\nLogic Synchronized!", "System Victory");

                StartNewGame(cmbDifficulty.Text);
            }
        }


        // 4. UI 생성 및 업데이트
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
                        Anchor = AnchorStyles.None,
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

        private void UpdateUI()
        {
            for (int r = 0; r < currentGridSize; r++)
            {
                for (int c = 0; c < currentGridSize; c++)
                {
                    var btn = gridButtons[c, r];
                    bool isLocked = (c == lockedPoint.X && r == lockedPoint.Y);

                    if (isLocked)
                    {
                        // [잠금 상태] 회색으로 변경 및 클릭 차단
                        btn.Enabled = false;
                        btn.Text = "LOCK"; // 텍스트로 한 번 더 강조
                        btn.Type = MaterialButton.MaterialButtonType.Contained;
                    }
                    else
                    {
                        // [일반 상태] 활성화 및 상태 반영
                        btn.Enabled = true;
                        btn.Text = "";
                        btn.Type = gridStates[c, r] ? MaterialButton.MaterialButtonType.Contained : MaterialButton.MaterialButtonType.Outlined;
                        btn.HighEmphasis = gridStates[c, r];
                        btn.UseAccentColor = gridStates[c, r];
                    }
                    btn.Invalidate();
                }
            }
        }

        // 5. 로직 및 이벤트 핸들러
        private void FlipSwitch(int x, int y)
        {
            ToggleBit(x, y);     // 자기 자신
            ToggleBit(x, y - 1); // 상
            ToggleBit(x, y + 1); // 하
            ToggleBit(x - 1, y); // 좌
            ToggleBit(x + 1, y); // 우
        }

        private void ToggleBit(int x, int y)
        {
            // [버그 수정] 기존 5 대신 currentGridSize를 사용하여 범위 체크
            if (x >= 0 && x < currentGridSize && y >= 0 && y < currentGridSize)
            {
                gridStates[x, y] = !gridStates[x, y];
            }
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

        private async void OnGridClick(object sender, EventArgs e) // async 추가
        {
            if (!(sender is MaterialButton btn)) return;
            Point pos = (Point)btn.Tag;

            // 잠긴 버튼 처리
            if (pos == lockedPoint)
            {
                // 경고 원인: await를 붙여서 흔들림이 끝날 때까지 기다리게 합니다.
                await ShakeScreen(100, 2);
                return;
            }

            this.ActiveControl = tableLayoutPanel1;
            FlipSwitch(pos.X, pos.Y);
            UpdateUI();
            CheckVictory();
        }

        private void cmbDifficulty_SelectedIndexChanged(object sender, EventArgs e)
        {
            StartNewGame(cmbDifficulty.Text);
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            if (this.ParentForm is SandBox sb)
            {
                sb.BackToList();
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            string selectedLevel = string.IsNullOrEmpty(cmbDifficulty.Text) ? "Normal" : cmbDifficulty.Text;

            StartNewGame(selectedLevel);
        }

        private async void GameTimer_Tick(object sender, EventArgs e)
        {
            // 타이머 중복 실행 방지 (흔들리는 동안 Tick 무시)
            if (tableLayoutPanel1.Enabled == false) return;

            playTimeSeconds++;
            TimeSpan t = TimeSpan.FromSeconds(playTimeSeconds);
            string timeStr = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);

            if (cmbDifficulty.Text == "Extreme")
            {
                extremeTickCount++;

                // 1. 잠금 기믹 (3초 시점: 경고 시작과 동시에 잠금)
                if (extremeTickCount == 5)
                {
                    SetRandomLock(); // 내부에서 UpdateUI() 혹은 버튼 색상 변경 포함 필수
                    lblTimer.ForeColor = Color.Red;
                    lblTimer.Text = timeStr + " [!] 시스템 불안정";
                }
                // 2. 경고 유지 (4초 시점)
                else if (extremeTickCount == 7)
                {
                    lblTimer.ForeColor = Color.White;
                    lblTimer.Text = timeStr + " [!] 시스템 불안정";
                }
                // 3. 발생 단계 (5초 시점)
                else if (extremeTickCount >= 8)
                {
                    lblTimer.ForeColor = Color.Red;
                    lblTimer.Text = timeStr + " [!] 데이터 변형 발생!";

                    // 조작 방지 및 흔들기/플립
                    int rx = rand.Next(0, currentGridSize);
                    int ry = rand.Next(0, currentGridSize);

                    var shakeTask = ShakeScreen(500, 12);
                    FlipSwitch(rx, ry);
                    FlashUpdate(rx, ry);

                    await shakeTask;

                    // 상태 복구 및 잠금 해제 (원한다면 여기서 해제)
                    extremeTickCount = 0;
                    lblTimer.ForeColor = Color.White;
                    lblTimer.Text = timeStr;

                    // 승리 체크 전 UI 최종 동기화
                    UpdateUI();
                    CheckVictory();
                }
                else
                {
                    lblTimer.Text = timeStr;
                }
            }
            else
            {
                lblTimer.Text = timeStr;
            }
        }


        private async System.Threading.Tasks.Task ShuffleAnimationAsync(int count, int baseDelay)
        {
            tableLayoutPanel1.Enabled = false;
            lblTimer.Text = "ENCRYPTING GRID...";

            // 셔플 횟수를 늘려 더 밀도 있게 표현 (기본 count의 1.5배 추천)
            int totalSteps = count;

            for (int i = 0; i < totalSteps; i++)
            {
                // [핵심] 한 번의 루프에서 2군데를 동시에 터뜨림
                for (int batch = 0; batch < 2; batch++)
                {
                    int rx = rand.Next(0, currentGridSize);
                    int ry = rand.Next(0, currentGridSize);

                    FlipSwitch(rx, ry);
                    FlashUpdate(rx, ry);
                }

                // 시스템 이벤트 처리 (화면 갱신 보장)
                Application.DoEvents();

                // 매우 빠른 속도 (20~30ms 추천)
                await System.Threading.Tasks.Task.Delay(20);

                // 진행률 표시 효과 (선택 사항)
                if (i % 5 == 0) lblTimer.Text = $"SHUFFLING {(int)((double)i / totalSteps * 100)}%";
            }

            // 마지막에 전체 화면을 깔끔하게 한 번 정돈
            UpdateUI();
            tableLayoutPanel1.Enabled = true;
        }

        private void FlashUpdate(int x, int y)
        {
            // 십자 범위 좌표
            int[] dx = { 0, 0, 0, -1, 1 };
            int[] dy = { 0, -1, 1, 0, 0 };

            for (int i = 0; i < 5; i++)
            {
                int nx = x + dx[i];
                int ny = y + dy[i];

                if (nx >= 0 && nx < currentGridSize && ny >= 0 && ny < currentGridSize)
                {
                    var btn = gridButtons[nx, ny];
                    bool state = gridStates[nx, ny];

                    // 스타일 즉시 강제 적용
                    btn.Type = state ? MaterialButton.MaterialButtonType.Contained : MaterialButton.MaterialButtonType.Outlined;
                    btn.HighEmphasis = state;
                    btn.UseAccentColor = state;

                    // Invalidate보다 강력한 Refresh로 즉시 렌더링 강제
                    btn.Refresh();
                }
            }
        }

        private async System.Threading.Tasks.Task ShakeScreen(int durationMs = 300, int amplitude = 6)
        {
            // 원래 위치와 Dock 상태 저장
            var originalDock = tableLayoutPanel1.Dock;
            Point originalPos = tableLayoutPanel1.Location;

            // 흔들기 위해 Dock을 잠시 해제 (Fill 상태면 좌표 이동이 안 됨)
            tableLayoutPanel1.Dock = DockStyle.None;

            Random shakeRand = new Random();
            DateTime endTime = DateTime.Now.AddMilliseconds(durationMs);

            while (DateTime.Now < endTime)
            {
                // 무작위 좌표로 이동
                tableLayoutPanel1.Location = new Point(
                    originalPos.X + shakeRand.Next(-amplitude, amplitude + 1),
                    originalPos.Y + shakeRand.Next(-amplitude, amplitude + 1)
                );

                await System.Threading.Tasks.Task.Delay(20);
            }

            // 복구
            tableLayoutPanel1.Location = originalPos;
            tableLayoutPanel1.Dock = originalDock;
        }

        private void SetRandomLock()
        {
            // 1. 이전 잠긴 버튼의 스타일 강제 초기화
            if (lockedPoint.X != -1)
            {
                var prevBtn = gridButtons[lockedPoint.X, lockedPoint.Y];
                prevBtn.Text = "";
                // 스타일을 일반 상태로 돌려놓음 (UpdateUI가 전체를 돌기 전 선제 조치)
                prevBtn.UseAccentColor = gridStates[lockedPoint.X, lockedPoint.Y];
            }

            // 2. 새로운 랜덤 위치 선정
            lockedPoint = new Point(rand.Next(0, currentGridSize), rand.Next(0, currentGridSize));

            // 3. UI 전체 갱신 (새로운 잠금 위치 반영)
            UpdateUI();
        }
    }
}