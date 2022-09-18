using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Data.SQLite;

namespace NDS_Networking_Project
{
    public class TCPChatClient : TCPChatBase
    {
        public Socket socket = new Socket(AddressFamily.InterNetwork,
                                          SocketType.Stream,
                                          ProtocolType.Tcp);
        public ClientSocket clientSocket = new ClientSocket();
        public int serverPort;
        public string serverIP;
        public TicTacToe ticTacToe;

        public static TCPChatClient CreateInstance(int port, int serverPort, string serverIP, Label playerTurnLabel, TextBox chatTextBox, PictureBox logoPic, TextBox clientUsername, List<Button> buttons)
        {
            TCPChatClient tcp = null;

            if (port > 0 && port < 65535 &&
                serverPort > 0 && serverPort < 65535 &&
                serverIP.Length > 0 && chatTextBox != null)
            {
                tcp = new TCPChatClient();
                tcp.port = port;
                tcp.serverPort = serverPort;
                tcp.serverIP = serverIP;
                tcp.clientSocket.socket = tcp.socket;
                tcp.playerTurnLabel = playerTurnLabel;
                tcp.chatTextBox = chatTextBox;
                tcp.logoPicBox = logoPic;
                tcp.clientUsernameTextBox = clientUsername;
                tcp.buttons = buttons; // set list
                tcp.clientSocket.isConnected = true;
                tcp.clientSocket.isModerator = false;
                tcp.clientSocket.isTurn = false; // set client turn for ticTacToe to default
                tcp.clientSocket.state = ClientSocket.State.Login; // set state to Login state upon connection
                tcp.clientSocket.player = ClientSocket.Player.NotPlaying; // NotPlaying is default
            }
            return tcp;
        }

        //Try connect to server, notify if successfull
        public void ConnectToServer()
        {
            int attempts = 0;
            while (!socket.Connected)
            {
                try
                {
                    attempts++;
                    SetChat("Connection Attempt: " + attempts);
                    socket.Connect(serverIP, serverPort);
                }
                catch (Exception ex)
                {
                    chatTextBox.Text += "\nError: " + ex.Message + "\n";
                }

                if (attempts > 5)
                {
                    AddToChat(nl + "<<< Connection Failed >>>");
                    return;
                }
            }

            AddToChat(nl + "<< Connected >>" + nl + "...ready to receive data..." +
                      nl + nl + "-----------------------------------------------------------------------" +
                      nl + "< Please enter your Username & Password using the '!login' command >" +
                      nl + "  e.g !login [username_here] [password]" +
                      nl + "-----------------------------------------------------------");

            //start thread for receeiving data from the server
            clientSocket.socket.BeginReceive(clientSocket.buffer,
                                             0,
                                             ClientSocket.BUFFER_SIZE,
                                             SocketFlags.None,
                                             ReceiveCallBack,
                                             clientSocket);
        }

