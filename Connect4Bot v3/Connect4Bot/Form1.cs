using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Connect4Bot
{
    public partial class Form1 : Form
    {
        readonly static int NUM_OF_COLUMNS = 7;
        readonly static int NUM_OF_ROWS = 6;
        readonly static int LAST_ROW_IND = NUM_OF_ROWS - 1;

        static int MAX_DEPTH = 9; // Maximum search depth
        static bool DO_STATIC_DEPTH = true;

        static double experimentalVariable;
        static double[] ROW_WORTH = new double[] {
            6.1, 4.1, 2.8, 2.0, 1.5, 1.2, 1.0
        }; // 0.2 * 1.6 ** x + 0.7

        readonly static int INFINITY = 100000000; // 100 000 000

        readonly static int CON_4_VAL = 10000; // 10 000
        readonly static int CON_3_VAL = 50;

        readonly static int BOT_COLOR = 1;
        readonly static int BOT_VALUE = CON_4_VAL;
        readonly static int FOE_COLOR = 2;
        readonly static int FOE_VALUE = -100 * CON_4_VAL;

        readonly static int[] COL_SEARCH_ORDER = { 
            3, 2, 4, 1, 5, 0, 6 
        };

        readonly static int[,] NUM_OF_POT_WINS = new int[,] {
            { 3,  4,  5,  5,  4,  3 },
            { 4,  6,  8,  8,  6,  4 },
            { 5,  8,  11, 11, 8,  5 },
            { 7,  10, 13, 13, 10, 7 },
            { 5,  8,  11, 11, 8,  5 },
            { 4,  6,  8,  8,  6,  4 },
            { 3,  4,  5,  5,  4,  3 }
        };

        public Form1()
        {
            InitializeComponent();
        }

        private void connectFourAdapter1_OnYourMove(Connect_Four_Adapter.YourMoveEventArgs e)
        {
            int[,] board = connectFourAdapter1.GetBoard();

            PrintBoard(board);

            int move = MainSearchAlgorithm(board);
            connectFourAdapter1.SendMove(move);
        }

        // Searches top-layer and returns the move
        static int MainSearchAlgorithm(int[,] board)
        {
            int row;                    // temp row
            int evalMaxIndex = -1;      // col-index to return
            int eval = -1 * INFINITY;   // evaluation value at col-index
            int alpha = -1 * INFINITY;  // MIN guaranteed value for BOT (botten kommer kunna få ett värde större än detta)
            int beta = INFINITY;        // MAX guaranteed value for FOE

            // For diagnostics
            int[] evalArr = new int[7];
            bool[] evalArrChanged = new bool[7];

            // TODO: Order moves acc to num of threats

            // TODO: Set minimal depth in begining and maximal in end.
            if (!DO_STATIC_DEPTH)
                MAX_DEPTH = FindOptimalDepth(board);

            System.Diagnostics.Debug.Write("MAX_DEPTH: ");
            System.Diagnostics.Debug.WriteLine(MAX_DEPTH);

            // Tests to put one in each column
            //for (int col = 0; col < 7; col++)
            foreach (int col in COL_SEARCH_ORDER)
            {
                row = ColToPos(col, board); // Grabs available y-coord
                if (row == -1)
                    continue;

                board[col, row] = BOT_COLOR; // Sets pos in board to BOT_COLOR
                int evalTemp = Minimax(board, MAX_DEPTH, alpha, beta, false); // Uses recursive minmax to evaluate pos
                board[col, row] = 0; // Resets pos

                evalArr[col] = evalTemp; // For diagnostics
                evalArrChanged[col] = true;

                if (evalTemp > eval)
                {
                    evalMaxIndex = col;
                    eval = evalTemp;
                }
            }

            for (int col = 0; col < 7; col++)
            {
                if (evalArrChanged[col])
                    System.Diagnostics.Debug.WriteLine("Column " + col + ": " + evalArr[col]);
                else
                    System.Diagnostics.Debug.WriteLine("Column " + col + ": None");
            }
            System.Diagnostics.Debug.WriteLine("");

            return evalMaxIndex;
        }

        // Finds optimal depth to search to not take to long.
        static int FindOptimalDepth(int[,] board)
        {
            double resDepth = 0;

            int temp = 0; // may be 1 with other met
            
            //int[] colArray = new int[NUM_OF_COLUMNS];
            for (int col = 0; col < NUM_OF_COLUMNS; col++)
            {
                for (int row = 0; row < NUM_OF_ROWS; row++)
                {
                    if (board[col, row] != 0)
                        break;
                    ++temp;
                    //++colArray[col];
                    //colArray[col] += NUM_OF_POT_WINS[col, row];
                }
                //resDepth += Math.Pow(experimentalVariable, colArray[col] - 2) - Math.Pow(experimentalVariable, 4) + 1; // 0.67 ** (x-2) + 1 - 0.67 ** 4
                //temp *= colArray[col];
            }

            if (temp < 16)
                resDepth = 16;
            else if (temp < 26)
                resDepth = 10;
            else if (temp < 36)
                resDepth = 9;
            else
                resDepth = 7;

            return Convert.ToInt32(resDepth); // ToInt32 rounds double to the nearest int
        }

        // Recursive function with alpha-beta till  evaluate
        static int Minimax(int[,] board, int depth, int alpha, int beta, bool playerMax)
        {
            if (depth == 0 || BoardIsFull(board) || GetWinner(board) != 0)
                return PartialEvaluation(board);

            int row;

            if (playerMax)
            {
                int eval = -1 * INFINITY;
                //for (int col = 0; col < 7; col++)
                foreach (int col in COL_SEARCH_ORDER)
                {
                    row = ColToPos(col, board);
                    if (row == -1)
                        continue;
                    board[col, row] = BOT_COLOR;
                    eval = Math.Max(eval, Minimax(board, depth - 1, alpha, beta, false));
                    board[col, row] = 0;
                    alpha = Math.Max(alpha, eval);
                    if (alpha >= beta)
                        break;
                }
                return eval;
            }
            else
            {
                int eval = INFINITY;
                //for (int col = 0; col < 7; col++)
                foreach (int col in COL_SEARCH_ORDER)
                {
                    row = ColToPos(col, board);
                    if (row == -1)
                        continue;
                    board[col, row] = FOE_COLOR;
                    int evalTemp = Minimax(board, depth - 1, alpha, beta, true);
                    board[col, row] = 0;
                    eval = Math.Min(evalTemp, eval);
                    beta = Math.Min(beta, eval);
                    if (alpha >= beta)
                        break;
                }
                return eval;
            }
        }

        // Approximates the value of a board with a positive
        // value beeing in favour of the bot and negative the foe.
        static int PartialEvaluation(int[,] board)
        {
            int res = 0;

            /*
            for (int col = 0; col < NUM_OF_COLUMNS; col++)
                for (int row = 0; row < NUM_OF_ROWS; row++)
                {
                    if (board[col, row] == BOT_COLOR)
                        res += NUM_OF_POT_WINS[col, row];
                    else if (board[col, row] != 0)
                        res -= NUM_OF_POT_WINS[col, row];
                }
            */

            res += ColorToWeightedScore(GetWinner(board));

            res += ValueOf3Connected(board) * CON_3_VAL;

            return res;
        }

        // Returns first instance of four-in-a-row.
        static int GetWinner(int[,] board)
        {
            // Kolla horrisontellt  
            for (int r = 0; r < NUM_OF_ROWS; r++)
                for (int c = 0; c < NUM_OF_COLUMNS - 3; c++)
                    if (board[c, r] != 0 && board[c, r] == board[c + 1, r] && board[c + 1, r] == board[c + 2, r] && board[c + 2, r] == board[c + 3, r])
                        return board[c, r];

            // Kolla vertikalt  
            for (int r = 0; r < NUM_OF_ROWS - 3; r++)
                for (int c = 0; c < NUM_OF_COLUMNS; c++)
                    if (board[c, r] != 0 && board[c, r] == board[c, r + 1] && board[c, r + 1] == board[c, r + 2] && board[c, r + 2] == board[c, r + 3])
                        return board[c, r];

            //Kolla diagonalt  
            for (int r = 0; r < NUM_OF_ROWS - 3; r++)
                for (int c = 0; c < NUM_OF_COLUMNS - 3; c++)
                {
                    if (board[c, r] != 0 && board[c, r] == board[c + 1, r + 1] && board[c + 1, r + 1] == board[c + 2, r + 2] && board[c + 2, r + 2] == board[c + 3, r + 3])
                        return board[c, r]; // upp  
                    if (board[c + 3, r] != 0 && board[c + 3, r] == board[c + 2, r + 1] && board[c + 2, r + 1] == board[c + 1, r + 2] && board[c + 1, r + 2] == board[c, r + 3])
                        return board[c + 3, r]; // ned  
                }

            return 0;
        }

        // Returns number of three-in-a-rows for bot minus foe.
        // Uses raw score.
        static int ValueOf3Connected(int[,] board)
        {
            int res = 0;

            // Check horizontally  
            for (int r = 0; r < NUM_OF_ROWS; r++)
                for (int c = 0; c < NUM_OF_COLUMNS - 2; c++)
                    if (board[c, r] != 0 && board[c, r] == board[c + 1, r] && board[c + 1, r] == board[c + 2, r])
                        res += ColorToRawScore(board[c, r]);

            // Check vertically  
            for (int r = 0; r < NUM_OF_ROWS - 2; r++)
                for (int c = 0; c < NUM_OF_COLUMNS; c++)
                    if (board[c, r] != 0 && board[c, r] == board[c, r + 1] && board[c, r + 1] == board[c, r + 2])
                        res += ColorToRawScore(board[c, r]);

            // Check diagonally 
            for (int r = 0; r < NUM_OF_ROWS - 2; r++)
                for (int c = 0; c < NUM_OF_COLUMNS - 2; c++)
                {
                    if (board[c, r] != 0 && board[c, r] == board[c + 1, r + 1] && board[c + 1, r + 1] == board[c + 2, r + 2])
                        res += ColorToRawScore(board[c, r]); // down  
                    if (board[c + 2, r] != 0 && board[c + 2, r] == board[c + 1, r + 1] && board[c + 1, r + 1] == board[c, r + 2])
                        res += ColorToRawScore(board[c + 2, r]); // up  
                }

            return res;
        }

        // Returns value of colour from the perspective of bot.
        // FOE's score is piroritized. 
        static int ColorToWeightedScore(int color)
        {
            if (color == BOT_COLOR)
                return BOT_VALUE;
            else if (color == FOE_COLOR)
                return FOE_VALUE;
            else
                return 0;
        }

        // Returns value of colour from the perspective of bot.
        // FOE's score is as important as the BOT's score. 
        static int ColorToRawScore(int color)
        {
            if (color == BOT_COLOR)
                return 1;
            else if (color == FOE_COLOR)
                return -1;
            else
                return 0;
        }

        // Looks up empty position in column from bottom up
        // and returns row number, or if full returns -1.
        static int ColToPos(int col, int[,] board)
        {
            for (int row = LAST_ROW_IND; row != -1; row--)
                if (board[col, row] == 0)
                    return row;
            return -1;
        }

        // Returns true there are no empty spots in top row.
        static bool BoardIsFull(int[,] board)
        {
            for (int col = 0; col < NUM_OF_COLUMNS; col++)
                if (board[col, 0] == 0)
                    return false;
            return true;
        }

        // Returns true there are only empty spots in bottom row.
        static bool BoardIsEmpty(int[,] board)
        {
            for (int col = 0; col < NUM_OF_COLUMNS; col++)
                if (board[col, LAST_ROW_IND] != 0)
                    return false;
            return true;
        }

        // Writes board to the debug console.
        static void PrintBoard(int[,] board)
        {
            for (int r = 0; r < NUM_OF_ROWS; r++)
            {
                for (int c = 0; c < NUM_OF_COLUMNS; c++)
                    System.Diagnostics.Debug.Write(board[c, r]);
                System.Diagnostics.Debug.WriteLine("");
            }

            System.Diagnostics.Debug.WriteLine("");
        }

        // Not in use.
        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            numericUpDown1.BackColor = Color.White;
            numericUpDown2.BackColor = Color.Gray;

            MAX_DEPTH = Convert.ToInt32(numericUpDown1.Value); //Must remove readonly from MAX_DEPTH 
            DO_STATIC_DEPTH = true;
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            numericUpDown1.BackColor = Color.Gray;
            numericUpDown2.BackColor = Color.White;

            experimentalVariable = Decimal.ToDouble(numericUpDown2.Value);
            DO_STATIC_DEPTH = false;
        }
    }
}
