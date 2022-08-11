using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NDS_Networking_Project
{
    public partial class ChatWindow : Form
    {
        TicTacToe ticTacToe = new TicTacToe();
        TCPChatServer server = null;
        TCPChatClient client = null;

        public ChatWindow()
        {
            InitializeComponent();
        }

        private void HostServerButton_Click(object sender, EventArgs e)
        {
            if(CanHostOrJoin())
            {
                try
                {
                    // pass in text(as string) from host port text box to try convert to INT
                    int port = int.Parse(HostPortTextBox.Text);
                    // Below Builds server with REFERENCES to chat box and logo pic for access
                    server = TCPChatServer.CreateInstance(port, ChatTextBox, LogoPicBox, ClientUsernameTextBox); //try build a TCPChatServer object
                    if(server == null)
                    {
                        // ERRORS!
                        // throw error to be caught by 'catch' block
                        throw new Exception("<< Incorrect Port Value >>"); // when thrown, it exits try block, starts ctach block
                    }

                    server.SetupServer();
                    //TODO Reference Tic Tac Toe board here
                    server.ticTacToe = ticTacToe;

                    // Indent Icon for connectivity
                    LogoPicBox.BorderStyle = BorderStyle.Fixed3D;
                    ClientUsernameTextBox.Text = "HOST";

                }
                catch(Exception ex) // if chars other than numbers passed in...
                {
                    ChatTextBox.Text += "\nError: " + ex + "\n";
                }
            }
        }

        private void JoinServerButton_Click(object sender, EventArgs e)
        {
            if (CanHostOrJoin())
            {
                try
                {
                    // check if ports are correct format..
                    int port = int.Parse(HostPortTextBox.Text);
                    int serverPort = int.Parse(ServerPortTextBox.Text);

                    // assigne details to the client connecting
                    client = TCPChatClient.CreateInstance(port, 
                                                          serverPort, 
                                                          ServerIPTextBox.Text, 
                                                          ChatTextBox,
                                                          LogoPicBox,
                                                          ClientUsernameTextBox);
                    if(client == null)
                    {
                        //assume port issue
                        throw new Exception("<< Incorrect Port Value >>");
                    }

                    //client.clientSocket.state =
                    client.ConnectToServer();

                    // Indent Icon for connectivity
                    LogoPicBox.BorderStyle = BorderStyle.Fixed3D;
                }
                catch(Exception ex)
                {
                    ChatTextBox.Text += "\nError: " + ex + "\n";
                }
            }
        }

        private void SendButton_Click(object sender, EventArgs e)
        {   
            if(client != null) // sender is a client
            {
                client.SendString(TypeTextBox.Text);
                TypeTextBox.Clear(); // clears previous message
            }
            else if (server != null) // if sender is the server
            {
                // ---------------------------------------------------------------------------- !mod & !mods commands
                string message = TypeTextBox.Text;
                if (TypeTextBox.Text.Contains("!mod "))
                {
                    // concatonate string,
                    string[] sub = message.Split(' ');
                    string clientToMod = sub[1];
                    bool clientFound = false;
                    bool clientDemoted = false;

                    // check clients for double names
                    if(sub.Length == 3)
                    clientToMod = sub[1] + " " + sub[2];

                    // check name against connected clients
                    for(int i = 0; i < server.clientSockets.Count; ++i)
                    {   
                        //DEMOTING
                        if(clientToMod == server.clientSockets[i].clientUserName &&    // if user exists
                                          server.clientSockets[i].isModerator == true) // if they're already a mod
                        {
                            server.clientSockets[i].isModerator = false; // Demote client!
                            clientDemoted = true;
                            break; // leave loop, demoting is done
                        }
                        //PROMOTING
                        else if(clientToMod == server.clientSockets[i].clientUserName) // if user exists
                        {
                            // make that server a moderator
                            server.clientSockets[i].isModerator = true;
                            clientFound = true;
                        }
                    }

                    if(clientDemoted)
                    {
                        server.SendToAll("\n< " + clientToMod + " has been demoted as Moderator >", null); // notify others
                        server.AddToChat("\n< " + "Demoted " + clientToMod + " as Moderator >"); // notify self
                        clientDemoted = false; // reset for next run
                    }
                    else if (clientFound)
                    {
                        server.SendToAll("\n< " + clientToMod + " has been designated a Moderator >", null); // notify others
                        server.AddToChat("\n< " + "Designated " + clientToMod + " as Moderator >"); // notify self
                        clientFound = false; // reset for next run
                    }
                    else // no client by that username
                    {
                        server.AddToChat("\n" + "< No client by that name found >"); // notify self
                    }

                    clientToMod = ""; //reset for next run
                }
                else if(TypeTextBox.Text.Contains("!mods")) // ----------------------------- end !mod, start !mods command
                {
                    //create title for readability
                    server.AddToChat("\n\n" + "----- Moderators -----");

                    string names = "";

                    // run through connected users, add to string seperated by empty space.
                    for (int i = 0; i < server.clientSockets.Count; ++i)
                    {
                        // if the client is listed as a moderator, add them to string
                        if (server.clientSockets[i].isModerator) 
                        names += " " + server.clientSockets[i].clientUserName;
                    }

                    //append string, store seperate names in an array
                    string[] allNames = names.Split(' ');

                    //loop through array and send their data to client window!
                    for (int i = 0; i < allNames.Length; ++i)
                    {
                        string temp = allNames[i];
                        if (temp == "")
                        {
                            //SKIP
                        }
                        else
                        {
                            // BEGIN Double name check. 
                            if(i <= allNames.Length - 2) // catch for out of bounds index
                            {
                                bool doubleName = false;

                                // run through clients
                                for (int j = 0; j < server.clientSockets.Count; ++j)
                                {
                                    // check if the next 2 names in a row match the client username, to avoid double name seperation
                                    if (server.clientSockets[j].clientUserName == temp + " " + allNames[i + 1])
                                    {
                                        doubleName = true;
                                        break;
                                    }
                                }

                                if (doubleName)
                                {
                                    server.AddToChat("\n" + "User: " + temp + " " + allNames[i + 1]);
                                    ++i; // increment i to skip next name because its a part of this one
                                }
                                else
                                {
                                    server.AddToChat("\n" + "User: " + temp);
                                }
                            }
                            else
                            {
                                server.AddToChat("\n" + "User: " + temp);
                            }
                        }
                    }

                    if (allNames.Length <= 1) // there's only an empty string within array
                    {
                        server.AddToChat("\n" + "...no current moderators...");
                    }
                }
                else // regular message ------------------------------------------------------------end !mods comands
                {
                    server.SendToAll("HOST: " + TypeTextBox.Text, null);
                }
                TypeTextBox.Clear();
            }
        }

        public bool CanHostOrJoin()
        {
            if(client != null) // check if client already exists
            {
                // if they do but their username is null, They are a reconnecting client. 
                if(client.clientSocket.isConnected == true && client.clientSocket.clientUserName == null)
                {
                    // set null for reconnection
                    client = null;
                    server = null;
                }
            }

            if(server == null && client == null) //no server/client existt yet, can host/join
            {
                return true;
            }
            else // you're a client or server already
            {
                return false;
            }
        }

        // Click events for Tic Tac Toe Board
        private void button1_Click(object sender, EventArgs e)
        {
            AttemptMove(0);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            AttemptMove(1);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            AttemptMove(2);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            AttemptMove(3);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            AttemptMove(4);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            AttemptMove(5);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            AttemptMove(6);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            AttemptMove(7);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            AttemptMove(8);
        }

        private void AttemptMove(int index)
        {
            //if client and is clients turn
            if(client != null && client.clientSocket.isTurn)
            {
                // set tile type for each player
                if (client.clientSocket.player == ClientSocket.Player.P1)
                    ticTacToe.playerTileType = TileType.Cross;
                else if (client.clientSocket.player == ClientSocket.Player.P2)
                    ticTacToe.playerTileType = TileType.Naught;

                // paint the board!
                bool validMove = ticTacToe.SetTile(index, ticTacToe.playerTileType);

                if(validMove)
                {
                    // demote their turn rights
                    client.clientSocket.isTurn = false;
                    //check bool for server distiguishing whose turn it is next
                    //client.clientSocket.justHadTurn = true;

                    //tell server I did my turn at position 'index'
                    client.SendString("!move " + index);

                    // ticTacToe.myTurn = false; // gotta wait till your damn turn
                }
                else // NOT valid move
                {

                }

                //TODO Client Side Prediction..
                //TODO move this to server side as each client shouldn't be doing it..(maybe) this is for testing
                GameState gs = ticTacToe.GetGameState();
                if(gs == GameState.CrossWins)
                {
                    ChatTextBox.AppendText("X Wins!");
                    ChatTextBox.AppendText(Environment.NewLine);
                    ticTacToe.ResetBoard();
                    //TELL all to RESET boards
                    //RESET all players back to Chatting state
                    // other clean up...
                } 
                else if (gs == GameState.NaughtWins)
                {
                    ChatTextBox.AppendText("O Wins!");
                    ChatTextBox.AppendText(Environment.NewLine);
                    ticTacToe.ResetBoard();
                    //TELL all to RESET boards
                    //RESET all players back to Chatting state
                    // other clean up...
                }
                else if (gs == GameState.Draw)
                {
                    ChatTextBox.AppendText("Draw! ...No Winners...");
                    ChatTextBox.AppendText(Environment.NewLine);
                    ticTacToe.ResetBoard();
                    //TELL all to RESET boards
                    //RESET all players back to Chatting state
                    // other clean up...
                }
            }
        }

        // Runs on startup of window
        private void ChatWindow_Load(object sender, EventArgs e)
        {
            // let ticTacToe object know about our gameboard buttons
            ticTacToe.buttons.Add(button1);
            ticTacToe.buttons.Add(button2);
            ticTacToe.buttons.Add(button3);
            ticTacToe.buttons.Add(button4);
            ticTacToe.buttons.Add(button5);
            ticTacToe.buttons.Add(button6);
            ticTacToe.buttons.Add(button7);
            ticTacToe.buttons.Add(button8);
            ticTacToe.buttons.Add(button9);
        }
    }
}
