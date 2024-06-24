using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueenSolver
{
    public class ChessBoard
    {
        private int size;
        private int[,] state;
        private List<int[]> queenLocations;
        private int[] queenRowCounter;
        private int[] queenColumnCounter;
        public ChessBoard ancestor;
        public ChessBoard child;

        public List<int[]> QueenLocations => this.queenLocations;

        public ChessBoard(int size)
        {
            this.size = size;
            this.state = new int[size, size];
            this.queenLocations = new List<int[]>(size);
            this.queenRowCounter = new int[size];
            this.queenColumnCounter = new int[size];
        }

        public void PlaceQueen(int col, int row)
        {
            this.state[row, col] = 1;
            this.queenLocations.Add(new int[] { row, col });
            this.queenRowCounter[row]++;
            this.queenColumnCounter[col]++;
        }

        public void RemoveQueen(int col, int row)
        {
            this.state[row, col] = 0;
            this.queenLocations.RemoveAll(q => q[0] == row && q[1] == col);
            this.queenRowCounter[row]--;
            this.queenColumnCounter[col]--;
        }

        public bool IsGoal()
        {
            return !HasConflicts();
        }

        public bool HasConflicts()
        {
            for (int i = 0; i < queenLocations.Count; i++)
            {
                for (int j = i + 1; j < queenLocations.Count; j++)
                {
                    if (InConflict(queenLocations[i], queenLocations[j]))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool InConflict(int[] q1, int[] q2)
        {
            int dx = Math.Abs(q1[0] - q2[0]);
            int dy = Math.Abs(q1[1] - q2[1]);
            return dx == 0 || dy == 0 || dx == dy;
        }

        public List<ChessBoard> GetSuccessors()
        {
            List<ChessBoard> successors = new List<ChessBoard>();
            int originCol = queenLocations[0][1];
            int originRow = queenLocations[0][0];
            RemoveQueen(originCol, originRow);

            for (int col = 0; col < size; col++)
            {
                if (state[originRow, col] == 0 && queenColumnCounter[col] == 0)
                {
                    ChessBoard newBoard = this.Clone();
                    newBoard.PlaceQueen(col, originRow);
                    successors.Add(newBoard);
                }
            }
            PlaceQueen(originCol, originRow);

            return successors;
        }

        public ChessBoard Clone()
        {
            ChessBoard cloneBoard = new ChessBoard(this.size);

            for (int row = 0; row < this.size; row++)
            {
                for (int col = 0; col < this.size; col++)
                {
                    cloneBoard.state[row, col] = this.state[row, col];
                }
            }

            foreach (int[] location in this.queenLocations)
            {
                cloneBoard.queenLocations.Add((int[])location.Clone());
            }

            return cloneBoard;
        }

        public void InitialProcessing()
        {
            Random rnd = new Random();
            int badRowCount = 0;
            for (int row = 0; row < this.size; row++)
            {
                if (this.queenRowCounter[row] > 1)
                {
                    badRowCount++;
                }
            }
            while (badRowCount > 0)
            {
                for (int row = 0; row < this.size; row++)
                {
                    if (this.queenRowCounter[row] > 1)
                    {
                        for (int col = 0; col < this.size; col++)
                        {
                            if (this.state[row, col] == 1)
                            {
                                this.RemoveQueen(col, row);
                                break;
                            }
                        }
                        if (this.queenRowCounter[row] <= 1)
                        {
                            badRowCount--;
                        }
                    }
                    if (this.queenRowCounter[row] == 0)
                    {
                        int col = rnd.Next(0, this.size);
                        this.PlaceQueen(col, row);
                    }
                }
            }
        }

        public (ChessBoard father, int nodeCount) getSolutionHistory()
        {
            ChessBoard iterator = this;
            ChessBoard directAncestor = null;
            int nodeCount = 1;
            while (iterator.ancestor != null)
            {
                iterator.ancestor.child = iterator;
                iterator = iterator.ancestor;
                directAncestor = iterator;
                nodeCount++;
            }
            if (directAncestor == null)
            {
                directAncestor = iterator;
            }
            return (directAncestor, nodeCount);
        }
    }
}
