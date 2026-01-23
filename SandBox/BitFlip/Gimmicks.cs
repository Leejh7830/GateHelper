using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GateHelper
{
    // [1] 기믹 및 게임 로직 관리자
    public class GimmickManager
    {
        private BitFlipControl _parent;
        private Random _rand = new Random();
        public List<GimmickBase> ActiveGimmicks { get; private set; } = new List<GimmickBase>();
        public HashSet<string> UsedGimmicks { get; private set; } = new HashSet<string>();

        public bool IsLagActive { get; set; } = false;
        public bool IsChainActive { get; set; } = false;
        public bool IsFogActive { get; set; } = false;

        public GimmickManager(BitFlipControl parent) => _parent = parent;

        private List<GimmickBase> GetPoolByTier(int tier)
        {
            switch (tier)
            {
                case 1:
                    return new List<GimmickBase>
                    {
                        new NoiseGimmick_1T(_parent),
                        new LockGimmick_1T(_parent),
                        new ShakeGimmick_1T(_parent),
                        new BlindGimmick_1T(_parent)
                    };
                case 2:
                    return new List<GimmickBase>
                    {
                        new TextShuffleGimmick_2T(_parent),
                        new VerticalFlipGimmick_2T(_parent),
                        new HorizontalFlipGimmick_2T(_parent),
                        new BlackoutGimmick_2T(_parent),
                        new FogGimmick_2T(_parent)
                    };
                case 3:
                    return new List<GimmickBase>
                    {
                        new CursorDriftGimmick_3T(_parent),
                        new InputLagGimmick_3T(_parent),
                        new ChainReactionGimmick_3T(_parent),
                        new BombGimmick_3T(_parent)
                    };
                default:
                    return new List<GimmickBase>();
            }
        }

        public void RefreshGimmicks(int count, int maxTier)
        {
            ActiveGimmicks.Clear();
            List<GimmickBase> totalPool = new List<GimmickBase>();
            for (int i = 1; i <= maxTier; i++) totalPool.AddRange(GetPoolByTier(i));

            ActiveGimmicks = totalPool
                .GroupBy(g => g.Name)
                .Select(g => g.First())
                .OrderBy(x => _rand.Next())
                .Take(count)
                .ToList();
            RecordGimmicks(); // 기믹기록!
        }

        public void AddGimmickFromTier(int tier)
        {
            var pool = GetPoolByTier(tier);
            var available = pool.Where(p => !ActiveGimmicks.Any(a => a.Name == p.Name)).ToList();

            if (available.Count > 0)
            {
                var newGimmick = available[_rand.Next(available.Count)];
                ActiveGimmicks.Add(newGimmick);
                _ = _parent.ShowGimmickNotify($"+{newGimmick.Name}");
                UsedGimmicks.Add(newGimmick.Name); // 기록!
            }
        }

        public void ResetAllGimmickStates()
        {
            IsLagActive = false;
            IsChainActive = false;
            IsFogActive = false;
            _parent.lockedPoints.Clear();
            _parent.ActiveBombs.Clear();
            UsedGimmicks.Clear(); // 새로운 게임 시작 시 기록 초기화
            _parent.UpdateUI();
        }

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
            await Task.Delay(500);
        }

        public async void FlipSwitch(int x, int y)
        {
            if (IsLagActive) await Task.Delay(700);

            Point p = new Point(x, y);
            if (_parent.ActiveBombs.ContainsKey(p)) _parent.ActiveBombs.Remove(p);

            if (IsChainActive)
            {
                for (int i = 0; i < _parent.currentGridSize; i++) ToggleState(i, y);
            }
            else
            {
                ToggleState(x, y);
                ToggleState(x, y - 1); ToggleState(x, y + 1);
                ToggleState(x - 1, y); ToggleState(x + 1, y);
            }
            _parent.UpdateUI();
        }

        public void ToggleState(int x, int y)
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

        public void SetStaticGimmicks(params (int count, int tier)[] configs)
        {
            ActiveGimmicks.Clear();
            foreach (var config in configs)
            {
                var pool = GetPoolByTier(config.tier);
                var available = pool.Where(p => !ActiveGimmicks.Any(a => a.Name == p.Name)).ToList();
                ActiveGimmicks.AddRange(available.OrderBy(x => _rand.Next()).Take(config.count));
                RecordGimmicks(); // 기록!
            }
        }

        public void ManageProgression(int seconds, string levelName)
        {
            if (seconds > 0 && seconds % 20 == 0 && _parent.lockedPoints.Count > 0)
            {
                _parent.lockedPoints.Clear();
                _parent.UpdateUI();
                _ = _parent.ShowGimmickNotify("SYSTEM: LOCKS RELEASED");
            }

            if (seconds == 0 && ActiveGimmicks.Count > 0)
            {
                string names = string.Join(", ", ActiveGimmicks.Select(g => g.Name));
                _ = _parent.ShowGimmickNotify((levelName == "Extreme" ? "+" : "GIMMICKS: ") + names);
            }

            if (levelName != "Extreme") return;

            if (seconds >= 60)
            {
                if ((seconds - 60) % 15 == 0)
                {
                    RefreshGimmicks(3, 3);
                    _ = _parent.ShowGimmickNotify(string.Join(" ", ActiveGimmicks.Select(g => "+" + g.Name)));
                }
                return;
            }

            switch (seconds)
            {
                case 0: RefreshGimmicks(1, 1); break;
                case 10: AddGimmickFromTier(2); break;
                case 20: AddGimmickFromTier(2); break;
                case 40: AddGimmickFromTier(3); break;
            }
        }

        private void RecordGimmicks()
        {
            foreach (var g in ActiveGimmicks)
            {
                UsedGimmicks.Add(g.Name);
            }
        }


        /// <summary>
        /// 현재 게임이 해결가능한지 계산
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public bool IsSolvable(bool[,] grid, int size)
        {
            int n = size * size;
            // 확장 행렬 생성 (n x n+1)
            bool[,] matrix = new bool[n, n + 1];

            // 1. 행렬 A 구성 (각 버튼을 눌렀을 때의 영향도)
            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    int i = r * size + c;
                    // 자기 자신과 주변 십자(+) 방향 버튼들의 영향력 설정
                    int[] dx = { 0, 0, 0, 1, -1 };
                    int[] dy = { 0, 1, -1, 0, 0 };

                    for (int k = 0; k < 5; k++)
                    {
                        int nx = c + dx[k];
                        int ny = r + dy[k];
                        if (nx >= 0 && nx < size && ny >= 0 && ny < size)
                        {
                            matrix[ny * size + nx, i] = true;
                        }
                    }
                    // 목표 상태: 모든 비트를 1(True)로 만드는 것이므로, 
                    // 현재 꺼져 있는(False) 칸을 뒤집어야 함.
                    matrix[i, n] = !grid[c, r];
                }
            }

            // 2. 가우스 소거법 실행 (Forward Elimination)
            int pivot = 0;
            for (int j = 0; j < n && pivot < n; j++)
            {
                int sel = pivot;
                while (sel < n && !matrix[sel, j]) sel++;
                if (sel == n) continue;

                // 행 바꿈
                for (int k = j; k <= n; k++)
                {
                    bool temp = matrix[sel, k];
                    matrix[sel, k] = matrix[pivot, k];
                    matrix[pivot, k] = temp;
                }

                // 다른 행들 XOR 연산
                for (int i = 0; i < n; i++)
                {
                    if (i != pivot && matrix[i, j])
                    {
                        for (int k = j; k <= n; k++)
                            matrix[i, k] ^= matrix[pivot, k];
                    }
                }
                pivot++;
            }

            // 3. 해 존재 여부 판별
            // 0 = 1 형태의 행이 존재하면 해가 없음 (Unsolvable)
            for (int i = 0; i < n; i++)
            {
                bool allZeros = true;
                for (int j = 0; j < n; j++) if (matrix[i, j]) allZeros = false;
                if (allZeros && matrix[i, n]) return false;
            }

            return true;
        }













    }

    // [2] 기믹 추상 부모 클래스
    public abstract class GimmickBase
    {
        public string Name { get; protected set; }
        public int Interval { get; protected set; }
        public int ElapsedSeconds { get; set; } = 0;
        protected BitFlipControl _parent;

        protected GimmickBase(string name, int interval, BitFlipControl parent)
        {
            Name = name;
            Interval = interval;
            _parent = parent;
        }

        public abstract Task ExecuteAsync();

        protected Point GetRandomPoint()
        {
            return new Point(_parent.rand.Next(_parent.currentGridSize), _parent.rand.Next(_parent.currentGridSize));
        }
    }

    // --- 기믹 구현체들 ---
    public class NoiseGimmick_1T : GimmickBase
    {
        public NoiseGimmick_1T(BitFlipControl p) : base("노이즈", 3, p) { }
        public override async Task ExecuteAsync()
        {
            Color[] colors = { Color.Lime, Color.Cyan, Color.Magenta, Color.Yellow, Color.White };
            for (int i = 0; i < 8; i++)
            {
                foreach (var b in _parent.gridButtons) b.BackColor = colors[_parent.rand.Next(colors.Length)];
                await Task.Delay(50);
            }
        }
    }

    public class LockGimmick_1T : GimmickBase
    {
        public LockGimmick_1T(BitFlipControl p) : base("잠금", 5, p) { }
        public override async Task ExecuteAsync()
        {
            Point p = GetRandomPoint();
            if (!_parent.lockedPoints.Contains(p)) _parent.lockedPoints.Add(p);
            await Task.CompletedTask;
        }
    }

    public class ShakeGimmick_1T : GimmickBase
    {
        public ShakeGimmick_1T(BitFlipControl p) : base("흔들림", 8, p) { }
        public override async Task ExecuteAsync()
        {
            _parent.GimmickHandler.FlipSwitch(_parent.rand.Next(_parent.currentGridSize), _parent.rand.Next(_parent.currentGridSize));
            await _parent.ShakeScreen(500, 12);
        }
    }

    public class BlindGimmick_1T : GimmickBase
    {
        public BlindGimmick_1T(BitFlipControl p) : base("블라인드", 6, p) { } // 6초 주기
        public override async Task ExecuteAsync()
        {
            // 모든 버튼을 일시적으로 검게 가림
            foreach (var b in _parent.gridButtons)
            {
                b.BackColor = Color.Black;
                b.Text = "";
            }
            await Task.Delay(1000); // 1초간 유지
            _parent.UpdateUI();
            await Task.CompletedTask;
        }
    }

    public class TextShuffleGimmick_2T : GimmickBase
    {
        public TextShuffleGimmick_2T(BitFlipControl p) : base("훼손", 9, p) { }

        public override async Task ExecuteAsync()
        {
            string[] symbols = { "0", "1", "?", "!", "X", "@", "#" };
            // 노이즈 연출을 위한 무작위 색상들 (보호 대상이 아닌 칸에만 적용)
            Color[] noiseColors = { Color.Gray, Color.DimGray, Color.Silver };

            for (int i = 0; i < 10; i++)
            {
                foreach (var btn in _parent.gridButtons)
                {
                    Point pos = (Point)btn.Tag;

                    // [보호 조건] 폭탄이 있거나 잠긴 칸인지 확인
                    bool isProtected = _parent.ActiveBombs.ContainsKey(pos) || _parent.lockedPoints.Contains(pos);

                    if (!isProtected)
                    {
                        // 1. 일반 칸은 텍스트와 글자색을 무작위로 섞어 노이즈 연출
                        btn.Text = symbols[_parent.rand.Next(symbols.Length)];
                        btn.ForeColor = noiseColors[_parent.rand.Next(noiseColors.Length)];
                    }
                    // 2. 보호 대상인 칸은 건드리지 않음 (UpdateUI에서 설정한 노란색/빨간색 유지)
                }
                await Task.Delay(100);
            }

            // 연출 종료 후 모든 버튼을 정상 상태(하얀색 글자 등)로 복구
            _parent.UpdateUI();
            await Task.CompletedTask;
        }
    }

    public class VerticalFlipGimmick_2T : GimmickBase
    {
        public VerticalFlipGimmick_2T(BitFlipControl p) : base("상하반전", 10, p) { }
        public override async Task ExecuteAsync()
        {
            int s = _parent.currentGridSize;
            for (int x = 0; x < s; x++)
            {
                for (int y = 0; y < s / 2; y++)
                {
                    bool t = _parent.gridStates[x, y];
                    _parent.gridStates[x, y] = _parent.gridStates[x, s - 1 - y];
                    _parent.gridStates[x, s - 1 - y] = t;
                }
            }
            await _parent.ShakeScreen(300, 5);
        }
    }

    public class HorizontalFlipGimmick_2T : GimmickBase
    {
        public HorizontalFlipGimmick_2T(BitFlipControl p) : base("좌우반전", 11, p) { }
        public override async Task ExecuteAsync()
        {
            int s = _parent.currentGridSize;
            for (int y = 0; y < s; y++)
            {
                for (int x = 0; x < s / 2; x++)
                {
                    bool t = _parent.gridStates[x, y];
                    _parent.gridStates[x, y] = _parent.gridStates[s - 1 - x, y];
                    _parent.gridStates[s - 1 - x, y] = t;
                }
            }
            await _parent.ShakeScreen(300, 5);
        }
    }

    public class BlackoutGimmick_2T : GimmickBase
    {
        public BlackoutGimmick_2T(BitFlipControl p) : base("블랙아웃", 12, p) { }
        public override async Task ExecuteAsync()
        {
            foreach (var b in _parent.gridButtons) b.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            await Task.Delay(2000);
        }
    }

    public class FogGimmick_2T : GimmickBase
    {
        public FogGimmick_2T(BitFlipControl p) : base("안개", 15, p) { }

        public override async Task ExecuteAsync()
        {
            _parent.GimmickHandler.IsFogActive = true;

            DateTime endTime = DateTime.Now.AddSeconds(7);
            while (DateTime.Now < endTime)
            {
                // [추가] 부모 창이 닫혔다면 루프 즉시 종료
                if (_parent.IsDisposed || _parent.Disposing) return;

                _parent.UpdateUI();
                await Task.Delay(50);
            }

            if (_parent.IsDisposed || _parent.Disposing) return;
            _parent.GimmickHandler.IsFogActive = false;
            _parent.UpdateUI();
        }
    }

    public class CursorDriftGimmick_3T : GimmickBase
    {
        public CursorDriftGimmick_3T(BitFlipControl p) : base("드리프트", 13, p) { }
        public override async Task ExecuteAsync()
        {
            int dx = _parent.rand.Next(-3, 4);
            int dy = _parent.rand.Next(-3, 4);
            for (int i = 0; i < 50; i++)
            {
                Cursor.Position = new Point(Cursor.Position.X + dx, Cursor.Position.Y + dy);
                await Task.Delay(40);
            }
        }
    }

    public class InputLagGimmick_3T : GimmickBase
    {
        public InputLagGimmick_3T(BitFlipControl p) : base("지연", 14, p) { }
        public override async Task ExecuteAsync()
        {
            _parent.GimmickHandler.IsLagActive = true;
            await Task.Delay(3000);
            _parent.GimmickHandler.IsLagActive = false;
        }
    }

    public class ChainReactionGimmick_3T : GimmickBase
    {
        public ChainReactionGimmick_3T(BitFlipControl p) : base("연쇄", 15, p) { }
        public override async Task ExecuteAsync()
        {
            _parent.GimmickHandler.IsChainActive = true;
            await Task.Delay(5000);
            _parent.GimmickHandler.IsChainActive = false;
        }
    }

    public class BombGimmick_3T : GimmickBase
    {
        private int _spawnCounter = 0;
        public BombGimmick_3T(BitFlipControl p) : base("BOMB", 1, p) { }

        public override async Task ExecuteAsync()
        {
            // 1. 카운트다운 및 폭발 로직
            var exploded = new List<Point>();
            var bombPoints = _parent.ActiveBombs.Keys.ToList();
            foreach (var p in bombPoints)
            {
                _parent.ActiveBombs[p]--;
                if (_parent.ActiveBombs[p] <= 0) exploded.Add(p);
            }

            foreach (var p in exploded)
            {
                _parent.ActiveBombs.Remove(p);
                for (int r = p.Y - 1; r <= p.Y + 1; r++)
                {
                    for (int c = p.X - 1; c <= p.X + 1; c++) _parent.GimmickHandler.ToggleState(c, r);
                }
                _ = _parent.ShowGimmickNotify("BOOM!!");
            }

            // 2. 새로운 폭탄 생성 로직
            _spawnCounter++;
            if (_spawnCounter >= 8)
            {
                _spawnCounter = 0;
                Point p = GetRandomPoint();
                if (!_parent.ActiveBombs.ContainsKey(p) && !_parent.lockedPoints.Contains(p))
                {
                    _parent.ActiveBombs[p] = 5;
                }
            }

            await Task.CompletedTask;
        }
    }
}