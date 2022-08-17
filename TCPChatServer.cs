using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Windows.Forms;
using System.Data.SQLite; // Using statement for SQL nuget package

namespace NDS_Networking_Project
{
    public class TCPChatServer : TCPChatBase
    {
        public static ChatWindow window = new ChatWindow();

        public Socket serverSocket = new Socket(AddressFamily.InterNetwork, 
                                                SocketType.Stream, 
                                                ProtocolType.Tcp);
        // Connected clients
        public List<ClientSocket> clientSockets = new List<ClientSocket>();

        public TicTacToe ticTacToe;

        // strings for !whisper function
        public string privateMsgReceiver = "";
        public string privateMsgSender = "";
        public string privateMessage = "";
        public bool isPrivateMessage = false;
        public string board = "---------"; // default board string

        //Helper creator function
        public static TCPChatServer CreateInstance(int port, TextBox chatTextBox, PictureBox logo, TextBox usernameTextBox, Label playerTurnLabel, List<Button> buttons)
        {
            TCPChatServer tcp = null; // set to null, if it returns null, user did something wrong

            //setup if port within range & textbox not null
            if(port > 0 && port < 65535 && // port within range
               chatTextBox != null) // text box exists
            {
                tcp = new TCPChatServer();
                tcp.port = port;
                tcp.chatTextBox = chatTextBox;
                tcp.logoPicBox = logo;
                tcp.clientUsernameTextBox = usernameTextBox;
                tcp.playerTurnLabel = playerTurnLabel;
                tcp.buttons = buttons;
            }

            //retunr as null OR built server
            return tcp;
        }

        public void SetupServer()
        {
            chatTextBox.Text += "...setting up server..." + nl;
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, port));
            serverSocket.Listen(0);
            serverSocket.BeginAccept(AcceptCallBack, this);
            chatTextBox.Text += "<< Server Setup Complete >>" + nl;

            //TODO Open DB Here
            chatTextBox.Text += "...constructing database..." + nl; // notify of db building..
            // Create Database
            string connectionString = "Data Source=TCPChatDB.db";
            var connection = new SQLiteConnection(connectionString);
            connection.Open();

