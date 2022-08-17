using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

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

        // helper creator static function
        public static TCPChatClient CreateInstance(int port, int serverPort, string serverIP, Label playerTurnLabel, TextBox chatTextBox, PictureBox logoPic, TextBox clientUsername, List<Button> buttons)
        {
            TCPChatClient tcp = null;

            //check ports are open and in range
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
                tcp.clientSocket.isModerator = false; // set to false on start up
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
            while(!socket.Connected)
            {
                try
                {
                    attempts++;
                    SetChat("Connection Attempt: " + attempts);
                    socket.Connect(serverIP, serverPort);
                } 
                catch(Exception ex)
                {
                    chatTextBox.Text += "\nError: " + ex.Message + "\n";
                }

                if(attempts > 5)
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
            catch(SocketException ex)
            {
                AddToChat("\nError: " + ex.Message + "\n");
                AddToChat("\n << Disconnecting >>");
                currentClientSocket.socket.Close();
                return;
            }

            byte[] recBuf = new byte[received];
            Array.Copy(currentClientSocket.buffer, recBuf, received);
            string text = Encoding.ASCII.GetString(recBuf);

            // ammend strings accordingly for commands
            string tempUserName = "";
            string stateEnum = "";
            string packet3 = "";
            string packet4 = "";
            string currentClientUserName = "";
            string currentGameBoard = "";

            //Catch Jumbled up messages! Why does this happen?
            bool gameReset = false;
            string resetGameMessage = "";
            string oWinConditionMessage = "";
            string xWinConditionMessage = "";
            string drawMessage = "";

            //TODO SHUTS DOWN ON X WINS NOW...
            // catch and seperate string for X WIN conditon
            if(text.Contains(";") && text.Contains("!xwins")) // the delimeter (this is to distinguish between jumbled up messages
            {
                string[] substrings = text.Split(";");

                for(int i = 0; i < substrings.Length; ++i)
                {
                    if (substrings[i].Contains("!updateboard"))
                    {
                        currentGameBoard = "---------";
                    }
                    else if (substrings[i].Contains("!changestate"))
                    {
                        stateEnum = "1";
                    }
                    else if(substrings[i].Contains("'Chatting'"))
                    {
                        resetGameMessage = substrings[i];
                        gameReset = true;
                    }
                    else if (substrings[i].Contains("(Player 1)")) // X win specific catch
                    {
                        xWinConditionMessage = substrings[i];
                    }
                }
            }

            // catch and seperate string for O WIN conditon
            if (text.Contains(";") && text.Contains("!owins")) // the delimeter (this is to distinguish between jumbled up messages
            {
                string[] substrings = text.Split(";");

                for (int i = 0; i < substrings.Length; ++i)
                {
                    if (substrings[i].Contains("!updateboard"))
                    {
                        //string[] UBsubString = subString[i].Split(' ');
                        //currentGameBoard = UBsubString[1];
                        //currentGameBoard = currentGameBoard.Remove(9); // append ';' for corrent gameboard values
                        currentGameBoard = "---------";
                    }
                    else if (substrings[i].Contains("!changestate"))
                    {
                        //string[] CSsubStrings = subString[i].Split(' ');
                        //stateEnum = CSsubStrings[1]; ??
                        //stateEnum = stateEnum.Remove(1); // append ';' and everything after
                        stateEnum = "1";
                    }
                    else if (substrings[i].Contains("'Chatting'"))
                    {
                        resetGameMessage = substrings[i];
                        gameReset = true;
                    }
                    else if(substrings[i].Contains("(Player 2)")) // O win specific catch
                    {
                        oWinConditionMessage = substrings[i];
                    }
                }
            }

            //Catch and seperate string for DRAW condition
            if (text.Contains(";") && text.Contains("!draw"))
            {
                string[] substrings = text.Split(";");

                for (int i = 0; i < substrings.Length; ++i)
                {

                    if (substrings[i].Contains("!updateboard"))
                    {
                        currentGameBoard = "---------";
                    }
                    else if (substrings[i].Contains("!changestate"))
                    {
                        stateEnum = "1";
                    }
                    else if (substrings[i].Contains("'Chatting'"))
                    {
                        resetGameMessage = substrings[i];
                        gameReset = true;
                    }
                    else if (substrings[i].Contains("DRAW"))
                    {
                        drawMessage = substrings[i];
                    }
                }
            }

            if (text.Contains("!displayusername "))
            {
                tempUserName = text.Remove(0, 17);
                text = text.Remove(16, text.Length - 16);
            }
            //NOTE belows check handles 2-3 packets of data that got mixed up together
            
            if (text.Contains("!changestate ")) 
            {
                // seperate string data and assign correctly
                string[] subStrings = text.Split(' ');

                // Assign Strings from byte[] received.
                stateEnum = subStrings[1];
                char[] letters = stateEnum.ToCharArray();
                if(letters.Length > 1) // theres too many letters in our state enum because bit stream error
                {
                    //stateEnum = stateEnum.Remove(1);
                    stateEnum = "1"; // hardcode...
                }
                
                //check if the change state command contains more commands and assign variables accordingly
                if(subStrings.Length >= 3 && gameReset == false)
                {
                    packet3 = subStrings[2]; // this contains "!success" command
                    currentClientUserName = subStrings[4];
                    // below contains command string message
                    packet4 = subStrings[9].Replace(">>\r\n", "") + " " + subStrings[10] + " " + subStrings[11] + " " + subStrings[12] + " " +
                              subStrings[13] + " " + subStrings[14] + " " + subStrings[15] + " " + subStrings[16] + " " +
                              subStrings[17]; 
                }

                //text = subStrings[0];
            }
            
            if (text.Contains("!updateboard "))
            {
                string temp = currentGameBoard; // store previous gameboard for safe keeping
                string[] subString = text.Split(' ');
                currentGameBoard = subString[1];

                // catch for extra clients
                char[] charBoard = currentGameBoard.ToCharArray();
                if (charBoard.Length != 9) // too many or too less 
                {
                    currentGameBoard = temp; // re-set it back to temp one
                }
                //text = subString[0];
            }
            

            // Reaction Commands ---------------------------------------------------------------
            if(text.ToLower() == "!exit")
            {
                // Reset icon Identation
                IndentIcon();

                //Empty Username box
                clientUsernameTextBox.Invoke((Action)delegate
                {
                    clientUsernameTextBox.Text = "";
                });

                // clear chat window
                chatTextBox.Invoke((Action)delegate
                {
                    chatTextBox.Text = "<< Disconnected From Server >>";
                });

                // nullify username and other objects for re-connection parameters
                currentClientSocket.clientUserName = null;
                clientUsernameTextBox = null;
                chatTextBox = null;
                logoPicBox = null;
                return; // leave function as calling further will cause crashes
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
            else if (text.ToLower() == "!displayusername")
            {
                clientUsernameTextBox.Invoke((Action)delegate
                {
                    clientUsernameTextBox.Text = tempUserName;
                });
            }
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

                    ////////////////////////////////////

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
                clientSocket.state = ClientSocket.State.Playing;
                clientSocket.player = ClientSocket.Player.P1;
                clientSocket.isTurn = true;

                AddToChat(nl + "Let's play Tic Tac Toe!" + nl + "You are: PLAYER 1 (X's)");
            }
            else if (text.ToLower() == "!player2")
            {
                clientSocket.state = ClientSocket.State.Playing;
                clientSocket.player = ClientSocket.Player.P2;
                clientSocket.isTurn = false;

                AddToChat(nl + "Let's play Tic Tac Toe!" + nl + "You are: PLAYER 2 (O's)");
            }
            else if(text.ToLower() == "!gamefull")
            {
                AddToChat(nl + "< Game Full >" + nl + "..Wait until next round, peasant...");
            }
            else if (text.ToLower() == "!updateturn")
            {
                clientSocket.isTurn = true;
                if (clientSocket.player == ClientSocket.Player.P1) // X's
                {
                    AddToChat(nl + "<< Player 1's Turn (X) >>");
                }
                else
                {
                    AddToChat(nl + "<< Player 2's Turn (O) >>");
                }
            }
            else if(text.Contains("!updateboard"))
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
                updateTurnLabel(false);
            }
            else if (gameReset)
            {
                if (oWinConditionMessage != "")
                {
                    AddToChat(nl + oWinConditionMessage);
                }
                else if (xWinConditionMessage != "")
                {
                    AddToChat(nl + xWinConditionMessage);
                }
                else if (drawMessage != "")
                {
                    AddToChat(nl + drawMessage);
                }

                AddToChat(nl + resetGameMessage);
                gameReset = false;

                #region Update board catch
                //if (text.Contains("!owins") || text.Contains("!xwins"))
                //{
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
                updateTurnLabel(true);
                //}
                #endregion
            }
            else // regular chat message!
            {
                AddToChat(text);
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
    }
}
