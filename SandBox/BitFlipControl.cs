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

        private struct DifficultyConfig
        {
            public int GridSize;
            public int ShuffleCount;
        }

        // 2. 생성자
        public BitFlipControl()
        {
            InitializeComponent();
            if (!DesignMode)
            {
                // 콤보박스 기본값 설정 (디자인에서 추가했다면 인덱스 설정)
                if (cmbDifficulty.Items.Count > 0) cmbDifficulty.SelectedIndex = 1; // Normal
                StartNewGame("Normal");
            }
        }

        // 3. 게임 제어 (핵심 흐름)
        private void StartNewGame(string levelName)
        {
            var config = GetConfig(levelName);

            // 격자 생성 및 배열 할당
            CreateGrid(config.GridSize);

            // 모든 상태 초기화 (false)
            for (int r = 0; r < config.GridSize; r++)
                for (int c = 0; c < config.GridSize; c++)
                    gridStates[c, r] = false;

            // 셔플 로직
            for (int i = 0; i < config.ShuffleCount; i++)
            {
                int rx = rand.Next(0, config.GridSize);
                int ry = rand.Next(0, config.GridSize);
                FlipSwitch(rx, ry);
            }
            UpdateUI();
        }

        private void CheckVictory()
        {
            // [수정] Cast<bool>()은 성능상 루프보다 느리지만 가독성은 좋습니다.
            // 모든 비트가 true(켜짐)인지 확인
            bool isAllClear = gridStates.Cast<bool>().All(state => state);

            if (isAllClear)
            {
                MessageBox.Show("All Bits Synchronized!", "Victory");
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
                    if (gridStates[c, r])
                    {
                        gridButtons[c, r].Type = MaterialButton.MaterialButtonType.Contained;
                        gridButtons[c, r].HighEmphasis = true;
                    }
                    else
                    {
                        gridButtons[c, r].Type = MaterialButton.MaterialButtonType.Outlined;
                        gridButtons[c, r].HighEmphasis = false;
                    }
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

        private void OnGridClick(object sender, EventArgs e)
        {
            if (!(sender is MaterialButton btn)) return;
            Point pos = (Point)btn.Tag;

            // 포커스 이동 방지
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
    }
}