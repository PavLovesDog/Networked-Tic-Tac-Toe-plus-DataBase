using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms; // to access UI elements

namespace NDS_Networking_Project
{
    public class TCPChatBase : TicTacToe
    {
        // create quick access for new line in textbox, as "\n" doesn't seem to be working...
        public Object nl = Environment.NewLine;

        public PictureBox logoPicBox;
        public TextBox clientUsernameTextBox;
        public TextBox chatTextBox; // to access main chat text box in app
        public int port; //when listenning for data, need port open

        // Function to control borderstyle of icon
        public void IndentIcon()
        {
            logoPicBox.Invoke((Action)delegate
            {
                if (logoPicBox.BorderStyle == BorderStyle.FixedSingle)
                {
                    logoPicBox.BorderStyle = BorderStyle.Fixed3D;
                }
                else if (logoPicBox.BorderStyle == BorderStyle.Fixed3D)
                {
                    logoPicBox.BorderStyle = BorderStyle.FixedSingle;
                }
            });
        }

        // Functions to help work with the chat text box
        public void SetChat(string str)
        {
            /* NOTE As chat text box is running in MAIN thread, we need to do the following so it is
             * accessible from whatever thread this class is running on...
             * Invoke is to send out a message to the textbox which it will receive and act on */

            //Send message from this thread to main thread, update chatTextBox
            chatTextBox.Invoke((Action)delegate 
            {
                // clear txt box screen, set it to string passed in
                chatTextBox.Text = str; 
                chatTextBox.AppendText(Environment.NewLine);
            
            });
        }

        // function to add a passed string to server chatbox
        public void AddToChat(string str)
        {

            chatTextBox.Invoke((Action)delegate
            {
                // add message to text box screen
                chatTextBox.AppendText(str); 
                chatTextBox.AppendText(Environment.NewLine);
            });
        }
    }
}
