using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace GateHelper
{
    public class SignalManager
    {
        private int _size;
        public SignalNode[,] Grid { get; private set; }
        public int PairCount { get; private set; }
        public Dictionary<int, List<Point>> SolutionPaths { get; private set; } = new Dictionary<int, List<Point>>();

        public void InitializeGrid(int size, int pairCount)
        {
            this.PairCount = pairCount;
            _size = size;
            SolutionPaths.Clear();

            while (true)
            {
                if (TryGenerateValidPerfectMap(size, pairCount)) break;
            }
        }

        private bool TryGenerateValidPerfectMap(int size, int pairCount)
        {
            Grid = new SignalNode[size, size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    Grid[x, y] = new SignalNode(x, y);

            Random rand = new Random();
            HashSet<Point> occupied = new HashSet<Point>();
            List<List<Point>> allPaths = new List<List<Point>>();

            // 1. 초기 씨앗 배치
            for (int i = 0; i < pairCount; i++)
            {
                Point start;
                int safety = 0;
                do
                {
                    start = new Point(rand.Next(size), rand.Next(size));
                    if (++safety > 100) return false;
                } while (occupied.Contains(start));

                allPaths.Add(new List<Point> { start });
                occupied.Add(start);
            }

            // 2. 물리적으로 가능한 확장 (Snake Growth)
            bool moved = true;
            while (moved)
            {
                moved = false;
                var indices = Enumerable.Range(0, pairCount).OrderBy(x => rand.Next()).ToList();
                foreach (int idx in indices)
                {
                    var path = allPaths[idx];
                    Point[] currentEnds = { path.First(), path.Last() };

                    foreach (var end in currentEnds)
                    {
                        var neighbors = GetEmptyNeighbors(end, occupied);
                        if (neighbors.Count > 0)
                        {
                            Point next = neighbors[rand.Next(neighbors.Count)];
                            if (end == path.First()) path.Insert(0, next);
                            else path.Add(next);

                            occupied.Add(next);
                            moved = true;
                            break;
                        }
                    }
                }
            }

            // 3. [핵심] 스마트 리라우팅 (빈칸 흡수)
            // 막다른 길에 들어갔다 나오는 게 아니라, 기존 경로 (A-B) 사이에 빈칸(H)이 있다면 (A-H-B)로 우회시킴
            if (occupied.Count < size * size)
            {
                if (!SmartRerouteHoles(allPaths, occupied)) return false;
            }

            // 4. 최종 정답 저장 및 노드 배치
            for (int i = 0; i < pairCount; i++)
            {
                var p = allPaths[i];
                if (p.Count < 2) return false;

                int colorId = i + 1;
                SolutionPaths[colorId] = new List<Point>(p);
                Grid[p[0].X, p[0].Y].Type = NodeType.Node;
                Grid[p[0].X, p[0].Y].ColorID = colorId;
                Grid[p.Last().X, p.Last().Y].Type = NodeType.Node;
                Grid[p.Last().X, p.Last().Y].ColorID = colorId;
            }
            return true;
        }

        private bool SmartRerouteHoles(List<List<Point>> allPaths, HashSet<Point> occupied)
        {
            bool changed = true;
            while (changed)
            {
                changed = false;
                for (int y = 0; y < _size; y++)
                {
                    for (int x = 0; x < _size; x++)
                    {
                        Point hole = new Point(x, y);
                        if (occupied.Contains(hole)) continue;

                        // 빈칸 'hole'의 이웃들을 확인
                        var neighbors = GetAllNeighbors(hole);
                        foreach (var path in allPaths)
                        {
                            for (int i = 0; i < path.Count - 1; i++)
                            {
                                // 만약 경로 상의 인접한 두 점 A(path[i]), B(path[i+1])가 모두 'hole'의 이웃이라면
                                // A-B 관계를 끊고 A-hole-B로 우회시킴
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
            return occupied.Count == _size * _size; // 모든 칸이 채워졌는지 확인
        }

        private List<Point> GetEmptyNeighbors(Point p, HashSet<Point> occupied)
        {
            var res = new List<Point>();
            Point[] dirs = { new Point(0, 1), new Point(0, -1), new Point(1, 0), new Point(-1, 0) };
            foreach (var d in dirs)
            {
                Point n = new Point(p.X + d.X, p.Y + d.Y);
                if (n.X >= 0 && n.X < _size && n.Y >= 0 && n.Y < _size && !occupied.Contains(n)) res.Add(n);
            }
            return res;
        }

        private List<Point> GetAllNeighbors(Point p)
        {
            var res = new List<Point>();
            Point[] dirs = { new Point(0, 1), new Point(0, -1), new Point(1, 0), new Point(-1, 0) };
            foreach (var d in dirs)
            {
                Point n = new Point(p.X + d.X, p.Y + d.Y);
                if (n.X >= 0 && n.X < _size && n.Y >= 0 && n.Y < _size) res.Add(n);
            }
            return res;
        }
    }
}