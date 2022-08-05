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
                tcp.clientSocket.state = ClientSocket.State.Login; // set state to Login state upon connection
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

            //AddToChat("<< Connected >>");
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

            // Store username data for display
            string tempUserName = "";
            string stateEnum = "";
            if (text.Contains("!displayusername "))
            {
                // create string to hold the username data
                tempUserName = text.Remove(0, 17);
                text = text.Remove(16, text.Length - 16);
            }
            else if(text.Contains("!changestate ")) //TODO CHANGE STATE COMMAND AMMENDENT
            {
                // seperate string data and assign correctly
                string[] subStrings = text.Split(' ');
                
                stateEnum = subStrings[1].Remove(1);
                text = subStrings[0];
            }

            // Reaction Commands --------------------------------------------------------
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
                    clientSocket.state = ClientSocket.State.Chatting;
                }
                else if (stateEnum == "2")
                {
                    clientSocket.state = ClientSocket.State.Playing;
                }
            }
            else // regular chat message!
            {
                AddToChat(text);
            }
            // -------------------------------------------------------- Reaction Commands

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
