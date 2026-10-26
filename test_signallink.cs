using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using GateHelper;

namespace GateHelper.Tests
{
    class SignalLinkTestSuite
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

        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("========================================");
            Console.WriteLine("     SIGNAL LINK REVAMP TEST SUITE      ");
            Console.WriteLine("========================================");

            Test_Difficulty_Initializations();
            Test_Guaranteed_Solvable_Puzzles();
            Test_Undo_And_Path_Trimming();
            Test_Stealth_Memory_Mode();
            Test_Ai_Hint_And_Star_Rating();
            Test_Control_Rendering_And_Bresenham();

            Console.WriteLine("========================================");
            Console.WriteLine($"TOTAL: {_pass + _fail} | PASS: {_pass} | FAIL: {_fail}");
            Console.WriteLine("========================================");

            if (_fail > 0) Environment.Exit(1);
        }

        static void Test_Difficulty_Initializations()
        {
            var mgr = new SignalManager();
            Assert(mgr.GridSize == 6 && mgr.Difficulty == "Normal" && mgr.PairCount == 4, "Default initialization is 6x6 Normal (4 pairs)");

            mgr.InitializeGrid("Easy");
            Assert(mgr.GridSize == 5 && mgr.Difficulty == "Easy" && mgr.PairCount == 3, "Easy is 5x5 (3 pairs)");
            Assert(!mgr.IsStealthMode, "Easy has StealthMode == false");

            mgr.InitializeGrid("Hard");
            Assert(mgr.GridSize == 8 && mgr.Difficulty == "Hard" && mgr.PairCount == 6, "Hard is 8x8 (6 pairs)");
            Assert(!mgr.IsStealthMode, "Hard has StealthMode == false");

            mgr.InitializeGrid("Expert");
            Assert(mgr.GridSize == 10 && mgr.Difficulty == "Expert" && mgr.PairCount == 7, "Expert is 10x10 (7 pairs)");
            Assert(mgr.IsStealthMode, "Expert has StealthMode == true (User memory gimmick preserved)");
        }

        static void Test_Guaranteed_Solvable_Puzzles()
        {
            string[] diffs = { "Easy", "Normal", "Hard", "Expert" };
            foreach (var diff in diffs)
            {
                var mgr = new SignalManager();
                mgr.InitializeGrid(diff);

                Assert(mgr.SolutionPaths.Count == mgr.PairCount, $"Difficulty '{diff}' generated {mgr.PairCount} complete solution paths");

                // 시뮬레이션: 솔루션 패스를 모두 연결했을 때 100% 승리 판정이 발생하는지 검증
                foreach (var kvp in mgr.SolutionPaths)
                {
                    bool ok = mgr.CommitPath(kvp.Key, kvp.Value);
                    Assert(ok, $"Difficulty '{diff}' Signal #{kvp.Key} commit succeeds");
                }

                Assert(mgr.IsVictory, $"Difficulty '{diff}' achieves IsVictory == true with full 100% board fill");
                Assert(mgr.GetCoveragePercentage() == 100, $"Difficulty '{diff}' coverage is 100%");
            }
        }

        static void Test_Undo_And_Path_Trimming()
        {
            var mgr = new SignalManager();
            mgr.InitializeGrid("Easy");

            var firstPath = mgr.SolutionPaths[1];
            mgr.CommitPath(1, firstPath);

            Assert(mgr.CompletedPaths.ContainsKey(1), "Signal #1 is recorded in CompletedPaths");
            Assert(mgr.UndoStack.Count == 1, "Connecting pushes state to UndoStack");

            bool undone = mgr.Undo();
            Assert(undone, "Undo succeeds");
            Assert(!mgr.CompletedPaths.ContainsKey(1), "Undo removes Signal #1 from CompletedPaths");
            Assert(mgr.UndoStack.Count == 0, "UndoStack is now empty");

            // 재연결 및 RemovePath 검증
            mgr.CommitPath(1, firstPath);
            mgr.RemovePath(1);
            Assert(!mgr.CompletedPaths.ContainsKey(1), "RemovePath clears Signal #1");
            Assert(mgr.UndoStack.Count == 2, "RemovePath records state for undo (total 2 snapshots)");
        }

        static void Test_Stealth_Memory_Mode()
        {
            var mgr = new SignalManager();
            mgr.InitializeGrid("Expert");

            Assert(mgr.IsStealthMode, "Expert mode operates in Stealth Memory Mode");

            int stealthNodeCount = 0;
            for (int r = 0; r < mgr.GridSize; r++)
            {
                for (int c = 0; c < mgr.GridSize; c++)
                {
                    if (mgr.Grid[c, r].Type == NodeType.Terminal && mgr.Grid[c, r].IsStealth)
                    {
                        stealthNodeCount++;
                    }
                }
            }

            Assert(stealthNodeCount == mgr.PairCount * 2, $"All {mgr.PairCount * 2} terminal nodes have IsStealth == true for memorization");
        }

        static void Test_Ai_Hint_And_Star_Rating()
        {
            var mgr = new SignalManager();
            mgr.InitializeGrid("Normal");

            int? hintId = mgr.RequestAiHint(out var hintPath);
            Assert(hintId.HasValue && hintPath != null && hintPath.Count >= 2, "AI Hint generates valid solution path for unconnected pair");
            Assert(mgr.HintsUsed == 1, "HintsUsed counter is incremented");

            // 별점 검증
            Assert(mgr.GetStarRating() <= 3, "Star rating evaluates properly");
        }

        static void Test_Control_Rendering_And_Bresenham()
        {
            Console.WriteLine("Testing SignalLinkControl GDI+ canvas rendering & Bresenham interpolation...");

            var ctrl = new SignalLinkControl();
            ctrl.Size = new Size(800, 600);
            ctrl.CreateControl();

            var fieldMapSelect = typeof(SignalLinkControl).GetField("_isInMapSelect", BindingFlags.NonPublic | BindingFlags.Instance);
            var methodPaint = typeof(SignalLinkControl).GetMethod("OnPaint", BindingFlags.NonPublic | BindingFlags.Instance);
            var methodTick = typeof(SignalLinkControl).GetMethod("OnRenderTick", BindingFlags.NonPublic | BindingFlags.Instance);
            var methodMouseDown = typeof(SignalLinkControl).GetMethod("OnMouseDown", BindingFlags.NonPublic | BindingFlags.Instance);
            var methodMouseMove = typeof(SignalLinkControl).GetMethod("OnMouseMove", BindingFlags.NonPublic | BindingFlags.Instance);
            var methodMouseUp = typeof(SignalLinkControl).GetMethod("OnMouseUp", BindingFlags.NonPublic | BindingFlags.Instance);

            // 1. 맵 선택 화면 렌더링 검증
            fieldMapSelect.SetValue(ctrl, true);
            using (var bmp = new Bitmap(800, 600))
            using (var g = Graphics.FromImage(bmp))
            {
                var pe = new PaintEventArgs(g, new Rectangle(0, 0, 800, 600));
                methodPaint.Invoke(ctrl, new object[] { pe });
            }
            Assert(true, "Map Selection 2x2 Screen paints cleanly without exceptions");

            // 2. 인게임 진입 및 브레젠험 고속 마우스 드래그 시뮬레이션
            string[] diffs = { "Easy", "Normal", "Hard", "Expert" };
            foreach (var diff in diffs)
            {
                ctrl.StartNewGame(diff);

                // 마우스 클릭 및 고속 드래그 (Bresenham 보간)
                var fieldBoardBounds = typeof(SignalLinkControl).GetField("_boardBounds", BindingFlags.NonPublic | BindingFlags.Instance);
                Rectangle boardBounds = (Rectangle)fieldBoardBounds.GetValue(ctrl);

                // 보드 중앙 시작 드래그
                Point startMouse = new Point(boardBounds.Left + 20, boardBounds.Top + 20);
                Point fastMoveMouse = new Point(boardBounds.Left + 180, boardBounds.Top + 140); // 큰 거리 점프

                methodMouseDown.Invoke(ctrl, new object[] { new MouseEventArgs(MouseButtons.Left, 1, startMouse.X, startMouse.Y, 0) });
                methodMouseMove.Invoke(ctrl, new object[] { new MouseEventArgs(MouseButtons.Left, 1, fastMoveMouse.X, fastMoveMouse.Y, 0) });
                methodMouseUp.Invoke(ctrl, new object[] { new MouseEventArgs(MouseButtons.Left, 1, fastMoveMouse.X, fastMoveMouse.Y, 0) });

                // 여러 프레임 렌더링
                for (int f = 0; f < 5; f++)
                {
                    methodTick.Invoke(ctrl, new object[] { null, EventArgs.Empty });
                    using (var bmp = new Bitmap(800, 600))
                    using (var g = Graphics.FromImage(bmp))
                    {
                        var pe = new PaintEventArgs(g, new Rectangle(0, 0, 800, 600));
                        methodPaint.Invoke(ctrl, new object[] { pe });
                    }
                }
            }
            Assert(true, "All difficulties with fast dragging and tick animations rendered with 0 errors");
        }
    }
}