        //Everytime a bit of data comes in from server, this function reads it
        public void ReceiveCallBack(IAsyncResult AR)
        {
            ClientSocket currentClientSocket = (ClientSocket)AR.AsyncState;
            int received;
            try
            {
                received = currentClientSocket.socket.EndReceive(AR);
            }
            catch (SocketException ex)
            {
                AddToChat("\nError: " + ex.Message + "\n");
                AddToChat("\n << Disconnecting >>");
                currentClientSocket.socket.Close();
                return;
            }

            byte[] recBuf = new byte[received];
            Array.Copy(currentClientSocket.buffer, recBuf, received);
            string text = Encoding.ASCII.GetString(recBuf);

            // string variables for commands --------------------------------------------------
            string tempUserName = "";
            string stateEnum = "";
            string packet3 = "";
            string packet4 = "";
            string packet5 = "";
            string packet6 = "";
            string currentClientUserName = "";
            string currentGameBoard = "";
            string winnerName = "";
            string loserName = "";
            bool gameReset = false;

            #region Get Time Stamp
            // get time info
            DateTime time = DateTime.Now;
            string timeNow = time.ToString();
            int hour = time.Hour;
            if (hour > 12) // PM, moving into 24hour time
            {
                hour -= 12; // convert to 24 hour time
                timeNow = hour + ":" + time.Minute + ":" + time.Second + "pm";
            }
            else // AM
            {
                timeNow = hour + ":" + time.Minute + ":" + time.Second + "am";
            }
            #endregion

            // Ammend command strings received --------------------------------------------------
            if (text.Contains("!displayusername "))
            {
                string[] substrings = text.Split("!");
                if(substrings.Length > 2) // theres a jumbled message in here
                {
                    tempUserName = substrings[1]; // set as first message, this SHOULD be "displayusername [name_here]"
                    tempUserName = tempUserName.Remove(0, 16); // grab username
                    //text = text.Remove(16, text.Length - 16); // set text back to basic command
                }
                else // its just the usual
                {
                    tempUserName = text.Remove(0, 17); // grab username
                    //text = text.Remove(16, text.Length - 16); // set text back to basic command
                }

                // update username display
                clientUsernameTextBox.Invoke((Action)delegate
                {
                    clientUsernameTextBox.Text = tempUserName;
                });
            }

            //NOTE belows check handles 2-3 packets of data from login request that got mixed up together
            if (text.Contains("!changestate "))
            {
                // seperate string data and assign correctly
                string[] subStrings = text.Split(' ');

                // Assign Strings from byte[] received.
                stateEnum = subStrings[1];
                char[] letters = stateEnum.ToCharArray();
                if (letters.Length > 1) // theres too many letters in our state enum because bit stream error
                {
                    //stateEnum = stateEnum.Remove(1);
                    stateEnum = "1"; // hardcode... string seems to change cient to client..
                }

                //check if the change state command contains more commands and assign variables accordingly
                if (subStrings.Length >= 3 && gameReset == false)
                {

                    // find !success command
                    for (int i = 0; i < subStrings.Length; i++)
                    {
                        if (subStrings[i].Contains("!success2")) // First tme login/creation condition
                        {
                            packet3 = subStrings[i];// this contains "!success" command
                            currentClientUserName = subStrings[5];
                            // below contains command string message
                            packet4 = subStrings[10].Replace(">>\r\n", "") + " " + subStrings[11] + " " + subStrings[12] + " " + subStrings[13] + " " +
                                      subStrings[14] + " " + subStrings[15] + " " + subStrings[16] + " " + subStrings[17] + " " +
                                      subStrings[18];
                            break; // leave for loop
                        }
                        else if(subStrings[i].Contains("!success")) // re-login condition
                        {
                            // this contains "!success" command
                            packet3 = subStrings[i];
                            currentClientUserName = subStrings[4];
                            // below contains command string message
                            packet4 = subStrings[9].Replace(">>\r\n", "") + " " + subStrings[10] + " " + subStrings[11] + " " + subStrings[12] + " " +
                                      subStrings[13] + " " + subStrings[14] + " " + subStrings[15] + " " + subStrings[16] + " " +
                                      subStrings[17];
                            break; // leave for loop
                        }
                    }
                }
            }

            if (text.Contains("!updateboard "))
            {
                string temp = currentGameBoard; // store previous gameboard for safe keeping
                string[] subString = text.Split(' ');
                currentGameBoard = subString[1];

                // catch for extra clients and second passes. in particular, bame board reset calls
                char[] charBoard = currentGameBoard.ToCharArray();
                if (charBoard.Length != 9) // too many or too less 
                {
                    currentGameBoard = temp; // re-set it back to temp one
                }
            }

            if (text.Contains("!xwins") || text.Contains("!owins") || text.Contains("!draw"))
            {
                string[] subStrings = text.Split(" ");
                text = subStrings[0];
                winnerName = subStrings[1];
                loserName = subStrings[2];
            }

            // Reaction Commands ---------------------------------------------------------------
            if(text.ToLower() == "!exit") // A2 command, exit gracefully..
            {
                IndentIcon();
                clientUsernameTextBox.Invoke((Action)delegate
                {
                    clientUsernameTextBox.Text = "";
                });

                chatTextBox.Invoke((Action)delegate
                {
                    chatTextBox.Text = "<< Disconnected From Server >>";
                });

                currentClientSocket.clientUserName = null;
                clientUsernameTextBox = null;
                chatTextBox = null;
                logoPicBox = null;
                return;
            }
            else if(text.ToLower() == "!printscores")
            {
                // dump all data from users, wins, loses columns to chatbox
                string connectionStr = "Data Source=TCPChatDB.db";
                var connection = new SQLiteConnection(connectionStr);
                connection.Open();

                string scoreCommand = "SELECT Username, Wins, Losses, Draws FROM Users GROUP BY Username ORDER BY Wins DESC";
                SQLiteCommand scoreCmd = new SQLiteCommand(scoreCommand, connection);
                SQLiteDataReader scoreRdr = scoreCmd.ExecuteReader();

                AddToChat("__________SCORES_________" + nl +
                          "User: Wins, Losses, Draws");

                while (scoreRdr.Read())
                {
                    // make variables every run through for new user
                    string name = scoreRdr.GetString(0);
                    int wins = scoreRdr.GetInt32(1);
                    int losses = scoreRdr.GetInt32(2);
                    int draws = scoreRdr.GetInt32(3);

                    if(wins < 10 || losses < 10)
                        AddToChat(" " + name + ":  " + wins + ",   " + losses + ",   " + draws);
                    else
                        AddToChat(" " + name + ":  " + wins + ",  " + losses + ",  " + draws);
                }

                scoreRdr.Close();
                connection.Close();
            }
            else if(text.ToLower() == "!xwins")
            {
                gameReset = true;

                //display winner message
                AddToChat(nl + "<<< " + winnerName + " (X's) WINS >>>");

                //reset game board
                ticTacToe.ResetBoard();
                string resetboard = ticTacToe.GridToString(); // pull data from tictactoe grid (should now be empty)
                UpdateGameBoard(resetboard, gameReset);

                //reset state
                stateEnum = "1"; // chatting
                ChangeState(stateEnum);

               //RESET player back to PLAYER status
               if (clientSocket.player == ClientSocket.Player.P1)
                   clientSocket.player = ClientSocket.Player.NotPlaying;
               else if (clientSocket.player == ClientSocket.Player.P2)
                   clientSocket.player = ClientSocket.Player.NotPlaying;

                //display state changes
                AddToChat(nl + "...All players reverted to 'Chatting' state..." + nl +
                               "< New game free to join! >");
            }
            else if (text.ToLower() == "!owins")
            {
                gameReset = true;

                //display winner message
                AddToChat(nl + "<<< " + winnerName + " (O's) WINS >>>");

                //reset game board
                ticTacToe.ResetBoard();
                string resetboard = ticTacToe.GridToString();
                UpdateGameBoard(resetboard, gameReset);

                //reset state
                stateEnum = "1"; // chatting
                ChangeState(stateEnum);

                //RESET player back to PLAYER status
                if (clientSocket.player == ClientSocket.Player.P1)
                    clientSocket.player = ClientSocket.Player.NotPlaying;
                else if (clientSocket.player == ClientSocket.Player.P2)
                    clientSocket.player = ClientSocket.Player.NotPlaying;

                //display state changes
                AddToChat(nl + "...All players reverted to 'Chatting' state..." + nl +
                               "< New game free to join! >");
            }
            else if (text.ToLower() == "!draw")
            {
                gameReset = true;

                //display winner message
                AddToChat(nl + "<<< DRAW >>>" + nl
                             + winnerName + " and " + loserName + " are equal losers.");

                //reset game board
                ticTacToe.ResetBoard();
                string resetboard = ticTacToe.GridToString();
                UpdateGameBoard(resetboard, gameReset);

                //reset state too chatting
                stateEnum = "1"; // chatting
                ChangeState(stateEnum);

                //RESET player back to PLAYER status
                if (clientSocket.player == ClientSocket.Player.P1)
                    clientSocket.player = ClientSocket.Player.NotPlaying;
                else if (clientSocket.player == ClientSocket.Player.P2)
                    clientSocket.player = ClientSocket.Player.NotPlaying;

                //display state changes
                AddToChat(nl + "...All players reverted to 'Chatting' state..." + nl +
                               "< New game free to join! >");
            }
            else if(text.ToLower() == "!forcedkick") // chose a username that already exists, getting kicked
            {
                chatTextBox.Invoke((Action)delegate
                {
                    chatTextBox.Text = "...disconnecting from server, username taken...";
                });

                currentClientSocket.socket.Shutdown(SocketShutdown.Both);
                currentClientSocket.socket.Close();
                AddToChat(nl + "<< Client Disconnected >>");
                return;
            }
            //else if (text.ToLower() == "!displayusername")
            //{
            //    clientUsernameTextBox.Invoke((Action)delegate
            //    {
            //        clientUsernameTextBox.Text = tempUserName;
            //    });
            //}
            else if(text.Contains("!changestate"))
            {
                if(stateEnum == "0")
                {
                    clientSocket.state = ClientSocket.State.Login;
                }
                else if(stateEnum == "1")
                {
                    //Update state!
                    clientSocket.state = ClientSocket.State.Chatting;
                    
                    // --Catch for initial login bit stream error of receiving messages!--
                    // Send Login Success Message and Display State
                    if (packet3 != "" && packet3 == "!success")
                    {
                        AddToChat(nl + "<< Login Success >>" +
                                    nl + nl + "Welcome back " + "'" + currentClientUserName + "'" + nl
                                    + "Current State: " + clientSocket.state.ToString());
                        packet3 = ""; // reset for change state during gameplay
                    }
                    else if (packet3 != "" && packet3 == "!success2")
                    {
                        AddToChat(nl + "<< User Created Success >>" +
                                    nl + nl + "Welcome " + "'" + currentClientUserName + "'" + nl
                                    + "Current State: " + clientSocket.state.ToString());
                        packet3 = ""; // reset for change state during gameplay
                    }

                    // Notify client of other commands available
                    if (packet4 != "")
                    {
                        AddToChat(nl + packet4);
                        packet4 = "";
                    }
                }
                else if (stateEnum == "2")
                {
                    clientSocket.state = ClientSocket.State.Playing;
                }
            }
            else if (text.ToLower() == "!player1")
            {
                // set state and player locally
                clientSocket.state = ClientSocket.State.Playing;
                clientSocket.player = ClientSocket.Player.P1;
                clientSocket.isTurn = true;

                AddToChat(nl + "Let's play Tic Tac Toe!" + nl + "You are: PLAYER 1 (X's)" + nl + nl + "...Awaiting another player...");
            }
            else if (text.ToLower() == "!player2")
            {
                // set state and player locally
                clientSocket.state = ClientSocket.State.Playing;
                clientSocket.player = ClientSocket.Player.P2;
                clientSocket.isTurn = false;

                AddToChat(nl + "Let's play Tic Tac Toe!" + nl + "You are: PLAYER 2 (O's)" + nl + nl + "Ready To Play!");
            }
            else if (text.ToLower() == "!letsplay")
            {
                AddToChat("Player 2 joined - Ready To Play!");
            }
            else if(text.ToLower() == "!gamefull")
            {
                AddToChat(nl + "< Game Full >" + nl + "..Wait until next round, peasant...");
            }
            else if (text.ToLower() == "!updateturn")
            {
                if (clientSocket.player == ClientSocket.Player.P1) // X's
                {
                    AddToChat(nl + timeNow + " - << Player 1's Turn (X) >>");
                }
                else // O's
                {
                    AddToChat(nl + timeNow + " - << Player 2's Turn (O) >>");
                }
                clientSocket.isTurn = true;
            }
            else if(text.Contains("!updateboard"))
            {
                UpdateGameBoard(currentGameBoard, gameReset);
            }
            else // regular chat message!
            {
                AddToChat(timeNow + " - " + text);
            }
            // ----------------------------------------------------------------------------- Reaction Commands

            //start thread for receeiving data from the server
            currentClientSocket.socket.BeginReceive(currentClientSocket.buffer,
                                             0,
                                             ClientSocket.BUFFER_SIZE,
                                             SocketFlags.None,
                                             ReceiveCallBack,
                                             currentClientSocket);
        }