            // Build Table to store Tic Tac Toe data--------
            // Create commandtext string to run
            SQLiteCommand cmd = new SQLiteCommand("CREATE TABLE IF NOT EXISTS Users(ID INTEGER PRIMARY KEY," +
                                                                                   "Username Text," +
                                                                                   "Password Text," +
                                                                                   "Wins INTEGER," +
                                                                                   "Losses INTEGER," +
                                                                                   "Draws INTEGER," +
                                                                                   "UNIQUE(ID, Username))", connection); // make ID and Username unique so there are NO duplicates
            cmd.ExecuteNonQuery(); // run command
            chatTextBox.Text += "<< Database Initialization Complete >>" + nl;
            connection.Close(); //close DB and save
        }

        // to close/diconnect all sockets at end of program
        public void CloseAllSockets()
        {
            foreach(ClientSocket clientSocket in clientSockets)
            {
                clientSocket.socket.Shutdown(SocketShutdown.Both);
                clientSocket.socket.Close();
            }
            clientSockets.Clear();
            serverSocket.Close();
        }

        // callback called when a client joins the server
        public void AcceptCallBack(IAsyncResult AR)
        {
            Socket joiningSocket; 
            try
            {
                joiningSocket = serverSocket.EndAccept(AR);
            }
            catch(ObjectDisposedException)
            {
                chatTextBox.Text += "...Client Join Failed...";
                return;
            }

            ClientSocket newClientSocket = new ClientSocket();
            newClientSocket.socket = joiningSocket;

            clientSockets.Add(newClientSocket);

            joiningSocket.BeginReceive(newClientSocket.buffer,    
                                       0,                         
                                       ClientSocket.BUFFER_SIZE,  
                                       SocketFlags.None,          
                                       ReceiveCallBack,           
                                       newClientSocket);          

            AddToChat(nl + "<< Client Connected >>"); 
            serverSocket.BeginAccept(AcceptCallBack, null);
        }

        // this function calls anytime data comes in from a client
        public void ReceiveCallBack(IAsyncResult AR)
        {
            ClientSocket currentClientSocket = (ClientSocket)AR.AsyncState;
            int received;

            try
            {
                received = currentClientSocket.socket.EndReceive(AR);
            }
            catch(Exception ex)
            {
                AddToChat("Error: " + ex.Message + nl + nl);
                AddToChat("! Error Occured !" + nl + "<< Client Disconnected >>");
                currentClientSocket.socket.Close();
                clientSockets.Remove(currentClientSocket);
                return;
            }

            byte[] recBuf = new byte[received];
            Array.Copy(currentClientSocket.buffer, recBuf, received);
            string text = Encoding.ASCII.GetString(recBuf);

            AddToChat(text);

            // Strings for command specific data control
            string clientLoginUsername = "";
            string clientPassword = "";
            string changeNameUserName = "";
            string magicQuestion = "";
            string userToKick = "";
            string playerSymbol = "";
            string boardIndex = "";
            //string board = "---------"; // default board string

            //Check for text specific commands from clients and adjust data accordingly below -----------
            if (text.Contains("!login ")) // setting username & password
            {
                string[] subString = text.Split(' ');

                // create string to hold the username data
                if (subString.Length == 4) // if the username is two words
                {
                    clientLoginUsername = subString[1] + " " + subString[2];
                    clientPassword = subString[3];
                }
                else
                {
                    clientLoginUsername = subString[1];
                    clientPassword = subString[2];
                }
                // OLD setupUserName = text.Remove(0, 10);

                //append text so it triggers command
                text = subString[0];
                // OLDtext = text.Remove(9, text.Length - 9);
            }
            else if (text.Contains("!user ")) // changing name
            {
                changeNameUserName = text.Remove(0, 6);
                text = text.Remove(5, text.Length - 5);
            }
            else if (text.Contains("!whisper ")) // private messaging
            {
                string[] subStrings = text.Split(' ');
                privateMsgReceiver = subStrings[1];

                for (int i = 0; i < clientSockets.Count; ++i)
                {
                    if (subStrings[1] + " " + subStrings[2] == clientSockets[i].clientUserName)
                        privateMsgReceiver = subStrings[1] + " " + subStrings[2];
                }

                int messageIndex = (9 + privateMsgReceiver.Length);
                privateMessage = text.Substring(messageIndex, (text.Length - messageIndex));

                text = subStrings[0];
            }
            else if(text.Contains("!magic "))
            {
                string[] subStrings = text.Split(' ');

                int messageIndex = (7 + privateMsgReceiver.Length);
                magicQuestion = text.Substring(messageIndex, (text.Length - messageIndex));

                text = subStrings[0];
            }
            else if(text.Contains("!kick "))
            {
                string[] subStrings = text.Split(' ');
                userToKick = subStrings[1];
                if(subStrings.Length == 3)
                    userToKick = subStrings[1] + " " + subStrings[2];
                text = subStrings[0];
            }
            else if (text.Contains("!move "))
            {
                string[] subStrings = text.Split(' ');

                playerSymbol = subStrings[2];
                boardIndex = subStrings[1];
                text = subStrings[0];
            }

            // Text specific command actions -------------------------------------------------------------
            if (text.ToLower() == "!commands")
            {
                byte[] data = Encoding.ASCII.GetBytes(nl + "<< COMMANDS >>" +
                                                      nl + "!commands   --> see a list of commands" +
                                                      nl + "!username [new_username]   --> set yourself a new username" +
                                                      nl + "!user   --> change your current username" +
                                                      nl + "!about   --> see details of the program" +
                                                      nl + "!who   --> see who is in the chat" +
                                                      nl + "!whisper [username] [message]   --> select user for private message" +
                                                      nl + "!magic [question]   --> ask the Magic-8-Ball a question, reap its wisdom" +
                                                      nl + "!kick [username]   --> kick selected user from the chat <<MODERATORS ONLY>>" +
                                                      nl + "!exit   --> disconnect from the server");
                currentClientSocket.socket.Send(data);
                data = Encoding.ASCII.GetBytes(nl + "-----------------------------------------------------------");
                currentClientSocket.socket.Send(data);
                AddToChat("\n...commands sent to client...");
            }
            else if (text.ToLower() == "!about")
            {
                string IP = currentClientSocket.socket.LocalEndPoint.ToString();

                string[] sub = IP.Split(":");
                string appendedPort = IP.Replace(sub[0] + ":", "");
                string appendedIP = IP.Replace(":" + sub[1], "");

                byte[] data = Encoding.ASCII.GetBytes(nl + "Created by Matthew Carr & Charles Bird" +
                                                      nl + "to ensure people have a ways to communicate in style." +
                                                      nl + nl + "IP address: " + appendedIP +
                                                      nl + "Port number: " + appendedPort +
                                                      nl + nl + "Netwoes INC. Copyright (c) All Rights Reserved, TM (2022)");
                currentClientSocket.socket.Send(data);

                data = Encoding.ASCII.GetBytes(nl + "-----------------------------------------------------------");
                currentClientSocket.socket.Send(data);

            }
            else if (text.ToLower() == "!who")
            {
                byte[] data = Encoding.ASCII.GetBytes(nl + "----- Connected Users -----");
                currentClientSocket.socket.Send(data);

                string names = "";
                for (int i = 0; i < clientSockets.Count; ++i)
                {
                    names += " " + clientSockets[i].clientUserName;
                }

                string[] allNames = names.Split(' ');

                for (int i = 0; i < allNames.Length; ++i)
                {
                    string temp = allNames[i];
                    if (temp == "")
                    {
                        //SKIP
                    }
                    else
                    {
                        // Double name check. 
                        if (i <= allNames.Length - 2)
                        {
                            bool doubleName = false;
                            for (int j = 0; j < clientSockets.Count; ++j)
                            {
                                // check if the next 2 names in a row match the client username, to avoid double name seperation
                                if (clientSockets[j].clientUserName == temp + " " + allNames[i + 1])
                                {
                                    doubleName = true;
                                    break;
                                }
                            }

                            if (doubleName)
                            {
                                data = Encoding.ASCII.GetBytes(nl + "User: " + temp + " " + allNames[i + 1]);
                                currentClientSocket.socket.Send(data);
                                ++i;
                            }
                            else
                            {
                                data = Encoding.ASCII.GetBytes(nl + "User: " + temp);
                                currentClientSocket.socket.Send(data);
                            }
                        }
                        else // last client in list, just send data
                        {
                            data = Encoding.ASCII.GetBytes(nl + "User: " + temp);
                            currentClientSocket.socket.Send(data);
                        }
                    }
                }
                // if there are no clients
                if (allNames[0] == "" && allNames[1] == "")
                {
                    data = Encoding.ASCII.GetBytes(nl + "...it's just you here..." +
                                                   nl + " *tumbleweed blows by*");
                    currentClientSocket.socket.Send(data);
                }

                data = Encoding.ASCII.GetBytes(nl + "-----------------------------------------------------------");
                currentClientSocket.socket.Send(data);
            }
            else if (text.ToLower() == "!whisper")
            {
                byte[] data = Encoding.ASCII.GetBytes(nl + "<Private Message> " + currentClientSocket.clientUserName +
                                                      ":" + privateMessage);

                for (int i = 0; i < clientSockets.Count; ++i)
                {
                    if (clientSockets[i].clientUserName == privateMsgReceiver)
                    {
                        clientSockets[i].socket.Send(data);
                    }
                }

                privateMsgReceiver = "";
                privateMessage = "";
            }
            else if (text.ToLower() == "!login")
            {
                //Open DB
                string connectionString = "Data Source=TCPChatDB.db"; // find and open db
                var connection = new SQLiteConnection(connectionString);
                connection.Open();

                // create command string to check if any data in DB
                string sqlCommandText = "SELECT COUNT(*) FROM Users";
                
                // create command
                SQLiteCommand cmd = new SQLiteCommand(sqlCommandText, connection);
                
                //Try INSERT data if Doesn't exist for FIRST TIME RUN
                long numRows = (long)cmd.ExecuteScalar();
                if (numRows > 0)
                {
                    // Data already exists. Move on
                }
                else
                {
                    // INSERT data!
                    cmd.CommandText = string.Format("INSERT OR IGNORE INTO Users(Username, Password, Wins, Losses, Draws) " +
                                         "VALUES('{0}','{1}', 0, 0, 0)", clientLoginUsername, clientPassword);
                    cmd.ExecuteNonQuery(); // execute adding of new user

                    //Notify server window
                    AddToChat("Inserted User: " + clientLoginUsername + " with Password: " + clientPassword + " into DB");
                }
                
                // update command to check data already in DB table Users
                sqlCommandText = string.Format("SELECT Username, Password " +
                                                      "FROM Users");
                cmd.CommandText = sqlCommandText;
                SQLiteDataReader rdr = cmd.ExecuteReader();// read data

                // reader bools
                bool userExists = false;
                bool passwordMatches = false;
                bool addUser = false;

                // Check database for users, assign necessary bools
                while(rdr.Read())
                {
                    //reads line by line
                    string storedName = rdr.GetString(0); // index would be 0 as Username is called first in command text
                    string storedPassword = rdr.GetString(1);

                    //if the user exists
                    if(clientLoginUsername == storedName)
                    {
                        // user exists
                        userExists = true;
                        passwordMatches = false;

                        // if the password matches
                        if (clientPassword == storedPassword)
                        {
                            userExists = true;
                            passwordMatches = true;
                            break;
                        }
                    }
                    else
                    {
                        // else no user exists, add them
                        addUser = true;
                    }
                }
                rdr.Close(); // close reader

                if(userExists && passwordMatches)
                { 
                    //continue log in process
                    //SET USERNAME TO CLIENTSOCKET
                    currentClientSocket.clientUserName = clientLoginUsername; // set usernme
                    currentClientSocket.state = ClientSocket.State.Chatting;

                    //UPDATE USER DISPLAY Send data to update display box for client Usernames
                    byte[] packet1 = Encoding.ASCII.GetBytes("!displayusername " + clientLoginUsername);
                    currentClientSocket.socket.Send(packet1);

                    //UPDATE STATE - Send data to client to change its state to 'Chatting'
                    byte[] packet2 = Encoding.ASCII.GetBytes("!changestate 1 ");
                    currentClientSocket.socket.Send(packet2);

                    //SHOW USER SUCCESS
                    byte[] packet3 = Encoding.ASCII.GetBytes("!success ");
                    currentClientSocket.socket.Send(packet3);

                    //notify all other clients of arrival
                    SendToAll(nl + "<< " + currentClientSocket.clientUserName + " has joined the chat >>", currentClientSocket);

                    // SHOW USER COMMANDS COMMAND
                    byte[] packet4 = Encoding.ASCII.GetBytes(nl + "< Type '!commands' to see all available commands >" +
                                                   nl + "-----------------------------------------------------------");
                    currentClientSocket.socket.Send(packet4);
                }
                else if(userExists && !passwordMatches)
                {
                    // wrong password
                    //tell server
                    AddToChat("< WRONG PASSWORD >");
                    //tell client
                    byte[] packet5 = Encoding.ASCII.GetBytes("< WRONG PASSWORD >");
                    currentClientSocket.socket.Send(packet5);
                }
                else // !user exists && addUser
                {
                    // No user exists
                    AddToChat("< NO USER EXISTS >" + nl + "...Adding User to DB...");
                    byte[] packet6 = Encoding.ASCII.GetBytes("< NO USER EXISTS >" + nl + "...Creating User in DB...");
                    currentClientSocket.socket.Send(packet6);

                    // add new user to DB
                    //Create new command for user addition
                    SQLiteCommand createUsercmd = new SQLiteCommand();
                    createUsercmd.Connection = connection;
                    createUsercmd.CommandText = string.Format("INSERT OR IGNORE INTO Users(Username, Password, Wins, Losses, Draws) " +
                                                              "VALUES('{0}','{1}', 0, 0, 0)", clientLoginUsername, clientPassword);
                    createUsercmd.ExecuteNonQuery(); // execute adding of new user
                    
                    //NOW CHECK IF IT WAS SUCCESSFULLY ADDED
                    createUsercmd.CommandText = "SELECT Username " +
                                                  "FROM Users";
                    SQLiteDataReader rdr2 = createUsercmd.ExecuteReader(); // run command for reader

                    bool clientAddedSuccess = false;
                    while (rdr2.Read()) // run through new command executed to find added user
                    {
                        // get username of current row data
                        string storedName = storedName = rdr2.GetString(0);

                        if (clientLoginUsername == storedName)
                        {
                            clientAddedSuccess = true; // the client was successfully added!
                        }
                    }

                    rdr2.Close();

                    if(clientAddedSuccess)
                    {
                        //continue log in process
                        //SET USERNAME TO CLIENTSOCKET
                        currentClientSocket.clientUserName = clientLoginUsername; // set usernme
                        currentClientSocket.state = ClientSocket.State.Chatting;

                        //UPDATE USER DISPLAY Send data to update display box for client Usernames
                        byte[] packet8 = Encoding.ASCII.GetBytes("!displayusername " + clientLoginUsername);
                        currentClientSocket.socket.Send(packet8);

                        //UPDATE STATE - Send data to client to change its state to 'Chatting'
                        byte[] packet9 = Encoding.ASCII.GetBytes("!changestate 1 ");
                        currentClientSocket.socket.Send(packet9);

                        //SHOW USER SUCCESS
                        byte[] packet10 = Encoding.ASCII.GetBytes("!success2 ");
                        currentClientSocket.socket.Send(packet10);

                        //notify all other clients of arrival
                        SendToAll(nl + "<< " + currentClientSocket.clientUserName + " has joined the chat >>", currentClientSocket);

                        // SHOW USER COMMANDS COMMAND
                        byte[] packet11 = Encoding.ASCII.GetBytes(nl + "< Type '!commands' to see all available commands >" +
                                                       nl + "-----------------------------------------------------------");
                        currentClientSocket.socket.Send(packet11);
                    }
                    else // remove DB addition
                    {
                        // User Creation Failed
                        AddToChat("< DB INSERT FAILED >");
                        byte[] packet7 = Encoding.ASCII.GetBytes("< USER CREATE FAILED >");
                        currentClientSocket.socket.Send(packet7);

                        //Remove failed user
                        SQLiteCommand removeUsercmd = new SQLiteCommand();
                        removeUsercmd.Connection = connection;
                        removeUsercmd.CommandText = string.Format("DELETE FROM Users " +
                                                                  "WHERE Username = '{0}' AND Password = '{1}'", clientLoginUsername, clientPassword);
                        removeUsercmd.ExecuteNonQuery(); // execute adding of new user
                    }
                }

                connection.Close(); // close up DB
            }
            else if (text.ToLower() == "!state")
            {
                byte[] data = Encoding.ASCII.GetBytes("You are in the '" +currentClientSocket.state.ToString() + "' state.");
                currentClientSocket.socket.Send(data);
            }
            else if (text.ToLower() == "!join")
            {
                bool player1Free = true;
                bool player2Free = true;

                //tell boards to reset
                //byte[] resetBoard = Encoding.ASCII.GetBytes("!updateboard ---------");

                //Loop through clientSockets to find availability in game!
                for (int i = 0; i < clientSockets.Count; ++i)
                {
                    //clientSockets[i].socket.Send(resetBoard); // send it to client to reset their boards
                    // if player one spot is taken
                    if (clientSockets[i].player == ClientSocket.Player.P1)
                    {
                        // move in
                        player1Free = false;
                    }
                    else if (clientSockets[i].player == ClientSocket.Player.P2)
                    {
                        // move on
                        player2Free = false;
                    }
                }

                if(player1Free & player2Free) // first to join. you'll be x's (go first)
                {
                    //set own player state to player 1
                    currentClientSocket.player = ClientSocket.Player.P1;
                    // set client isTurn to true
                    currentClientSocket.isTurn = true;
                    //update state
                    currentClientSocket.state = ClientSocket.State.Playing;

                    //alert client
                    byte[] data = Encoding.ASCII.GetBytes("!player1");
                    currentClientSocket.socket.Send(data);
                }
                else if(player2Free)
                {
                    //set own player state to player 2
                    currentClientSocket.player = ClientSocket.Player.P2;
                    // set client isTurn to true
                    currentClientSocket.isTurn = false;
                    //update state
                    currentClientSocket.state = ClientSocket.State.Playing;

                    //alert client
                    byte[] data = Encoding.ASCII.GetBytes("!player2");
                    currentClientSocket.socket.Send(data);
                }
                else //game is full
                {
                    //keep current state to Not Playing
                    currentClientSocket.player = ClientSocket.Player.NotPlaying;
                    // notify client that the game is full
                    byte[] data = Encoding.ASCII.GetBytes("!gamefull");
                    currentClientSocket.socket.Send(data);
                }
            }
            else if (text.ToLower() == "!move")
            {
                #region Move specific stuff
                //update board string with new position
                int index = Int32.Parse(boardIndex);
                board = board.Insert(index, playerSymbol); // insert symbol at location
                board = board.Remove(index + 1, 1); // remove previous symbol, which has now been pushed along

                //...now string is amended...
                //update the game grid
                ticTacToe.StringToGrid(board);

                //create gameboard string to send to clients
                string gameboard = ticTacToe.GridToString(); // pull data from tictactoe grid
                byte[] boardData = Encoding.ASCII.GetBytes("!updateboard " + gameboard);
                //run through clients and send them game data
                for(int i = 0; i < clientSockets.Count; i++)
                {
                    clientSockets[i].socket.Send(boardData);
                }

                //update server game grid
                for (int i = 0; i < ticTacToe.grid.Length; i++)
                {
                    char[] position = board.ToCharArray();
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

                    ticTacToe.grid[i] = tile; // set the grid
                }
                UpdateGameBoardText(gameboard); // update server board text
                #endregion

                #region Check For Wins
                //CHECK FOR WINS, now that grid and board is updated
                GameState gs = ticTacToe.GetGameState();
                if (gs == GameState.CrossWins) // --------------------------------------------------------------------- CROSSES WINS
                {
                    #region Reset Gameboard & Notify Winners
                    //find winner/loser usernames
                    string winnerName = "";
                    string loserName = "";
                    for (int i = 0; i < clientSockets.Count; i++)
                    {
                        if (clientSockets[i].player == ClientSocket.Player.P1) // X player
                            winnerName = clientSockets[i].clientUserName;
                        if (clientSockets[i].player == ClientSocket.Player.P2) // O player
                            loserName = clientSockets[i].clientUserName;
                    }

                    //Notify all of winner
                    SendToAll(nl + "<<< " + winnerName + " (Player 1) WINS >>>;", currentClientSocket);

                    // Reset Server board
                    ticTacToe.ResetBoard();

                    //TELL all to RESET boards
                    board = "---------"; // reset board and grid
                    string resetboard = ticTacToe.GridToString(); // pull data from tictactoe grid (should now be empty)
                    byte[] resetData = Encoding.ASCII.GetBytes("!updateboard " + resetboard + ";");
                    for (int i = 0; i < clientSockets.Count; i++)
                    {
                        clientSockets[i].socket.Send(resetData); // send it to client to reset their boards
                    }

                    //RESET all players State and Player status's
                    byte[] resetStateData = Encoding.ASCII.GetBytes("!changestate 1;");
                    for (int i = 0; i < clientSockets.Count; ++i)
                    {
                        //  -send command to reset CLIENT SIDE
                        if (clientSockets[i].state == ClientSocket.State.Playing)
                            clientSockets[i].socket.Send(resetStateData);
                        //  -Reset through all clients SERVER SIDE
                        if (clientSockets[i].state == ClientSocket.State.Playing)
                            clientSockets[i].state = ClientSocket.State.Chatting;
                        //RESET all players back to PLAYER status
                        if (clientSockets[i].player == ClientSocket.Player.P1)
                            clientSockets[i].player = ClientSocket.Player.NotPlaying;
                        else if (clientSockets[i].player == ClientSocket.Player.P2)
                            clientSockets[i].player = ClientSocket.Player.NotPlaying;
                    }

                    //Notify of state changes
                    SendToAll(nl + "...All players reverted to 'Chatting' state..." + nl +
                                   "< New game free to join! >; !xwins;", currentClientSocket);
                    #endregion

                    //Update DATABASE
                    string updateXConnectionString = "Data Source=TCPChatDB.db";
                    var updateXConnection = new SQLiteConnection(updateXConnectionString);
                    updateXConnection.Open();

                    //Winner DB additions
                    SQLiteCommand updateXCmd1 = new SQLiteCommand("", updateXConnection);
                    //construct win/losses ints
                    int xWins = 1;
                    int xlosses = 0;

                    //Get current data from DB and store wins and losses in ints
                    updateXCmd1.CommandText = string.Format("SELECT Wins, Losses " +
                                                           "FROM Users " +
                                                           "WHERE Username = '{0}'", winnerName);
                    SQLiteDataReader updateXRdr = updateXCmd1.ExecuteReader();// open data reader
                    while(updateXRdr.Read())
                    {
                        xWins += updateXRdr.GetInt32(0);
                        xlosses += updateXRdr.GetInt32(1);
                    }
                    updateXRdr.Close();

                    //Insert final data (add updated wins + losses)
                    updateXCmd1.CommandText = string.Format("UPDATE Users " +
                                         "SET Wins = '{0}', Losses = '{1}' " +
                                         "WHERE Username = '{2}'", xWins, xlosses, winnerName);

                    updateXCmd1.ExecuteNonQuery();
                    
                    

                    //Loser DB additions
                    SQLiteCommand updateXCmd2 = new SQLiteCommand("", updateXConnection);
                    int oWins = 0;
                    int oLosses = 1;

                    //Get current data from DB and store wins and losses in ints
                    updateXCmd2.CommandText = string.Format("SELECT Wins, Losses " +
                                                           "FROM Users " +
                                                           "WHERE Username = '{0}'", loserName);
                    SQLiteDataReader updateXRdr2 = updateXCmd2.ExecuteReader();// open data reader
                    while (updateXRdr2.Read())
                    {
                        oWins += updateXRdr2.GetInt32(0);
                        oLosses += updateXRdr2.GetInt32(1);
                    }
                    updateXRdr2.Close();

                    //Insert final data (add updated wins + losses)
                    updateXCmd2.CommandText = string.Format("UPDATE Users " +
                                         "SET Wins = '{0}', Losses = '{1}' " +
                                         "WHERE Username = '{2}'", oWins, oLosses, loserName);
                    updateXCmd2.ExecuteNonQuery();
                    

                    updateXConnection.Close(); //close DB and save

                    currentClientSocket.socket.BeginReceive(currentClientSocket.buffer,
                                            0,
                                            ClientSocket.BUFFER_SIZE,
                                            SocketFlags.None,
                                            ReceiveCallBack,
                                            currentClientSocket);
                    return;
                }
                else if (gs == GameState.NaughtWins) // --------------------------------------------------------------- NAUGHTS WINS
                {
                    //find winner/loser usernames
                    string winnerName = "";
                    string loserName = "";
                    for (int i = 0; i < clientSockets.Count; i++)
                    {
                        if (clientSockets[i].player == ClientSocket.Player.P2) // O player
                            winnerName = clientSockets[i].clientUserName;
                        if (clientSockets[i].player == ClientSocket.Player.P1) // X player
                            loserName = clientSockets[i].clientUserName;
                    }

                    //Notify all of winner
                    byte[] winData = Encoding.ASCII.GetBytes(nl + "<<< " + winnerName + " (Player 2) WINS >>>;");
                    for (int i = 0; i < clientSockets.Count; i++)
                    {
                        clientSockets[i].socket.Send(winData); // send it to client to reset their boards
                    }

                    //Notify of state changes
                    SendToAll(nl + "...All players reverted to 'Chatting' state..." + nl +
                                   "< New game free to join! >;!owins;", currentClientSocket);

                    // Reset Server board
                    ticTacToe.ResetBoard();

                    //TELL all to RESET boards
                    board = "---------"; // reset board and grid
                    string resetboard = ticTacToe.GridToString(); // pull data from tictactoe grid (should now be empty)
                    byte[] resetData = Encoding.ASCII.GetBytes("!updateboard " + resetboard + " ;");
                    for (int i = 0; i < clientSockets.Count; i++)
                    {
                        clientSockets[i].socket.Send(resetData); // send it to client to reset their boards
                    }

                    //RESET all players State and Player status's
                    byte[] resetStateData = Encoding.ASCII.GetBytes("!changestate 1 ;");
                    for (int i = 0; i < clientSockets.Count; ++i)
                    {
                        //  -send command to reset CLIENT SIDE
                        if (clientSockets[i].state == ClientSocket.State.Playing)
                            clientSockets[i].socket.Send(resetStateData);
                        //  -Reset through all clients SERVER SIDE
                        if (clientSockets[i].state == ClientSocket.State.Playing)
                            clientSockets[i].state = ClientSocket.State.Chatting;
                        //RESET all players back to PLAYER status
                        if (clientSockets[i].player == ClientSocket.Player.P1)
                            clientSockets[i].player = ClientSocket.Player.NotPlaying;
                        else if (clientSockets[i].player == ClientSocket.Player.P2)
                            clientSockets[i].player = ClientSocket.Player.NotPlaying;
                    }

                    //Update DATABASE
                    string updateOConnectionString = "Data Source=TCPChatDB.db";
                    var updateOConnection = new SQLiteConnection(updateOConnectionString);
                    updateOConnection.Open();
                    SQLiteCommand updateOCmd1 = new SQLiteCommand("", updateOConnection);

                    //Winner DB additions
                    {
                        //construct win/losses ints
                        int oWins = 1;
                        int oLosses = 0;

                        //Get current data from DB and store wins and losses in ints
                        updateOCmd1.CommandText = string.Format("SELECT Wins, Losses " +
                                                               "FROM Users " +
                                                               "WHERE Username = '{0}'", winnerName);
                        SQLiteDataReader updateORdr = updateOCmd1.ExecuteReader();// open data reader
                        while (updateORdr.Read())
                        {
                            oWins += updateORdr.GetInt32(0);
                            oLosses += updateORdr.GetInt32(1);
                        }
                        updateORdr.Close();

                        //Insert final data (add updated wins + losses)
                        updateOCmd1.CommandText = string.Format("UPDATE Users " +
                                             "SET Wins = '{0}', Losses = '{1}' " +
                                             "WHERE Username = '{2}'", oWins, oLosses, winnerName);

                        updateOCmd1.ExecuteNonQuery();
                    }

                    //Loser DB additions
                    {
                        //construct win/losses ints
                        int xWins = 0;
                        int xLosses = 1;

                        //Get current data from DB and store wins and losses in ints
                        updateOCmd1.CommandText = string.Format("SELECT Wins, Losses " +
                                                               "FROM Users " +
                                                               "WHERE Username = '{0}'", loserName);
                        SQLiteDataReader updateORdr2 = updateOCmd1.ExecuteReader();// open data reader
                        while (updateORdr2.Read())
                        {
                            xWins += updateORdr2.GetInt32(0);
                            xLosses += updateORdr2.GetInt32(1);
                        }
                        updateORdr2.Close();

                        //Insert final data (add updated wins + losses)
                        updateOCmd1.CommandText = string.Format("UPDATE Users " +
                                             "SET Wins = '{0}', Losses = '{1}' " +
                                             "WHERE Username = '{2}'", xWins, xLosses, loserName);
                        updateOCmd1.ExecuteNonQuery();
                    }

                    updateOConnection.Close(); //close DB and save

                    currentClientSocket.socket.BeginReceive(currentClientSocket.buffer,
                                            0,
                                            ClientSocket.BUFFER_SIZE,
                                            SocketFlags.None,
                                            ReceiveCallBack,
                                            currentClientSocket);
                    return;
                }
                else if (gs == GameState.Draw) //----------------------------------------------------------------------- DRAW!
                {
                    //find player usernames
                    string Player1 = "";
                    string Player2 = "";
                    for (int i = 0; i < clientSockets.Count; i++)
                    {
                        if (clientSockets[i].player == ClientSocket.Player.P1)
                            Player1 = clientSockets[i].clientUserName;
                        if (clientSockets[i].player == ClientSocket.Player.P2)
                            Player2 = clientSockets[i].clientUserName;
                    }

                    //Notify all of winner
                    SendToAll(nl + "<<< DRAW >>>;", currentClientSocket);

                    //Notify of state changes
                    SendToAll(nl + "...All players reverted to 'Chatting' state..." + nl +
                                   "< New game free to join! >;!draw;", currentClientSocket);

                    // Reset Server board
                    ticTacToe.ResetBoard();

                    //TELL all to RESET boards
                    board = "---------"; // reset board and grid
                    string resetboard = ticTacToe.GridToString(); // pull data from tictactoe grid (should now be empty)
                    byte[] resetData = Encoding.ASCII.GetBytes("!updateboard " + resetboard + ";");
                    for (int i = 0; i < clientSockets.Count; i++)
                    {
                        clientSockets[i].socket.Send(resetData); // send it to client to reset their boards
                    }

                    //RESET all players State and Player status's
                    byte[] resetStateData = Encoding.ASCII.GetBytes("!changestate 1;");
                    for (int i = 0; i < clientSockets.Count; ++i)
                    {
                        //  -send command to reset CLIENT SIDE
                        if (clientSockets[i].state == ClientSocket.State.Playing)
                            clientSockets[i].socket.Send(resetStateData);
                        //  -Reset through all clients SERVER SIDE
                        if (clientSockets[i].state == ClientSocket.State.Playing)
                            clientSockets[i].state = ClientSocket.State.Chatting;
                        //RESET all players back to PLAYER status
                        if (clientSockets[i].player == ClientSocket.Player.P1)
                            clientSockets[i].player = ClientSocket.Player.NotPlaying;
                        else if (clientSockets[i].player == ClientSocket.Player.P2)
                            clientSockets[i].player = ClientSocket.Player.NotPlaying;
                    }

                    //UPDATE DB
                    //Update DATABASE
                    string updateDConnectionString = "Data Source=TCPChatDB.db";
                    var updateDConnection = new SQLiteConnection(updateDConnectionString);
                    updateDConnection.Open();
                    SQLiteCommand updateDCmd1 = new SQLiteCommand("", updateDConnection);

                    //Player 1 (X) DB additions
                    {
                        //Get current data from DB and store draws
                        int xDraws = 1;
                        updateDCmd1.CommandText = string.Format("SELECT Draws " +
                                                               "FROM Users " +
                                                               "WHERE Username = '{0}'", Player1);
                        SQLiteDataReader updateDRdr = updateDCmd1.ExecuteReader();
                        while (updateDRdr.Read())
                        {
                            xDraws += updateDRdr.GetInt32(0);
                        }
                        updateDRdr.Close();

                        //Insert final data (add updated wins + losses)
                        updateDCmd1.CommandText = string.Format("UPDATE Users " +
                                             "SET Draws = '{0}' " +
                                             "WHERE Username = '{1}'", xDraws, Player1);

                        updateDCmd1.ExecuteNonQuery();
                    }

                    //Player 2 (O) DB additions
                    {
                        //Get current data from DB and store draws
                        int oDraws = 1;
                        updateDCmd1.CommandText = string.Format("SELECT Draws " +
                                                               "FROM Users " +
                                                               "WHERE Username = '{0}'", Player2);
                        SQLiteDataReader updateDRdr2 = updateDCmd1.ExecuteReader();
                        while (updateDRdr2.Read())
                        {
                            oDraws += updateDRdr2.GetInt32(0);
                        }
                        updateDRdr2.Close();

                        //Insert final data (add updated wins + losses)
                        updateDCmd1.CommandText = string.Format("UPDATE Users " +
                                             "SET Draws = '{0}' " +
                                             "WHERE Username = '{1}'", oDraws, Player2);

                        updateDCmd1.ExecuteNonQuery();
                    }

                    updateDConnection.Close(); //close DB and save

                    currentClientSocket.socket.BeginReceive(currentClientSocket.buffer,
                                            0,
                                            ClientSocket.BUFFER_SIZE,
                                            SocketFlags.None,
                                            ReceiveCallBack,
                                            currentClientSocket);
                    return;
                }
                #endregion

                //tells whoevers turn it is that it is their go -------------------------------------UPDATE TURN
                for (int i = 0; i < clientSockets.Count; i ++)
                {
                    // check if they are player 2 & they haven't just had their turn
                    if(clientSockets[i].player == ClientSocket.Player.P2 &&
                       clientSockets[i].isTurn == false)
                    {
                        // set their go.
                        clientSockets[i].isTurn = true;
                        byte[] data = Encoding.ASCII.GetBytes("!updateturn");
                        clientSockets[i].socket.Send(data);

                        //run through clients to find player 1
                        for (int j = 0; j < clientSockets.Count; j++)
                        {
                            //set player one justHadTurn bool back to false!
                            if (clientSockets[j].player == ClientSocket.Player.P1)
                            {
                                clientSockets[j].isTurn = false;
                                break; // leave inner loop, we'eve found P1
                            }
                        }
                        break; // Leave outer loop P2's turn has been updated
                    } 
                    else if(clientSockets[i].player == ClientSocket.Player.P1 &&
                            clientSockets[i].isTurn == false)
                    {
                        clientSockets[i].isTurn = true;
                        byte[] data = Encoding.ASCII.GetBytes("!updateturn");
                        clientSockets[i].socket.Send(data);

                        //run through clients to find player 1
                        for (int j = 0; j < clientSockets.Count; j++)
                        {
                            //set player one justHadTurn bool back to false!
                            if (clientSockets[j].player == ClientSocket.Player.P2)
                            {
                                //clientSockets[j].justHadTurn = true;
                                clientSockets[j].isTurn = false;
                                break; // leave inner loop, we'eve found P2
                            }
                        }
                        break; // Leave outer loop P1's turn has been updated
                    }
                }
            }
            else if (text.ToLower() == "!scores") //TODO display Scores for TIC TAC TOE from DB
            {
                // dump all data from users, wins, loses columns to chatbox
            }
            else if (text.Contains("!deleteDB")) // DANGER !! Debug Only...
            {
                string connectionString = "Data Source=TCPChatDB.db";
                var connection = new SQLiteConnection(connectionString);
                connection.Open();
                string sqlCommandText = "DROP TABLE Users";
                SQLiteCommand cmd = new SQLiteCommand(sqlCommandText, connection);
                cmd.ExecuteNonQuery();
            }
            else if (text.ToLower() == "!user")
            {
                byte[] data = Encoding.ASCII.GetBytes(" ");
                string temp = currentClientSocket.clientUserName;
                bool canChangeName = false;

                for (int i = 0; i < clientSockets.Count; ++i)
                {
                    if (clientSockets[i].clientUserName != changeNameUserName)
                    {
                        canChangeName = true;
                    }
                    else // user already exists
                    {
                        data = Encoding.ASCII.GetBytes(nl + "< Cannot Change Username >" +
                                                        nl + "User: " + changeNameUserName + " already exists.");
                        canChangeName = false;
                        break;
                    }
                }

                if (canChangeName)
                {
                    currentClientSocket.clientUserName = changeNameUserName;
                    data = Encoding.ASCII.GetBytes("!displayusername " + changeNameUserName);
                    currentClientSocket.socket.Send(data);

                    SendToAll(nl + "..." + temp + " has transformed..." + nl +
                              "They shall now be know as: '" + currentClientSocket.clientUserName + "'" + nl,
                              currentClientSocket);
                }
                else
                {
                    currentClientSocket.socket.Send(data);
                }

                data = Encoding.ASCII.GetBytes(nl + "-----------------------------------------------------------");
                currentClientSocket.socket.Send(data);
            }
            else if (text.ToLower() == "!magic")
            {
                // User has shaken the Magic 8 Ball and whispered their deepest desires into it glossy black shell...
                string[] phrases = { "It is certain.", "It is decidedly so.", "Without a doubt.", "Yes, definitely.", "You may rely on it.",
                                     "As I see it, yes.", "Most likely.", "Outlook looks good.", "Yes.", "Signs point to yes.",
                                     "Reply hazy, try again.", "Ask again later.", "Better not tell you now.", "Cannot predict now.", "Concentrate and ask again.",
                                     "Don't count on it.", "My reply is no.", "My sources say no.", "Outlook looks bleak.", "Very doubtful.",};
                
                Random rnd = new Random();
                int index = rnd.Next(0, 19);

                byte[] data = Encoding.ASCII.GetBytes(nl + "Your Question --->" + magicQuestion + 
                                                      nl + "My Divine Answer --->" + phrases[index]);
                //concoct message based on answer
                string messageToAll = "";
                if(index >= 0 && index <= 9) // positive message
                {
                    messageToAll = nl + currentClientSocket.clientUserName + " has asked the Magic 8 Ball a question!" + nl + "...things are in their favour...";
                }
                else if(index >= 10 && index <= 14) // uncertain message
                {
                    messageToAll = nl + currentClientSocket.clientUserName + " has asked the Magic 8 Ball a question!" + nl + "...things are questionable...";
                }
                else if(index >= 15 && index <= 19) // Negative message
                {
                    messageToAll = nl + currentClientSocket.clientUserName + " has asked the Magic 8 Ball a question!" + nl + "...misfortune befalls them...";
                }

                SendToAll(messageToAll, currentClientSocket);
                currentClientSocket.socket.Send(data);
                data = Encoding.ASCII.GetBytes(nl + "-----------------------------------------------------------");
                currentClientSocket.socket.Send(data);
            }
            else if (text.ToLower() == "!kick")
            {
                if(currentClientSocket.isModerator == true)
                {
                    try
                    {
                        byte[] data = Encoding.ASCII.GetBytes("!exit");
                        for (int i = 0; i < clientSockets.Count; ++i)
                        {
                            if (userToKick == clientSockets[i].clientUserName)
                            {
                                clientSockets[i].socket.Send(data);
                                SendToAll(nl + "<< " + userToKick + " was kicked from the chat by Moderator " + currentClientSocket.clientUserName + " >>", currentClientSocket);
                                AddToChat("<< Client " + userToKick + " Disconnected by " + currentClientSocket.clientUserName + " >>");

                                clientSockets[i].socket.Shutdown(SocketShutdown.Both);
                                clientSockets[i].socket.Close();
                                clientSockets.Remove(clientSockets[i]);
                                break; // as now the clientSockets.count has been adjusted
                            }
                        }
                    }
                    catch (ObjectDisposedException er)
                    {
                        AddToChat(nl+ "Error: " + er.Message + nl + nl);
                    }
                }
                else // No go, Pablo
                {
                    byte[] data = Encoding.ASCII.GetBytes(nl + "< You do NOT have Moderator privileges >" + nl);
                    currentClientSocket.socket.Send(data);
                }
            }
            else if (text.ToLower() == "!exit") // client wants to exit gracefully...
            {
                byte[] data = Encoding.ASCII.GetBytes("!exit");
                currentClientSocket.socket.Send(data);
                SendToAll(nl + "<< " + currentClientSocket.clientUserName + " has left the chat >>", currentClientSocket);

                currentClientSocket.socket.Shutdown(SocketShutdown.Both);
                currentClientSocket.socket.Close();
                clientSockets.Remove(currentClientSocket);
                AddToChat("<< Client Disconnected >>");

                if (text.ToLower() == "!exit" && clientSockets.Count <= 0) // all clients disconnected
                {
                    IndentIcon(); // change server Icon Identation
                }

                return; // bail early, rest of function not useful for !exit
            }
            else // no command, REGULAR CHAT MESSAGE
            {
                SendToAll(currentClientSocket.clientUserName + ": " + text, currentClientSocket);
            }

            currentClientSocket.socket.BeginReceive(currentClientSocket.buffer,
                                                    0,                         
                                                    ClientSocket.BUFFER_SIZE,  
                                                    SocketFlags.None,          
                                                    ReceiveCallBack,           
                                                    currentClientSocket);     
        }

        // function for server to send messages out to all clients
        // i.e 'from' says "Hello", server broadcasts to the other clients
        public void SendToAll(string str, ClientSocket from)
        {
            foreach(ClientSocket clientSocket in clientSockets)
            {
                if(from == null || !from.socket.Equals(clientSocket))
                {
                    byte[] data = Encoding.ASCII.GetBytes(str);
                    clientSocket.socket.Send(data);
                }
            }
        }
    }
}
