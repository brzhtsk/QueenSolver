using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace QueenSolver
{
    public partial class MainForm : Form
    {
        private const int BoardSize = 8;
        private PictureBox[,] chessBoard = new PictureBox[BoardSize, BoardSize];
        private Image queenImage;
        private ChessBoard board;
        private Solver solver;
        private int queenCount = 0;
        private int totalNodeCount = 0;
        private int currentNodeCount = 1;
        private bool isBoardLocked = false;
        public MainForm()
        {
            InitializeComponent();
            LoadQueenImage();
            InitializeChessBoard();
            board = new ChessBoard(BoardSize);
            ButtonsInitialAvailability();
        }

        private ChessBoard currentState = null;
        private ChessBoard greatAncestor = null;
        private ChessBoard initialBoard = null;

        private void LoadQueenImage()
        {
            queenImage = Image.FromFile("queen.png");
        }

        private void ButtonsInitialAvailability()
        {
            SetButtonState(initialGenerationButton, true);
            SetButtonState(ldfsSolverButton, false);
            SetButtonState(bfsSolverButton, false);
            SetButtonState(idsSolverButton, false);
            SetButtonState(fileWritingButton, false);
            SetButtonState(stepByStepViewingButton, false);
            SetButtonState(viewPreviousButton, false);
            SetButtonState(viewNextButton, false);
            SetButtonState(startOverButton, true);
        }

        private void SetButtonState(Button button, bool isEnabled)
        {
            button.Enabled = isEnabled;
            button.BackColor = isEnabled ? Color.White : Color.WhiteSmoke;
            button.ForeColor = isEnabled ? Color.Black : Color.Gray;
        }

        private void ChosenButtonState(Button button)
        {
            button.Enabled = false;
            button.BackColor = Color.Silver;
            button.ForeColor = Color.Black;
        }

        private void PictureBox_Click(object sender, EventArgs e)
        {
            if (isBoardLocked)
            {
                MessageBox.Show("Для нового рішення почніть спочатку");
                return;
            }
            PictureBox pictureBox = sender as PictureBox;
            int col = pictureBox.Location.Y / pictureBox.Height;
            int row = pictureBox.Location.X / pictureBox.Width;

            if (pictureBox.Image == null)
            {
                if (queenCount >= 8)
                {
                    MessageBox.Show("Не можна встановити більше 8 ферзів");
                    return;
                }
                pictureBox.Image = queenImage;
                board.PlaceQueen(col, row);
                queenCount++;
            }
            else
            {
                pictureBox.Image = null;
                board.RemoveQueen(col, row);
                queenCount--;
            }

            if (queenCount == 8)
            {
                SetButtonState(ldfsSolverButton, true);
                SetButtonState(bfsSolverButton, true);
                SetButtonState(idsSolverButton, true);
            }
            else
            {
                SetButtonState(ldfsSolverButton, false);
                SetButtonState(bfsSolverButton, false);
                SetButtonState(idsSolverButton, false);
            }
        }

        private void InitializeChessBoard()
        {
            int tileSize = chessboardPanel.Width / BoardSize;
            chessboardPanel.Height = chessboardPanel.Width;
            chessboardPanel.Controls.Clear();

            for (int i = 0; i < BoardSize; i++)
            {
                for (int j = 0; j < BoardSize; j++)
                {
                    chessBoard[i, j] = new PictureBox
                    {
                        Width = tileSize,
                        Height = tileSize,
                        Location = new Point(i * tileSize, j * tileSize),
                        BackColor = (i + j) % 2 == 0 ? Color.White : Color.Gray,
                        BorderStyle = BorderStyle.FixedSingle,
                        SizeMode = PictureBoxSizeMode.StretchImage
                    };
                    chessBoard[i, j].Click += new EventHandler(PictureBox_Click);
                    chessboardPanel.Controls.Add(chessBoard[i, j]);
                }
            }
        }

        private void initialGenerationButton_Click(object sender, EventArgs e)
        {
            Random rand = new Random();
            board = new ChessBoard(BoardSize);
            currentState = null;
            greatAncestor = null;
            queenCount = 0;

            for (int i = 0; i < BoardSize; i++)
            {
                for (int j = 0; j < BoardSize; j++)
                {
                    chessBoard[i, j].Image = null;
                }
            }

            for (int i = 0; i < BoardSize; i++)
            {
                int row = rand.Next(BoardSize);
                chessBoard[row, i].Image = queenImage;
                board.PlaceQueen(i, row);
                queenCount++;
            }
            if (queenCount == 8)
            {
                SetButtonState(ldfsSolverButton, true);
                SetButtonState(bfsSolverButton, true);
                SetButtonState(idsSolverButton, true);
            }
            else
            {
                SetButtonState(ldfsSolverButton, false);
                SetButtonState(bfsSolverButton, false);
                SetButtonState(idsSolverButton, false);
            }
        }

        private void ProcessInitialBoard()
        {
            initialBoard = board.Clone();
            board.InitialProcessing();
        }

        private void SolveAndDisplay(Func<(ChessBoard, int)> solveMethod)
        {
            ProcessInitialBoard();
            board.ancestor = null;
            solver = new Solver(board.Clone());
            var (solution, nodeCount) = solveMethod();
            if (solution != null)
            {
                isBoardLocked = true;
                currentState = solution;
                greatAncestor = null;
                DisplaySolution(solution);
                practicalComplexityLabel.Text = $"Практична складність: {nodeCount}";
                SetButtonState(fileWritingButton, true);
                SetButtonState(stepByStepViewingButton, true);
                solver.ResetMemory();
            }
            else
            {
                solver.ResetMemory();
                MessageBox.Show("Рішення не знайдено");
                practicalComplexityLabel.Text = "Практична складність: -";
            }
            SetButtonState(initialGenerationButton, false);
        }

        private void bfsSolverButton_Click(object sender, EventArgs e)
        {
            ChosenButtonState(bfsSolverButton);
            SetButtonState(ldfsSolverButton, false);
            SetButtonState(idsSolverButton, false);
            SolveAndDisplay(() => solver.SolveBFS());
        }

        private void ldfsSolverButton_Click(object sender, EventArgs e)
        {
            ChosenButtonState(ldfsSolverButton);
            SetButtonState(bfsSolverButton, false);
            SetButtonState(idsSolverButton, false);
            int depthLimit = 10;
            SolveAndDisplay(() => solver.SolveLDFS(depthLimit));
        }

        private void idsSolverButton_Click(object sender, EventArgs e)
        {
            ChosenButtonState(idsSolverButton);
            SetButtonState(ldfsSolverButton, false);
            SetButtonState(bfsSolverButton, false);
            SolveAndDisplay(() => solver.SolveIDS());
        }

        private void DisplaySolution(ChessBoard solution)
        {
            for (int i = 0; i < BoardSize; i++)
            {
                for (int j = 0; j < BoardSize; j++)
                {
                    chessBoard[i, j].Image = null;
                }
            }

            foreach (var queen in solution.QueenLocations)
            {
                chessBoard[queen[0], queen[1]].Image = queenImage;
            }
        }

        private void fileWritingButton_Click(object sender, EventArgs e)
        {
            if (currentState != null)
            {
                var (father, nodeCount) = currentState.getSolutionHistory();
                greatAncestor = father;

                if (!BoardsAreEqual(initialBoard, greatAncestor) && initialBoard.child == null && greatAncestor.ancestor != initialBoard)
                {
                    initialBoard.child = greatAncestor;
                    greatAncestor.ancestor = initialBoard;
                }
                SaveSolutionToFile(initialBoard);
            }
            else
            {
                MessageBox.Show("Немає рішення для збереження");
                SetButtonState(fileWritingButton, false);
            }
        }

        private void SaveSolutionToFile(ChessBoard solution)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                saveFileDialog.Title = "Save solution to file";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = saveFileDialog.FileName;

                    using (StreamWriter writer = new StreamWriter(filePath))
                    {
                        ChessBoard current = solution;
                        while (current != null)
                        {
                            writer.WriteLine(BoardToString(current));
                            writer.WriteLine();
                            current = current.child;
                        }
                    }
                }
            }
        }

        private string BoardToString(ChessBoard board)
        {
            StringBuilder sb = new StringBuilder();
            for (int col = 0; col < BoardSize; col++)
            {
                for (int row = 0; row < BoardSize; row++)
                {
                    sb.Append(board.QueenLocations.Exists(q => q[0] == row && q[1] == col) ? " Q " : " . ");
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private void stepByStepViewingButton_Click(object sender, EventArgs e)
        {
            if (currentState != null)
            {
                var (father, nodeCount) = currentState.getSolutionHistory();
                greatAncestor = father;
                totalNodeCount = nodeCount + 1;

                if (!BoardsAreEqual(initialBoard, greatAncestor) && initialBoard.child == null && greatAncestor.ancestor != initialBoard)
                {
                    initialBoard.child = greatAncestor;
                    greatAncestor.ancestor = initialBoard;
                }
                DisplaySolution(initialBoard);
                currentState = initialBoard;

                if (BoardsAreEqual(initialBoard, greatAncestor)) { totalNodeCount -= 1; }
                nodeCountLabel.Text = $"{currentNodeCount} з {totalNodeCount}";
                if (greatAncestor.child != null)
                {
                    SetButtonState(viewNextButton, true);
                }
            }
            else
            {
                MessageBox.Show("Немає рішення для перегляду");
                SetButtonState(viewNextButton, false);
            }

            SetButtonState(stepByStepViewingButton, false);
            SetButtonState(viewPreviousButton, false);
            ChosenButtonState(stepByStepViewingButton);
        }

        private void viewPreviousButton_Click(object sender, EventArgs e)
        {
            if (currentState != null)
            {
                if (currentState.ancestor == null)
                {
                    SetButtonState(viewPreviousButton, false);
                    return;
                }
                DisplaySolution(currentState.ancestor);
                currentState = currentState.ancestor;
                currentNodeCount -= 1;
                if (currentState.ancestor == null)
                    SetButtonState(viewPreviousButton, false);
                nodeCountLabel.Text = $"{currentNodeCount} з {totalNodeCount}";
                SetButtonState(viewNextButton, true);
            }
            else
            {
                MessageBox.Show("Помилка перегляду");
            }
        }

        private void viewNextButton_Click(object sender, EventArgs e)
        {
            if (currentState != null)
            {
                if (currentState.child == null)
                {
                    SetButtonState(viewNextButton, false);
                    return;
                }
                DisplaySolution(currentState.child);
                currentState = currentState.child;
                currentNodeCount += 1;
                if (currentState.child == null)
                    SetButtonState(viewNextButton, false);
                nodeCountLabel.Text = $"{currentNodeCount} з {totalNodeCount}";
                SetButtonState(viewPreviousButton, true);
            }
            else
            {
                MessageBox.Show("Помилка перегляду");
            }
        }

        private void startOverButton_Click(object sender, EventArgs e)
        {
            board = new ChessBoard(BoardSize);
            currentState = null;
            greatAncestor = null;
            queenCount = 0;
            currentNodeCount = 1;
            isBoardLocked = false;
            practicalComplexityLabel.Text = "Практична складність: ";
            nodeCountLabel.Text = " ";
            ButtonsInitialAvailability();
            for (int i = 0; i < BoardSize; i++)
            {
                for (int j = 0; j < BoardSize; j++)
                {
                    chessBoard[i, j].Image = null;
                }
            }
        }

        private bool BoardsAreEqual(ChessBoard board1, ChessBoard board2)
        {
            if (board1.QueenLocations.Count != board2.QueenLocations.Count)
                return false;

            for (int i = 0; i < board1.QueenLocations.Count; i++)
            {
                if (board1.QueenLocations[i][0] != board2.QueenLocations[i][0] || board1.QueenLocations[i][1] != board2.QueenLocations[i][1])
                    return false;
            }
            return true;
        }
    }
}