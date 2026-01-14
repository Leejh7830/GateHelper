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
                new ShakeGimmick(_parent),
                new LockGimmick(_parent),
                new NoiseGimmick(_parent) // [추가] 빈 껍데기라도 넣어둬야 Take(3)이 의미가 있습니다.
            };

            // 풀에 있는 기믹 중 최대 3개를 랜덤하게 선정
            ActiveGimmicks = pool.OrderBy(x => _rand.Next()).Take(3).ToList();

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
                // [최적화] 실행할 기믹이 있을 때만 조작을 방지합니다.
                _parent.tableLayoutPanel1.Enabled = false;

                await Task.WhenAll(executionTasks);

                _parent.UpdateUI();
                _parent.tableLayoutPanel1.Enabled = true;
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
        public ShakeGimmick(BitFlipControl parent) : base("화면 흔들림", 5, parent) { }
        public override async Task ExecuteAsync()
        {
            // 랜덤 플립 1회
            Parent.GimmickHandler.FlipSwitch(Parent.rand.Next(Parent.currentGridSize), Parent.rand.Next(Parent.currentGridSize));
            // 화면 흔들기
            await Parent.ShakeScreen(500, 12);
        }
    }

    public class LockGimmick : GimmickBase
    {
        public LockGimmick(BitFlipControl parent) : base("시스템 잠금", 3, parent) { }
        public override async Task ExecuteAsync()
        {
            // 잠금 위치만 변경 (UI 업데이트는 매니저가 일괄 처리)
            Parent.lockedPoint = new System.Drawing.Point(Parent.rand.Next(Parent.currentGridSize), Parent.rand.Next(Parent.currentGridSize));
            await Task.CompletedTask;
        }
    }

    public class NoiseGimmick : GimmickBase
    {
        public NoiseGimmick(BitFlipControl parent) : base("데이터 노이즈", 2, parent) { }
        public override async Task ExecuteAsync()
        {
            // 곧 구현할 번쩍임 효과 자리
            await Task.CompletedTask;
        }
    }
}