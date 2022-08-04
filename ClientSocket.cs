using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets; // to access the .NET core socket

namespace NDS_Networking_Project
{
    // Represents a client
    public class ClientSocket
    {
        public enum State
        {
            Login,
            Chatting,
            Playing
        }

        public State state;
        public string clientUserName;
        public bool isModerator;
        public bool isConnected;

        public Socket socket; //port and IP address
        public const int BUFFER_SIZE = 2048;
        public byte[] buffer = new byte[BUFFER_SIZE]; //byte aray like a string, data we want to send and receive

    }
}
