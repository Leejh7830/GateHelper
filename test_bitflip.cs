using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace GateHelper.Tests
{
    class Program
    {
        static int _pass = 0;
        static int _fail = 0;

        static void Assert(bool condition, string testName)
        {
            if (condition)
            {
                Console.WriteLine($"[PASS] {testName}");
                _pass++;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[FAIL] {testName}");
                Console.ResetColor();
                _fail++;
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("========================================");
            Console.WriteLine("       BITFLIP REVAMP TEST SUITE        ");
            Console.WriteLine("========================================");

            Test_Difficulty_Initializations();
            Test_Guaranteed_Solvable_Puzzles();
            Test_Undo_Stack();
            Test_Diagonal_Gimmick();
            Test_Line_Laser_Gimmick();
            Test_Firewall_Gimmick();
            Test_Linked_Quantum_Gimmick();
            Test_Key_And_Locked_Gate_Gimmick();
            Test_Ai_Hint_And_Star_Rating();

            Console.WriteLine("========================================");
            Console.WriteLine($"TOTAL: {_pass + _fail} | PASS: {_pass} | FAIL: {_fail}");
            Console.WriteLine("========================================");

            if (_fail > 0) Environment.Exit(1);
        }

        static void Test_Difficulty_Initializations()
        {
            var model = new BitFlipModel();
            Assert(model.GridSize == 4 && model.Difficulty == "Normal", "Default initialization is 4x4 Normal");

            model.InitializeGame("Easy");
            Assert(model.GridSize == 3 && model.Difficulty == "Easy", "Easy initialization is 3x3");

            model.InitializeGame("Hard");
            Assert(model.GridSize == 5 && model.Difficulty == "Hard", "Hard initialization is 5x5");

            model.InitializeGame("Expert");
            Assert(model.GridSize == 6 && model.Difficulty == "Expert", "Expert initialization is 6x6");
        }

        static void Test_Guaranteed_Solvable_Puzzles()
        {
            string[] diffs = { "Easy", "Normal", "Hard", "Expert" };
            foreach (var diff in diffs)
            {
                var model = new BitFlipModel();
                model.InitializeGame(diff);

                var solution = model.SolveGF2();
                Assert(solution != null && solution.Count > 0, $"Difficulty '{diff}' generated guaranteed solvable puzzle (Par: {model.ParMoves})");

                // 시뮬레이션: 도출된 최적 해를 클릭하여 실제로 100% 승리 상태에 도달하는지 검증
                foreach (var pt in solution)
                {
                    model.ClickNode(pt.X, pt.Y, out _, out _);
                }
                Assert(model.IsVictory, $"Difficulty '{diff}' solution simulation achieves IsVictory == true");
            }
        }

        static void Test_Undo_Stack()
        {
            var model = new BitFlipModel();
            model.InitializeGame("Easy");

            bool initialC0R0 = model.Grid[0, 0].State;
            int initialMoves = model.MoveCount;

            model.ClickNode(0, 0, out _, out _);
            Assert(model.MoveCount == initialMoves + 1, "Clicking increments MoveCount");
            Assert(model.UndoStack.Count == 1, "Clicking pushes state to UndoStack");

            bool undone = model.Undo();
            Assert(undone, "Undo succeeds");
            Assert(model.MoveCount == initialMoves, "Undo restores MoveCount");
            Assert(model.Grid[0, 0].State == initialC0R0, "Undo restores original cell state");
            Assert(model.UndoStack.Count == 0, "Undo stack is now empty");

            bool emptyUndo = model.Undo();
            Assert(!emptyUndo, "Undo on empty stack safely returns false");
        }

        static void Test_Diagonal_Gimmick()
        {
            var model = new BitFlipModel();
            model.InitializeGame("Normal");

            // 1,1 노드가 대각선(Diagonal) 타입인지 확인
            var dNode = model.Grid[1, 1];
            Assert(dNode.Type == BitNodeType.Diagonal, "Normal mode has Diagonal node at (1,1)");

            var affected = model.GetAffectedNodes(1, 1);
            // 대각선 4방향: (0,0), (2,0), (0,2), (2,2) + 중심 (1,1) = 총 5개
            Assert(affected.Count == 5, "Diagonal node affects exactly 5 nodes (self + 4 diagonals)");
            Assert(affected.Any(n => n.X == 0 && n.Y == 0), "Diagonal affects (0,0)");
            Assert(affected.Any(n => n.X == 2 && n.Y == 0), "Diagonal affects (2,0)");
            Assert(affected.Any(n => n.X == 0 && n.Y == 2), "Diagonal affects (0,2)");
            Assert(affected.Any(n => n.X == 2 && n.Y == 2), "Diagonal affects (2,2)");
            Assert(!affected.Any(n => n.X == 1 && n.Y == 0), "Diagonal does NOT affect orthogonal (1,0)");
        }

        static void Test_Line_Laser_Gimmick()
        {
            var model = new BitFlipModel();
            model.InitializeGame("Hard");

            // Hard 모드의 중앙 노드는 Line 타입
            int mid = model.GridSize / 2;
            var lineNode = model.Grid[mid, mid];
            Assert(lineNode.Type == BitNodeType.Line, "Hard mode has Line Laser node at center");

            var affected = model.GetAffectedNodes(mid, mid);
            // 가로 1행 전체 5개 노드가 모두 영향 범위에 포함되어야 함
            Assert(affected.Count == model.GridSize, $"Line Laser affects entire row ({model.GridSize} nodes)");
            for (int c = 0; c < model.GridSize; c++)
            {
                Assert(affected.Any(n => n.X == c && n.Y == mid), $"Line Laser affects (col: {c}, row: {mid})");
            }
        }

        static void Test_Firewall_Gimmick()
        {
            var model = new BitFlipModel();
            model.InitializeGame("Hard");

            // Hard 모드의 (0, 4)는 Firewall 노드
            var fwNode = model.Grid[0, model.GridSize - 1];
            Assert(fwNode.Type == BitNodeType.Firewall, "Hard mode has Firewall node at (0, 4)");

            // 인접 노드 (0, 3)를 클릭했을 때 Firewall 노드가 면역되는지 검증
            var affected = model.GetAffectedNodes(0, model.GridSize - 2);
            Assert(!affected.Contains(fwNode), "Neighbor click does NOT affect Firewall node (Immunity)");

            // Firewall 노드를 직접 클릭했을 때는 정상적으로 자신이 반전되는지 검증
            var directAffected = model.GetAffectedNodes(fwNode.X, fwNode.Y);
            Assert(directAffected.Contains(fwNode), "Direct click on Firewall node affects itself");
        }

        static void Test_Linked_Quantum_Gimmick()
        {
            var model = new BitFlipModel();
            model.InitializeGame("Expert");

            // Expert는 (0,1)과 (5,4)가 양자 얽힘 노드
            var l1 = model.Grid[0, 1];
            var l2 = model.Grid[model.GridSize - 1, model.GridSize - 2];
            Assert(l1.Type == BitNodeType.Linked && l2.Type == BitNodeType.Linked, "Expert mode has Linked nodes");
            Assert(l1.LinkId == l2.LinkId, "Linked nodes share same LinkId");

            var affected = model.GetAffectedNodes(l1.X, l1.Y);
            Assert(affected.Contains(l1), "Linked click affects self");
            Assert(affected.Contains(l2), "Linked click simultaneously affects partner node");
        }

        static void Test_Key_And_Locked_Gate_Gimmick()
        {
            var model = new BitFlipModel();
            model.InitializeGame("Expert");

            BitNode keyNode = null;
            BitNode lockedNode = null;
            for (int r = 0; r < model.GridSize; r++)
                for (int c = 0; c < model.GridSize; c++)
                {
                    if (model.Grid[c, r].Type == BitNodeType.Key) keyNode = model.Grid[c, r];
                    if (model.Grid[c, r].Type == BitNodeType.Locked) lockedNode = model.Grid[c, r];
                }

            Assert(keyNode != null && lockedNode != null, "Expert mode has Key and Locked nodes");
            lockedNode.IsLocked = true;

            // 잠긴 노드 직접 클릭 거부 확인
            bool clickLocked = model.ClickNode(lockedNode.X, lockedNode.Y, out _, out _);
            Assert(!clickLocked, "Clicking locked node is rejected");

            // 키 노드를 켜짐(true)으로 반전
            keyNode.State = false;
            model.ClickNode(keyNode.X, keyNode.Y, out _, out bool unlocked);
            Assert(keyNode.State == true, "Key node is now ON");
            Assert(!lockedNode.IsLocked, "Key node activation unlocked the gate");
            Assert(unlocked, "Unlocked flag was returned true");

            // 해제 후 정상 클릭 가능 확인
            bool clickUnlocked = model.ClickNode(lockedNode.X, lockedNode.Y, out _, out _);
            Assert(clickUnlocked, "Now unlocked node can be clicked normally");
        }

        static void Test_Ai_Hint_And_Star_Rating()
        {
            var model = new BitFlipModel();
            model.InitializeGame("Normal");

            var hint = model.RequestAiHint();
            Assert(hint.HasValue, "RequestAiHint returns a valid next move point");
            Assert(model.CurrentHintNode.HasValue && model.CurrentHintNode.Value == hint.Value, "CurrentHintNode is updated");
            Assert(model.HintsUsed == 1, "HintsUsed counter is incremented");

            // Star Rating
            Assert(model.GetStarRating() == 3, "0 moves <= Par gives 3 stars");
        }
    }
}