        // Sends string to server
        public void SendString(string text)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(text);
            socket.Send(buffer, 0, buffer.Length, SocketFlags.None);
        }

        // shut down, no longer listening to server
        public void Close()
        {
            socket.Close();
        }

        // Updates own game board and sets 'turn' label
        public void UpdateGameBoard(string currentGameBoard, bool condition)
        {
            //update client game grid
            for (int i = 0; i < ticTacToe.grid.Length; i++)
            {
                // break string down to read seperate chars
                char[] position = currentGameBoard.ToCharArray();
                TileType tile = new TileType();

                if (position[i] == 'x')
                {
                    tile = TileType.Cross;
                }
                else if (position[i] == 'o')
                {
                    tile = TileType.Naught;
                }
                else if (position[i] == '-')
                {
                    tile = TileType.Blank;
                }

                ticTacToe.grid[i] = tile; // set grid
            }
            //update board text 
            UpdateGameBoardText(currentGameBoard);
            updateTurnLabel(condition);
        }

        // Change the local state of the client
        public void ChangeState(string stateEnum)
        {
            if (stateEnum == "0")
            {
                clientSocket.state = ClientSocket.State.Login;
            }
            else if (stateEnum == "1")
            {
                clientSocket.state = ClientSocket.State.Chatting;
            }
            else if (stateEnum == "2")
            {
                clientSocket.state = ClientSocket.State.Playing;
            }
        }
    }
}
