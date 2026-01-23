using System;
using System.Collections.Generic;
using System.Drawing;

namespace GateHelper
{
    public class SignalManager
    {
        private int _size;
        public SignalNode[,] Grid { get; private set; }
        public int PairCount { get; private set; }


        public void InitializeGrid(int size, int pairCount)
        {
            this.PairCount = pairCount; // 저장
            _size = size;
            Grid = new SignalNode[size, size];

            // 1. 그리드 초기화
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Grid[x, y] = new SignalNode(x, y);
                }
            }

            // 2. 랜덤 노드(시작/끝점) 배치
            GeneratePairs(pairCount);
        }

        private void GeneratePairs(int pairCount)
        {
            Random rand = new Random();
            for (int i = 1; i <= pairCount; i++) // 색상 ID는 1부터 시작
            {
                PlaceRandomNode(i, rand); // 시작점
                PlaceRandomNode(i, rand); // 끝점
            }
        }

        private void PlaceRandomNode(int color, Random rand)
        {
            while (true)
            {
                int x = rand.Next(_size);
                int y = rand.Next(_size);
                if (Grid[x, y].Type == NodeType.Empty)
                {
                    Grid[x, y].Type = NodeType.Node;
                    Grid[x, y].ColorID = color;
                    break;
                }
            }
        }
    }
}