using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GateHelper
{
    // [1] 기믹 및 게임 로직 관리자 (Brain)
    public class GimmickManager
    {
        private BitFlipControl _parent;
        private Random _rand = new Random();
        public List<GimmickBase> ActiveGimmicks { get; private set; } = new List<GimmickBase>();

        // 기믹 상태 변수 (지연 및 연쇄 반응 제어용)
        public bool IsLagActive { get; set; } = false;
        public bool IsChainActive { get; set; } = false;

        public GimmickManager(BitFlipControl parent) => _parent = parent;

        // [핵심] 시간에 따른 기믹 관리 로직 (0~60초+ 진행 단계)
        public void ManageProgression(int seconds)
        {
            // [추가] 20초마다 누적된 잠금을 리셋하여 플레이 공간을 확보합니다.
            if (seconds > 0 && seconds % 20 == 0)
            {
                _parent.lockedPoints.Clear();
                _parent.UpdateUI();
            }

            if (seconds >= 60)
            {
                // 60초 이후 글로벌 룰렛 (15초마다 교체)
                if ((seconds - 60) % 15 == 0)
                {
                    RefreshGimmicks(3, 3);
                    string names = string.Join(" ", ActiveGimmicks.Select(g => $"+{g.Name}"));
                    _ = _parent.ShowGimmickNotify(names);
                }
                return;
            }

            // 단계별 기믹 해금 (기존 로직)
            if (seconds == 0)
            {
                RefreshGimmicks(1, 1);
                if (ActiveGimmicks.Count > 0) _ = _parent.ShowGimmickNotify($"+{ActiveGimmicks[0].Name}");
            }
            else if (seconds == 10) AddGimmickFromTier(2);
            else if (seconds == 20) AddGimmickFromTier(2);
            else if (seconds == 40) AddGimmickFromTier(3);
        }

        // 티어별 기믹 풀 생성 (소수 주기를 사용하여 발동 겹침 최소화)
        private List<GimmickBase> GetPoolByTier(int maxTier)
        {
            var pool = new List<GimmickBase>();

            // 1티어: 시각적 자극 위주
            pool.Add(new NoiseGimmick(_parent));    // 3s
            pool.Add(new LockGimmick(_parent));     // 5s
            pool.Add(new ShakeGimmick(_parent));    // 7s

            if (maxTier >= 2)
            {
                // 2티어: 공간/기억 방해
                pool.Add(new TextShuffleGimmick(_parent));  // 11s
                pool.Add(new VerticalFlipGimmick(_parent)); // 13s
                pool.Add(new HorizontalFlipGimmick(_parent)); // 17s
                pool.Add(new BlackoutGimmick(_parent));     // 19s
            }

            if (maxTier >= 3)
            {
                // 3티어: 물리적/규칙 파괴
                pool.Add(new CursorDriftGimmick(_parent));   // 23s
                pool.Add(new InputLagGimmick(_parent));      // 29s
                pool.Add(new ChainReactionGimmick(_parent)); // 31s
            }

            return pool;
        }

        private void RefreshGimmicks(int count, int tier)
        {
            ResetAllGimmickStates(); // 이전 기믹 상태(지연 등) 완전 초기화
            var pool = GetPoolByTier(tier);
            ActiveGimmicks = pool.OrderBy(x => _rand.Next()).Take(count).ToList();
        }

        private void AddGimmickFromTier(int tier)
        {
            var pool = GetPoolByTier(tier);
            // 이미 활성화된 기믹은 제외하고 새로 뽑음
            var newGimmick = pool.Where(p => !ActiveGimmicks.Any(a => a.Name == p.Name))
                                 .OrderBy(x => _rand.Next())
                                 .FirstOrDefault();

            if (newGimmick != null)
            {
                ActiveGimmicks.Add(newGimmick);
                _ = _parent.ShowGimmickNotify($"+{newGimmick.Name}"); // 빨간 글씨 알림
            }
        }

        public void ResetAllGimmickStates()
        {
            IsLagActive = false;
            IsChainActive = false;
            _parent.lockedPoints.Clear();
            _parent.UpdateUI();
        }

        // 게임 시작 시 '익스트림 연출'용 메서드
        public async Task ApplyRandomGimmicksAsync()
        {
            ActiveGimmicks.Clear();
            var pool = GetPoolByTier(3);

            _parent.lblTimer.ForeColor = Color.Red;
            for (int i = 0; i < 10; i++)
            {
                _parent.lblTimer.Text = pool[_rand.Next(pool.Count)].Name;
                await Task.Delay(100);
            }
            await Task.Delay(1500);
        }

        public async void FlipSwitch(int x, int y)
        {
            if (IsLagActive) await Task.Delay(700); // 지연 효과

            if (IsChainActive)
            {
                for (int i = 0; i < _parent.currentGridSize; i++) ToggleBit(i, y); // 연쇄 효과
            }
            else
            {
                ToggleBit(x, y);
                ToggleBit(x, y - 1); ToggleBit(x, y + 1);
                ToggleBit(x - 1, y); ToggleBit(x + 1, y);
            }
            _parent.UpdateUI();
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
        protected GimmickBase(string name, int interval, BitFlipControl parent) { Name = name; Interval = interval; Parent = parent; }
        public abstract Task ExecuteAsync();
    }

    // --- 기믹 구현체들 (소수 주기 적용) ---
    public class NoiseGimmick : GimmickBase
    {
        public NoiseGimmick(BitFlipControl p) : base("노이즈", 3, p) { }
        public override async Task ExecuteAsync()
        {
            Color[] colors = { Color.Lime, Color.Cyan, Color.Magenta, Color.Yellow, Color.White };
            for (int i = 0; i < 8; i++)
            {
                foreach (var b in Parent.gridButtons) { b.UseAccentColor = false; b.BackColor = colors[Parent.rand.Next(colors.Length)]; b.Refresh(); }
                await Task.Delay(50);
            }
        }
    }
    // [기믹 2] 잠금
    public class LockGimmick : GimmickBase
    {
        public LockGimmick(BitFlipControl p) : base("잠금", 5, p) { }

        public override async Task ExecuteAsync()
        {
            int size = Parent.currentGridSize;

            // 새로운 잠금 위치를 찾습니다. (최대 10번 시도)
            Point newLock;
            int attempts = 0;
            do
            {
                newLock = new Point(Parent.rand.Next(size), Parent.rand.Next(size));
                attempts++;
            }
            while (Parent.lockedPoints.Contains(newLock) && attempts < 10);

            // 리스트에 추가 (누적)
            if (!Parent.lockedPoints.Contains(newLock))
            {
                Parent.lockedPoints.Add(newLock);
            }

            await Task.CompletedTask;
        }
    }

    // [기믹 1] 흔들림
    public class ShakeGimmick : GimmickBase
    {
        public ShakeGimmick(BitFlipControl p) : base("흔들림", 8, p) { }
        public override async Task ExecuteAsync()
        {
            Parent.tableLayoutPanel1.Enabled = false;
            Parent.GimmickHandler.FlipSwitch(Parent.rand.Next(Parent.currentGridSize), Parent.rand.Next(Parent.currentGridSize));
            await Parent.ShakeScreen(500, 12);
            Parent.tableLayoutPanel1.Enabled = true;
        }
    }

    // [기믹 10] 훼손 (텍스트 셔플)
    public class TextShuffleGimmick : GimmickBase
    {
        public TextShuffleGimmick(BitFlipControl p) : base("훼손", 9, p) { }
        public override async Task ExecuteAsync()
        {
            string[] g = { "0", "1", "?", "!", "X", "@", "#", "곽", "황", "벽", "천", "락", "샵" };
            for (int i = 0; i < 20; i++)
            {
                foreach (var b in Parent.gridButtons)
                {
                    b.Text = g[Parent.rand.Next(g.Length)];
                }
                await Task.Delay(150);
            }
        }
    }

    // [기믹 4] 상하반전
    public class VerticalFlipGimmick : GimmickBase
    {
        public VerticalFlipGimmick(BitFlipControl p) : base("상하반전", 10, p) { }
        public override async Task ExecuteAsync()
        {
            Parent.tableLayoutPanel1.Enabled = false;
            int s = Parent.currentGridSize;
            for (int x = 0; x < s; x++)
            {
                for (int y = 0; y < s / 2; y++)
                {
                    bool t = Parent.gridStates[x, y];
                    Parent.gridStates[x, y] = Parent.gridStates[x, s - 1 - y];
                    Parent.gridStates[x, s - 1 - y] = t;
                }
            }
            await Parent.ShakeScreen(300, 5);
            Parent.tableLayoutPanel1.Enabled = true;
        }
    }

    // [기믹 5] 좌우반전
    public class HorizontalFlipGimmick : GimmickBase
    {
        public HorizontalFlipGimmick(BitFlipControl p) : base("좌우반전", 11, p) { }
        public override async Task ExecuteAsync()
        {
            Parent.tableLayoutPanel1.Enabled = false;
            int s = Parent.currentGridSize;
            for (int y = 0; y < s; y++)
            {
                for (int x = 0; x < s / 2; x++)
                {
                    bool t = Parent.gridStates[x, y];
                    Parent.gridStates[x, y] = Parent.gridStates[s - 1 - x, y];
                    Parent.gridStates[s - 1 - x, y] = t;
                }
            }
            await Parent.ShakeScreen(300, 5);
            Parent.tableLayoutPanel1.Enabled = true;
        }
    }

    // [기믹 6] 블랙아웃
    public class BlackoutGimmick : GimmickBase
    {
        public BlackoutGimmick(BitFlipControl p) : base("블랙아웃", 12, p) { }
        public override async Task ExecuteAsync()
        {
            foreach (var b in Parent.gridButtons)
            {
                b.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
                b.UseAccentColor = false;
                b.Refresh();
            }
            await Task.Delay(2000);
        }
    }

    // [기믹 7] 드리프트
    public class CursorDriftGimmick : GimmickBase
    {
        public CursorDriftGimmick(BitFlipControl p) : base("드리프트", 13, p) { }
        public override async Task ExecuteAsync()
        {
            int dx = Parent.rand.Next(-3, 4);
            int dy = Parent.rand.Next(-3, 4);
            if (dx == 0 && dy == 0) dx = 2;
            for (int i = 0; i < 80; i++)
            {
                Point cp = Cursor.Position;
                Cursor.Position = new Point(cp.X + dx, cp.Y + dy);
                await Task.Delay(50);
            }
        }
    }

    // [기믹 8] 지연
    public class InputLagGimmick : GimmickBase
    {
        public InputLagGimmick(BitFlipControl p) : base("지연", 14, p) { }
        public override async Task ExecuteAsync()
        {
            Parent.GimmickHandler.IsLagActive = true;
            await Task.Delay(3000);
            Parent.GimmickHandler.IsLagActive = false;
        }
    }

    // [기믹 9] 연쇄
    public class ChainReactionGimmick : GimmickBase
    {
        public ChainReactionGimmick(BitFlipControl p) : base("연쇄", 15, p) { }
        public override async Task ExecuteAsync()
        {
            Parent.GimmickHandler.IsChainActive = true;
            await Task.Delay(5000);
            Parent.GimmickHandler.IsChainActive = false;
        }
    }
}