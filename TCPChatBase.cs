using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms; // to access UI elements

namespace NDS_Networking_Project
{
    public class TCPChatBase
    {
        // create quick access for new line in textbox, as "\n" doesn't seem to be working...
        public Object nl = Environment.NewLine;

        public List<Button> buttons; // refernece to tictactoe buttons
        public Label playerTurnLabel;
        public PictureBox logoPicBox;
        public TextBox clientUsernameTextBox;
        public TextBox chatTextBox; // to access main chat text box in app
        public int port; //when listenning for data, need port open

        //TODO When debugging SERVER SIDE as the HOST, an error saying the buttons accessed from another thread other than what it was created on throws.
        public void UpdateGameBoardText(string s)
        {
            char[] position = s.ToCharArray();
            for (int i = 0; i < 9; ++i)
            {
                buttons[i].Invoke((Action)delegate
                {
                    if(position[i].ToString() == "-")
                    {
                        buttons[i].Text = ""; //keep empty
                    }
                    else
                    {
                        buttons[i].Text = position[i].ToString(); // paint the rest
                    }
                });
            }
        }

        public void updateTurnLabel(bool reset)
        {
            if(!reset)
            {
                playerTurnLabel.Invoke((Action)delegate
                {
                    if (playerTurnLabel.Text == "X's Turn...")
                    {
                        playerTurnLabel.Text = "O's Turn...";
                    }
                    else if (playerTurnLabel.Text == "O's Turn...")
                    {
                        playerTurnLabel.Text = "X's Turn...";
                    }
                });
            }
            else
            {
                playerTurnLabel.Invoke((Action)delegate
                {
                    playerTurnLabel.Text = "X's Turn...";
                });
            }
        }

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
            //Send message from this thread to main thread, update chatTextBox
            chatTextBox.Invoke((Action)delegate 
            {
                chatTextBox.Text = str; 
                chatTextBox.AppendText(Environment.NewLine);
            
            });
        }

        // function to add a passed string to server chatbox
        public void AddToChat(string str)
        {

            chatTextBox.Invoke((Action)delegate
            {
                chatTextBox.AppendText(str); 
                chatTextBox.AppendText(Environment.NewLine);
            });
        }
    }
}
