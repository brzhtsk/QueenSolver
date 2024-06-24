using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueenSolver
{
    public class Solver
    {
        private ChessBoard initialBoard;
        private int nodeCount;
        private const long MemoryLimit = 500 * 1024 * 1024;
        private const int MemoryCheckInterval = 1000;

        public Solver(ChessBoard board)
        {
            initialBoard = board;
            nodeCount = 0;
        }

        private bool IsMemoryLimitExceeded()
        {
            Process currentProcess = Process.GetCurrentProcess();
            long memoryUsage = currentProcess.PrivateMemorySize64;
            return memoryUsage > MemoryLimit;
        }

        public void ResetMemory()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        public (ChessBoard solution, int nodeCount) SolveBFS()
        {
            var queue = new Queue<ChessBoard>();
            queue.Enqueue(initialBoard);
            nodeCount = 1;

            while (queue.Count > 0)
            {
                if (nodeCount % MemoryCheckInterval == 0 && IsMemoryLimitExceeded())
                {
                    return (null, nodeCount);
                }

                var currentBoard = queue.Dequeue();

                if (currentBoard.IsGoal())
                {
                    return (currentBoard, nodeCount);
                }

                foreach (var nextBoard in currentBoard.GetSuccessors())
                {
                    nextBoard.ancestor = currentBoard;
                    queue.Enqueue(nextBoard);
                    nodeCount++;
                }
            }

            return (null, nodeCount);
        }

        public (ChessBoard solution, int nodeCount) SolveLDFS(int depthLimit)
        {
            var stack = new Stack<(ChessBoard board, int depth)>();
            stack.Push((initialBoard, 0));
            nodeCount = 0;

            while (stack.Count > 0)
            {
                var (currentBoard, currentDepth) = stack.Pop();
                nodeCount++;

                if (currentBoard.IsGoal())
                {
                    return (currentBoard, nodeCount);
                }

                if (currentDepth < depthLimit)
                {
                    foreach (var nextBoard in currentBoard.GetSuccessors())
                    {
                        nextBoard.ancestor = currentBoard;
                        stack.Push((nextBoard, currentDepth + 1));
                    }
                }
            }

            return (null, nodeCount);
        }

        public (ChessBoard solution, int nodeCount) SolveIDS()
        {
            int totalNodeCount = 0;
            int depth = 0;
            int maxDepth = 1000;
            while (depth <= maxDepth)
            {
                var (solution, nodeCount) = SolveDLS(depth);
                totalNodeCount += nodeCount;
                if (solution != null)
                {
                    return (solution, totalNodeCount);
                }
                depth++;
            }
            return (null,  totalNodeCount);
        }

        public (ChessBoard solution, int nodeCount) SolveDLS(int depthLimit)
        {
            nodeCount = 0;
            return DLS(initialBoard.Clone(), depthLimit);
        }

        private (ChessBoard solution, int nodeCount) DLS(ChessBoard board, int depthLimit)
        {
            if (depthLimit == 0 && board.IsGoal())
            {
                return (board, 1);
            }

            if (depthLimit == 0)
            {
                return (null, 1);
            }
            int currentNodeCount = 1;

            foreach (var nextBoard in board.GetSuccessors())
            {
                nextBoard.ancestor = board;
                var (solution, subNodeCount) = DLS(nextBoard, depthLimit - 1);
                currentNodeCount += subNodeCount;
                if (solution != null)
                {
                    return (solution, currentNodeCount);
                }
            }

            return (null, currentNodeCount);
        }
    }
}
