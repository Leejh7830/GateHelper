using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace GateHelper
{
    /// <summary>
    /// 실행 취소(Undo)를 위한 시그널 링크 상태 스냅샷
    /// </summary>
    public class SignalSnapshot
    {
        public Dictionary<int, List<Point>> CompletedPaths { get; }
        public int MoveCount { get; }

        public SignalSnapshot(Dictionary<int, List<Point>> paths, int moveCount)
        {
            CompletedPaths = new Dictionary<int, List<Point>>();
            foreach (var kvp in paths)
            {
                CompletedPaths[kvp.Key] = new List<Point>(kvp.Value);
            }
            MoveCount = moveCount;
        }
    }

    /// <summary>
    /// 시그널 링크 핵심 퍼즐 엔진 및 고속 회로 생성기
    /// </summary>
    public class SignalManager
    {
        private static readonly Random _rand = new Random();

        public int GridSize { get; private set; } = 6;
        public string Difficulty { get; private set; } = "Normal";
        public bool IsStealthMode { get; private set; } = false; // 고난이도 암기 모드

        public SignalNode[,] Grid { get; private set; }
        public int PairCount { get; private set; }

        // 정답 경로 (AI 힌트 및 완성도 검증용)
        public Dictionary<int, List<Point>> SolutionPaths { get; private set; } = new Dictionary<int, List<Point>>();

        // 사용자가 완성한 경로 목록
        public Dictionary<int, List<Point>> CompletedPaths { get; private set; } = new Dictionary<int, List<Point>>();

        // Undo 스택
        public Stack<SignalSnapshot> UndoStack { get; } = new Stack<SignalSnapshot>();

        public int MoveCount { get; private set; } = 0;
        public int HintsUsed { get; private set; } = 0;
        public bool IsGameActive { get; private set; } = false;
        public bool IsVictory { get; private set; } = false;

        public SignalManager()
        {
            InitializeGrid("Normal");
        }

        public void InitializeGrid(string difficulty)
        {
            Difficulty = difficulty;
            UndoStack.Clear();
            CompletedPaths.Clear();
            SolutionPaths.Clear();
            MoveCount = 0;
            HintsUsed = 0;
            IsVictory = false;

            int size;
            int pairs;

            switch (difficulty)
            {
                case "Easy":
                    size = 5;
                    pairs = 3;
                    IsStealthMode = false;
                    break;
                case "Normal":
                    size = 6;
                    pairs = 4;
                    IsStealthMode = false;
                    break;
                case "Hard":
                    size = 8;
                    pairs = 6;
                    IsStealthMode = false;
                    break;
                case "Expert":
                    size = 10;
                    pairs = 7;
                    IsStealthMode = true; // 사용자가 즐기는 스텔스 암기 모드 전격 활성화
                    break;
                default:
                    size = 6;
                    pairs = 4;
                    IsStealthMode = false;
                    break;
            }

            GridSize = size;
            PairCount = pairs;

            // 1. 고속 맵 생성 시도 (최대 25회 재시도)
            bool generated = false;
            for (int attempt = 0; attempt < 25; attempt++)
            {
                if (TryGenerateValidPerfectMap(size, pairs))
                {
                    generated = true;
                    break;
                }
            }

            // 2. 만약 시간 내 100% 충진 맵이 나오지 않았을 경우, UI 프리징 방지를 위한 검증된 고속 폴백 템플릿 로드
            if (!generated)
            {
                LoadVerifiedFallbackPuzzle(size, pairs);
            }

            // 3. 스텔스 모드 속성 적용
            if (IsStealthMode)
            {
                for (int y = 0; y < GridSize; y++)
                    for (int x = 0; x < GridSize; x++)
                        if (Grid[x, y].Type == NodeType.Terminal)
                            Grid[x, y].IsStealth = true;
            }

            IsGameActive = true;
        }

        // 구버전 호환용 오버로드
        public void InitializeGrid(int size, int pairCount)
        {
            string diff = size <= 5 ? "Easy" : (size <= 6 ? "Normal" : (size <= 8 ? "Hard" : "Expert"));
            InitializeGrid(diff);
        }

        /// <summary>
        /// 스네이크 확장 + 빈칸 우회 흡수 100% 충진 알고리즘
        /// </summary>
        private bool TryGenerateValidPerfectMap(int size, int pairCount)
        {
            Grid = new SignalNode[size, size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    Grid[x, y] = new SignalNode(x, y);

            HashSet<Point> occupied = new HashSet<Point>();
            List<List<Point>> allPaths = new List<List<Point>>();

            // 1. 초기 씨앗 배치
            for (int i = 0; i < pairCount; i++)
            {
                Point start;
                int safety = 0;
                do
                {
                    start = new Point(_rand.Next(size), _rand.Next(size));
                    if (++safety > 100) return false;
                } while (occupied.Contains(start));

                allPaths.Add(new List<Point> { start });
                occupied.Add(start);
            }

            // 2. 물리적 뱀형 확장 (Snake Growth)
            bool moved = true;
            while (moved)
            {
                moved = false;
                var indices = Enumerable.Range(0, pairCount).OrderBy(_ => _rand.Next()).ToList();
                foreach (int idx in indices)
                {
                    var path = allPaths[idx];
                    Point[] currentEnds = { path.First(), path.Last() };

                    foreach (var end in currentEnds)
                    {
                        var neighbors = GetEmptyNeighbors(end, occupied, size);
                        if (neighbors.Count > 0)
                        {
                            Point next = neighbors[_rand.Next(neighbors.Count)];
                            if (end == path.First()) path.Insert(0, next);
                            else path.Add(next);

                            occupied.Add(next);
                            moved = true;
                            break;
                        }
                    }
                }
            }

            // 3. 스마트 빈칸 흡수 우회
            if (occupied.Count < size * size)
            {
                if (!SmartRerouteHoles(allPaths, occupied, size)) return false;
            }

            // 4. 경로 확정 및 터미널 배치
            SolutionPaths.Clear();
            for (int i = 0; i < pairCount; i++)
            {
                var p = allPaths[i];
                if (p.Count < 2) return false;

                int colorId = i + 1;
                SolutionPaths[colorId] = new List<Point>(p);

                Grid[p[0].X, p[0].Y].Type = NodeType.Terminal;
                Grid[p[0].X, p[0].Y].ColorID = colorId;

                Grid[p.Last().X, p.Last().Y].Type = NodeType.Terminal;
                Grid[p.Last().X, p.Last().Y].ColorID = colorId;
            }

            return true;
        }

        private bool SmartRerouteHoles(List<List<Point>> allPaths, HashSet<Point> occupied, int size)
        {
            bool changed = true;
            int passes = 0;
            while (changed && passes++ < 15)
            {
                changed = false;
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        Point hole = new Point(x, y);
                        if (occupied.Contains(hole)) continue;

                        var neighbors = GetAllNeighbors(hole, size);
                        foreach (var path in allPaths)
                        {
                            for (int i = 0; i < path.Count - 1; i++)
                            {
                                if (neighbors.Contains(path[i]) && neighbors.Contains(path[i + 1]))
                                {
                                    path.Insert(i + 1, hole);
                                    occupied.Add(hole);
                                    changed = true;
                                    goto NextHole;
                                }
                            }
                        }
                    }
                NextHole:;
                }
            }
            return occupied.Count == size * size;
        }

        /// <summary>
        /// 대형 맵 등에서 고속 생성이 지연될 때 0ms 즉시 보장되는 검증된 대칭 변환 템플릿 로더
        /// </summary>
        private void LoadVerifiedFallbackPuzzle(int size, int pairs)
        {
            Grid = new SignalNode[size, size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    Grid[x, y] = new SignalNode(x, y);

            SolutionPaths.Clear();

            // 지그재그 분할 템플릿 생성
            int rowsPerPair = Math.Max(1, size / pairs);

            for (int r = 0; r < size; r++)
            {
                int colorId = Math.Min(pairs, (r / rowsPerPair) + 1);
                if (!SolutionPaths.ContainsKey(colorId))
                    SolutionPaths[colorId] = new List<Point>();

                if (r % 2 == 0)
                {
                    for (int c = 0; c < size; c++)
                        SolutionPaths[colorId].Add(new Point(c, r));
                }
                else
                {
                    for (int c = size - 1; c >= 0; c--)
                        SolutionPaths[colorId].Add(new Point(c, r));
                }
            }

            // 노드 배치
            foreach (var kvp in SolutionPaths)
            {
                var path = kvp.Value;
                if (path.Count >= 2)
                {
                    Grid[path[0].X, path[0].Y].Type = NodeType.Terminal;
                    Grid[path[0].X, path[0].Y].ColorID = kvp.Key;

                    Grid[path.Last().X, path.Last().Y].Type = NodeType.Terminal;
                    Grid[path.Last().X, path.Last().Y].ColorID = kvp.Key;
                }
            }
        }

        private List<Point> GetEmptyNeighbors(Point p, HashSet<Point> occupied, int size)
        {
            var res = new List<Point>();
            Point[] dirs = { new Point(0, 1), new Point(0, -1), new Point(1, 0), new Point(-1, 0) };
            foreach (var d in dirs)
            {
                Point n = new Point(p.X + d.X, p.Y + d.Y);
                if (n.X >= 0 && n.X < size && n.Y >= 0 && n.Y < size && !occupied.Contains(n))
                    res.Add(n);
            }
            return res;
        }

        private List<Point> GetAllNeighbors(Point p, int size)
        {
            var res = new List<Point>();
            Point[] dirs = { new Point(0, 1), new Point(0, -1), new Point(1, 0), new Point(-1, 0) };
            foreach (var d in dirs)
            {
                Point n = new Point(p.X + d.X, p.Y + d.Y);
                if (n.X >= 0 && n.X < size && n.Y >= 0 && n.Y < size)
                    res.Add(n);
            }
            return res;
        }

        // ==========================================
        // 인게임 조작 및 상태 관리
        // ==========================================

        /// <summary>
        /// 신호 경로 연결 확정
        /// </summary>
        public bool CommitPath(int colorId, List<Point> path)
        {
            if (path == null || path.Count < 2) return false;

            Point start = path[0];
            Point end = path[path.Count - 1];

            var startNode = Grid[start.X, start.Y];
            var endNode = Grid[end.X, end.Y];

            // 시작점과 끝점이 같은 색상의 단말 노드이고 서로 다른 좌표여야 함
            if (startNode.Type == NodeType.Terminal && endNode.Type == NodeType.Terminal &&
                startNode.ColorID == colorId && endNode.ColorID == colorId && start != end)
            {
                // 스냅샷 저장 (Undo 지원)
                UndoStack.Push(new SignalSnapshot(CompletedPaths, MoveCount));
                MoveCount++;

                CompletedPaths[colorId] = new List<Point>(path);

                // 연결 성공 펄스 부여
                startNode.ConnectPulse = 1.0f;
                endNode.ConnectPulse = 1.0f;

                CheckVictory();
                return true;
            }

            return false;
        }

        /// <summary>
        /// 단일 신호선 연결 해제
        /// </summary>
        public void RemovePath(int colorId)
        {
            if (CompletedPaths.ContainsKey(colorId))
            {
                UndoStack.Push(new SignalSnapshot(CompletedPaths, MoveCount));
                CompletedPaths.Remove(colorId);
            }
        }

        /// <summary>
        /// 1수 되돌리기 (Undo)
        /// </summary>
        public bool Undo()
        {
            if (UndoStack.Count == 0 || !IsGameActive || IsVictory) return false;

            var snapshot = UndoStack.Pop();
            CompletedPaths.Clear();
            foreach (var kvp in snapshot.CompletedPaths)
            {
                CompletedPaths[kvp.Key] = new List<Point>(kvp.Value);
            }
            MoveCount = snapshot.MoveCount;

            return true;
        }

        /// <summary>
        /// 승리 여부 검증 (모든 쌍 연결 + 전 구역 100% 충진)
        /// </summary>
        public bool CheckVictory()
        {
            if (CompletedPaths.Count != PairCount) return false;

            int filledCount = 0;
            foreach (var path in CompletedPaths.Values)
                filledCount += path.Count;

            if (filledCount == GridSize * GridSize)
            {
                IsVictory = true;
                IsGameActive = false;

                try
                {
                    GateHelper.Dashboard.DashboardManager.RecordSignalLinkWin();
                }
                catch { }

                return true;
            }

            return false;
        }

        /// <summary>
        /// AI 힌트 요청 (아직 연결되지 않은 1개 색상의 최적 경로 반환)
        /// </summary>
        public int? RequestAiHint(out List<Point> hintPath)
        {
            hintPath = null;
            if (!IsGameActive || IsVictory) return null;

            // 아직 연결되지 않은 색상 우선 선택
            var unconnected = SolutionPaths.Keys.Where(k => !CompletedPaths.ContainsKey(k)).ToList();
            if (unconnected.Count == 0)
            {
                // 모두 연결되어 있으나 오답 경로인 경우 첫 번째 경로 제공
                unconnected = SolutionPaths.Keys.ToList();
            }

            if (unconnected.Count > 0)
            {
                int colorId = unconnected[_rand.Next(unconnected.Count)];
                hintPath = SolutionPaths[colorId];
                HintsUsed++;
                return colorId;
            }

            return null;
        }

        /// <summary>
        /// 3-Star 평가 산출 (이동 수, 힌트 수 기반)
        /// </summary>
        public int GetStarRating()
        {
            if (HintsUsed == 0 && MoveCount <= PairCount + 1) return 3;
            if (HintsUsed <= 1 && MoveCount <= PairCount + 4) return 2;
            return 1;
        }

        /// <summary>
        /// 현재 보드 채움율 (0% ~ 100%)
        /// </summary>
        public int GetCoveragePercentage()
        {
            int filled = 0;
            foreach (var p in CompletedPaths.Values) filled += p.Count;
            return (filled * 100) / (GridSize * GridSize);
        }
    }
}