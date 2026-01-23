using System.Collections.Generic;
using System.Drawing;

namespace GateHelper
{
    // 각 칸의 상태를 정의하는 열거형
    public enum NodeType { Empty, Node, Path }

    public class SignalNode
    {
        public Point Pos { get; set; }           // 좌표 (x, y)
        public NodeType Type { get; set; }       // 상태: 빈칸, 시작/끝점, 경로
        public int ColorID { get; set; }         // 신호 색상 번호 (0: 없음)

        public SignalNode(int x, int y)
        {
            Pos = new Point(x, y);
            Type = NodeType.Empty;
            ColorID = 0;
        }
    }
}