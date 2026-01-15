using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GateHelper
{
    // [1] 기믹 및 게임 로직 관리자 (Brain)
    public class GimmickManager
    {
        private BitFlipControl _parent;
        private Random _rand = new Random();
        public List<GimmickBase> ActiveGimmicks { get; private set; } = new List<GimmickBase>();

        public GimmickManager(BitFlipControl parent) => _parent = parent;

        public async Task ApplyRandomGimmicksAsync()
        {
            ActiveGimmicks.Clear();

            // 현재 구현된 기믹들
            var pool = new List<GimmickBase> {
                //new ShakeGimmick(_parent),       
                //new LockGimmick(_parent),        
                new NoiseGimmick(_parent),       
                //new VerticalFlipGimmick(_parent)
            };

            // 풀에 있는 기믹 중 최대 3개를 랜덤하게 선정
            ActiveGimmicks = pool.OrderBy(x => _rand.Next()).Take(1).ToList();

            _parent.lblTimer.ForeColor = System.Drawing.Color.Red;
            for (int i = 0; i < 10; i++)
            {
                // 풀에서 랜덤한 이름을 보여주는 연출
                _parent.lblTimer.Text = pool[_rand.Next(pool.Count)].Name;
                await Task.Delay(100);
            }

            _parent.lblTimer.Text = "ACTIVE: " + string.Join(" | ", ActiveGimmicks.Select(g => g.Name));
            await Task.Delay(1500);
        }

        public void FlipSwitch(int x, int y)
        {
            ToggleBit(x, y);
            ToggleBit(x, y - 1);
            ToggleBit(x, y + 1);
            ToggleBit(x - 1, y);
            ToggleBit(x + 1, y);
        }

        private void ToggleBit(int x, int y)
        {
            if (x >= 0 && x < _parent.currentGridSize && y >= 0 && y < _parent.currentGridSize)
            {
                _parent.gridStates[x, y] = !_parent.gridStates[x, y];
            }
        }

        public async Task UpdateTickAsync()
        {
            List<Task> executionTasks = new List<Task>();

            foreach (var gimmick in ActiveGimmicks)
            {
                gimmick.ElapsedSeconds++;
                if (gimmick.ElapsedSeconds >= gimmick.Interval)
                {
                    executionTasks.Add(gimmick.ExecuteAsync());
                    gimmick.ElapsedSeconds = 0;
                }
            }

            if (executionTasks.Count > 0)
            {
                // [수정] 여기서 일괄적으로 Enabled = false를 하던 코드를 삭제했습니다.
                await Task.WhenAll(executionTasks);
                _parent.UpdateUI();
            }
        }
    }

    // [2] 기믹 추상 부모 클래스
    public abstract class GimmickBase
    {
        public string Name { get; protected set; }
        public int Interval { get; protected set; }
        public int ElapsedSeconds { get; set; } = 0;

        protected BitFlipControl Parent;
        protected GimmickBase(string name, int interval, BitFlipControl parent)
        {
            Name = name;
            Interval = interval;
            Parent = parent;
        }

        public abstract Task ExecuteAsync();
    }

    // [3] 개별 기믹 클래스들
    public class ShakeGimmick : GimmickBase
    {
        public ShakeGimmick(BitFlipControl parent) : base("화면 흔들림", 9, parent) { }
        public override async Task ExecuteAsync()
        {
            Parent.tableLayoutPanel1.Enabled = false; // 실행 동안 클릭 차단
            // 랜덤 플립 1회
            Parent.GimmickHandler.FlipSwitch(Parent.rand.Next(Parent.currentGridSize), Parent.rand.Next(Parent.currentGridSize));
            // 화면 흔들기
            await Parent.ShakeScreen(500, 12);
            Parent.tableLayoutPanel1.Enabled = true; // 클릭 복구
        }
    }

    public class LockGimmick : GimmickBase
    {
        public LockGimmick(BitFlipControl parent) : base("시스템 잠금", 7, parent) { }
        public override async Task ExecuteAsync()
        {
            Parent.tableLayoutPanel1.Enabled = false; // 실행 동안 클릭 차단
            Parent.GimmickHandler.FlipSwitch(Parent.rand.Next(Parent.currentGridSize), Parent.rand.Next(Parent.currentGridSize));
            await Parent.ShakeScreen(500, 12);
            Parent.tableLayoutPanel1.Enabled = true; // 클릭 복구
        }

    }

    // [기믹 3] 데이터 노이즈
    public class NoiseGimmick : GimmickBase
    {
        public NoiseGimmick(BitFlipControl parent) : base("데이터 노이즈", 4, parent) { }

        public override async Task ExecuteAsync()
        {
            // 글리치에 사용할 네온 컬러 배열
            System.Drawing.Color[] glitchColors = {
            System.Drawing.Color.Lime,    // 네온 그린
            System.Drawing.Color.Cyan,    // 사이언
            System.Drawing.Color.Magenta, // 마젠타
            System.Drawing.Color.Yellow,  // 노란색
            System.Drawing.Color.White    // 흰색
        };

            for (int i = 0; i < 8; i++)
            {
                foreach (var btn in Parent.gridButtons)
                {
                    btn.UseAccentColor = false;
                    btn.BackColor = glitchColors[Parent.rand.Next(glitchColors.Length)];
                    btn.Refresh();
                }
                await Task.Delay(50);
            }

            // 효과 종료 후에는 매니저가 UpdateUI를 호출해 다시 붉은색/흰색으로 복구합니다.
        }
    }

    // [기믹 4] 상하반전
    public class VerticalFlipGimmick : GimmickBase
    {
        public VerticalFlipGimmick(BitFlipControl parent) : base("상하반전", 11, parent) { }

        public override async Task ExecuteAsync()
        {
            Parent.tableLayoutPanel1.Enabled = false; // 실행 동안 클릭 차단
            int size = Parent.currentGridSize;
            // 상하 반전 로직: y축의 절반만 돌면서 위아래 데이터를 교환함
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size / 2; y++)
                {
                    bool temp = Parent.gridStates[x, y];
                    Parent.gridStates[x, y] = Parent.gridStates[x, size - 1 - y];
                    Parent.gridStates[x, size - 1 - y] = temp;
                }
            }

            // 시각적 효과: 반전될 때 짧게 흔들림 추가
            await Parent.ShakeScreen(300, 5);
            Parent.tableLayoutPanel1.Enabled = true; // 클릭 복구
        }
    }




}