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
        // socket for client
        public Socket socket = new Socket(AddressFamily.InterNetwork,
                                          SocketType.Stream,
                                          ProtocolType.Tcp);

        public ClientSocket clientSocket = new ClientSocket();

        //info to connect to server
        public int serverPort; // Port number
        public string serverIP; // IP address

        // helper creator static function
        public static TCPChatClient CreateInstance(int port, int serverPort, string serverIP, TextBox chatTextBox, PictureBox logoPic, TextBox clientUsername)
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
                tcp.chatTextBox = chatTextBox;
                tcp.clientSocket.socket = tcp.socket;
                tcp.logoPicBox = logoPic;
                tcp.clientUsernameTextBox = clientUsername;
                tcp.clientSocket.isConnected = true; // bool for connectivity
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
            //connection attempts per user
            int attempts = 0;

            //while socket is not connected to server
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

                // break if continues to fail
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
            //get our client socket from AR
            ClientSocket currentClientSocket = (ClientSocket)AR.AsyncState;

            //How many bytes of data received
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

            //build array ready for data
            byte[] recBuf = new byte[received];
            Array.Copy(currentClientSocket.buffer, recBuf, received); // copy info into array
            //convert received byte data into string
            string text = Encoding.ASCII.GetString(recBuf);

            // ammend strings accordingly for commands
            string tempUserName = "";
            string stateEnum = "";
            string packet3 = "";
            string packet4 = "";
            string currentClientUserName = "";
            string currentGameBoard = "";

            if (text.Contains("!displayusername "))
            {
                // create string to hold the username data
                tempUserName = text.Remove(0, 17);
                text = text.Remove(16, text.Length - 16);
            }
            //NOTE belows check handles 2-3 packets of data that got mixed up together
            else if (text.Contains("!changestate ")) 
            {
                // seperate string data and assign correctly
                string[] subStrings = text.Split(' ');

                // Assign Strings from byte[] received.
                stateEnum = subStrings[1];
                packet3 = subStrings[2]; // this contains "!success" command
                currentClientUserName = subStrings[4];
                // below contains command string message
                packet4 = subStrings[9].Replace(">>\r\n", "") + " " + subStrings[10] + " " + subStrings[11] + " " + subStrings[12] + " " +
                          subStrings[13] + " " + subStrings[14] + " " + subStrings[15] + " " + subStrings[16] + " " +
                          subStrings[17]; 
                text = subStrings[0];
            }
            else if (text.Contains("!updateboard "))
            {
                string[] subString = text.Split(' ');
                currentGameBoard = subString[1];
                text = subString[0];
            }
            

            // Reaction Commands ---------------------------------------------------------------
            //TODO Other commands
            //UPDATE BOARD
            //Is player joining game? playing?
            //WHat player client is
            // whose turn it is
            //gameover?? - reset to chatting phase

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
                //currentClientSocket.isModerator = false;
                clientUsernameTextBox = null;
                chatTextBox = null;
                logoPicBox = null;
                return; // leave function as calling further will cause crashes
            }
            else if(text.ToLower() == "!forcedkick") // chose a username that already exists, getting kicked
            {
                // tell 'em what happened
                chatTextBox.Invoke((Action)delegate
                {
                    chatTextBox.Text = "...disconnecting from server, username taken...";
                });

                currentClientSocket.socket.Shutdown(SocketShutdown.Both); // shutdown server-side for client
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
            else if(text.ToLower() == "!changestate")
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
                    if (packet3 == "!success")
                    {
                        AddToChat(nl + "<< Login Success >>" +
                                    nl + nl + "Welcome back " + "'" + currentClientUserName + "'" + nl
                                    + "Current State: " + clientSocket.state.ToString());
                    }
                    else if (packet3 == "!success2")
                    {
                        AddToChat(nl + "<< User Created Success >>" +
                                    nl + nl + "Welcome " + "'" + currentClientUserName + "'" + nl
                                    + "Current State: " + clientSocket.state.ToString());
                    }

                    // Notify client of other commands available
                    AddToChat(nl + packet4);
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
            }
            else if(text.ToLower() == "!updateboard")
            {
                //TODO UPDATING GAME BOARD, NOT WORKING??
                //update client game board
                for (int i = 0; i < grid.Length; i++)
                {
                    // break string down to read seperate chars
                    char[] position = currentGameBoard.ToCharArray();
                    TileType tile = new TileType();
                    
                    if (position[i] == 'x')
                    {
                        tile = TileType.Cross;
                        //TODO Invoke or not, the buttons list is STILL empty...
                        buttons[i].Invoke((Action)delegate
                        {
                            buttons[i].Text = TileTypeToString(TileType.Cross);
                        });
                    }
                    else if (position[i] == '0')
                    {
                        tile = TileType.Naught;
                        buttons[i].Text = TileTypeToString(TileType.Naught);
                    }
                    else if (position[i] == '-')
                    {
                        tile = TileType.Blank;
                        buttons[i].Text = TileTypeToString(TileType.Blank);
                    }

                    //TODO This will be the button change call when the list empty issue is solved..
                    //SetTile(i, tile);
                }
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
            //Send data to the server
            byte[] buffer = Encoding.ASCII.GetBytes(text); // encode data
            socket.Send(buffer, 0, buffer.Length, SocketFlags.None); // send encoded data
        }

        // shut down, no longer listening to server
        public void Close()
        {
            socket.Close();
        }
    }
}
