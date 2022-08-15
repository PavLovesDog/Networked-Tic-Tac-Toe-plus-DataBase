using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace NDS_Networking_Project
{
    public enum TileType
    {
        Blank,
        Cross,
        Naught
    }

    public enum GameState
    {
        Playing,
        Draw,
        CrossWins,
        NaughtWins
    }

    public class TicTacToe
    {
        //TODO have server dictate whose turn it is and which tile types all clients are (blank for none players)
        public TileType playerTileType;
        public List<Button> buttons = new List<Button>(); // assuming 9 buttons for game grid
        public TileType[] grid = new TileType[9]; // represents how board looks (i.e state of gameboard)

        //as sending strings is easier Server needs to tell clients what the state of the gameboard looks like
        public string GridToString()
        {
            string s = "";
            //convert grid array to string perhaps someeitng Like this e.g "-xo--o-x-" (0,1,2,3,4,5,6,7,8)
            for(int i = 0; i < grid.Length; i++)
            {
                if(grid[i] == TileType.Cross)
                {
                    //s += grid[i].ToString(); //?
                    s += "x"; 
                }
                else if(grid[i] == TileType.Naught)
                {
                    //s += grid[i].ToString();
                    s += "o";
                }
                else if(grid[i] == TileType.Blank)
                {
                    //s += grid[i].ToString();
                    s += "-";
                }
            }

            return s;
        }

        //convert grid string to grid array
        public void StringToGrid(string s)
        {
            //interperet string and update grid array
            char[] cells = s.ToCharArray();
            for (int i = 0; i < grid.Length; i++)
            {
                if(cells[i] == 'x')
                {
                    grid[i] = TileType.Cross; 
                }
                else if (cells[i] == 'o')
                {
                    grid[i] = TileType.Naught;
                }
                else if (cells[i] == '-')
                {
                    grid[i] = TileType.Blank;
                }
            }
        }

        // helper function that takes in a tile index and the type we wish to change it to
        public bool SetTile(int index, TileType tileType)
        {
            if(grid[index] == TileType.Blank)
            {
                //update tile in grid
                grid[index] = tileType;

                //also update button representing this grid cell
                if(buttons.Count >= 9)
                {
                    buttons[index].Text = TileTypeToString(tileType);
                }

                return true;
            }

            //not free to place this tile here. not a valid move
            return false;
        }

        // converts a tiletype passed in into a string for button text to read/display
        public static string TileTypeToString(TileType tileType)
        {
            if (tileType == TileType.Blank)
                return "";
            else if (tileType == TileType.Cross)
                return "X";
            else
                return "O";
        }

        //Function to track game state
        public GameState GetGameState()
        {
            GameState state = GameState.Playing;

            if (CheckForWin(TileType.Cross)) // if crosses has won
                state = GameState.CrossWins;
            else if (CheckForWin(TileType.Naught))
                state = GameState.NaughtWins;
            else if (CheckForDraw())
                state = GameState.Draw;

            return state;
        }


        public bool CheckForWin(TileType type)
        {
            //horizontal wins
            if (grid[0] == type && grid[1] == type && grid[2] == type)
            return true;
            if (grid[3] == type && grid[4] == type && grid[5] == type)
            return true;
            if (grid[6] == type && grid[7] == type && grid[8] == type)
            return true;

            // Vertical wins
            if (grid[0] == type && grid[3] == type && grid[6] == type)
                return true;
            if (grid[1] == type && grid[4] == type && grid[7] == type)
                return true;
            if (grid[2] == type && grid[5] == type && grid[8] == type)
                return true;

            // Diagonal wins
            if (grid[0] == type && grid[4] == type && grid[8] == type)
                return true;
            if (grid[2] == type && grid[4] == type && grid[6] == type)
                return true;

            // NO WINS FOUND
            return false;
        }

        public bool CheckForDraw()
        {
            for(int i = 0; i < 9; ++i)
            {
                if (grid[i] == TileType.Blank)
                    return false; // found  blank space, still more moves to be made
            }

            // no moves left, draw game!
            return true;
        }

        public void ResetBoard()
        {
            for (int i = 0; i < 9; ++i)
            {
                //change grid cell to blank
                grid[i] = TileType.Blank;
                //change board buttons to blank
                if (buttons.Count >= 9)
                    buttons[i].Text = TileTypeToString(TileType.Blank);
            }
        }
    }
}
