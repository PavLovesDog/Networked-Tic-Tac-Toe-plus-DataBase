
namespace NDS_Networking_Project
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.HostPortLabel = new System.Windows.Forms.Label();
            this.HostPortTextBox = new System.Windows.Forms.TextBox();
            this.HostServerButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.ServerPortLabel = new System.Windows.Forms.Label();
            this.ServerPortTextBox = new System.Windows.Forms.TextBox();
            this.ServerIPLabel = new System.Windows.Forms.Label();
            this.ServerIPTextBox = new System.Windows.Forms.TextBox();
            this.JoinServerButton = new System.Windows.Forms.Button();
            this.ChatTextBox = new System.Windows.Forms.TextBox();
            this.ChatLabel = new System.Windows.Forms.Label();
            this.TypeTextBox = new System.Windows.Forms.TextBox();
            this.SendButton = new System.Windows.Forms.Button();
            this.LogoPicBox = new System.Windows.Forms.PictureBox();
            this.ClientUsernameTextBox = new System.Windows.Forms.TextBox();
            this.ClientUsernameLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.LogoPicBox)).BeginInit();
            this.SuspendLayout();
            // 
            // HostPortLabel
            // 
            this.HostPortLabel.AutoSize = true;
            this.HostPortLabel.BackColor = System.Drawing.Color.Transparent;
            this.HostPortLabel.Font = new System.Drawing.Font("Gill Sans Nova Light", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.HostPortLabel.Location = new System.Drawing.Point(59, 6);
            this.HostPortLabel.Name = "HostPortLabel";
            this.HostPortLabel.Size = new System.Drawing.Size(79, 23);
            this.HostPortLabel.TabIndex = 0;
            this.HostPortLabel.Text = "Host Port";
            this.HostPortLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // HostPortTextBox
            // 
            this.HostPortTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.HostPortTextBox.Location = new System.Drawing.Point(31, 32);
            this.HostPortTextBox.Name = "HostPortTextBox";
            this.HostPortTextBox.Size = new System.Drawing.Size(139, 27);
            this.HostPortTextBox.TabIndex = 1;
            this.HostPortTextBox.Text = "6666";
            this.HostPortTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // HostServerButton
            // 
            this.HostServerButton.BackColor = System.Drawing.Color.LightSkyBlue;
            this.HostServerButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.HostServerButton.Font = new System.Drawing.Font("Comic Sans MS", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.HostServerButton.Location = new System.Drawing.Point(48, 65);
            this.HostServerButton.Name = "HostServerButton";
            this.HostServerButton.Size = new System.Drawing.Size(103, 47);
            this.HostServerButton.TabIndex = 2;
            this.HostServerButton.Text = "Host Server";
            this.HostServerButton.UseVisualStyleBackColor = false;
            this.HostServerButton.Click += new System.EventHandler(this.HostServerButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(188, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 20);
            this.label1.TabIndex = 3;
            this.label1.Text = "OR";
            // 
            // ServerPortLabel
            // 
            this.ServerPortLabel.AutoSize = true;
            this.ServerPortLabel.BackColor = System.Drawing.Color.Transparent;
            this.ServerPortLabel.Font = new System.Drawing.Font("Gill Sans Nova Light", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.ServerPortLabel.Location = new System.Drawing.Point(259, 6);
            this.ServerPortLabel.Name = "ServerPortLabel";
            this.ServerPortLabel.Size = new System.Drawing.Size(88, 23);
            this.ServerPortLabel.TabIndex = 4;
            this.ServerPortLabel.Text = "Server Port";
            // 
            // ServerPortTextBox
            // 
            this.ServerPortTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ServerPortTextBox.Location = new System.Drawing.Point(234, 32);
            this.ServerPortTextBox.Name = "ServerPortTextBox";
            this.ServerPortTextBox.Size = new System.Drawing.Size(133, 27);
            this.ServerPortTextBox.TabIndex = 5;
            this.ServerPortTextBox.Text = "6666";
            this.ServerPortTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // ServerIPLabel
            // 
            this.ServerIPLabel.AutoSize = true;
            this.ServerIPLabel.BackColor = System.Drawing.Color.Transparent;
            this.ServerIPLabel.Font = new System.Drawing.Font("Gill Sans Nova Light", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.ServerIPLabel.Location = new System.Drawing.Point(439, 9);
            this.ServerIPLabel.Name = "ServerIPLabel";
            this.ServerIPLabel.Size = new System.Drawing.Size(66, 23);
            this.ServerIPLabel.TabIndex = 6;
            this.ServerIPLabel.Text = "ServerIP";
            // 
            // ServerIPTextBox
            // 
            this.ServerIPTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ServerIPTextBox.Location = new System.Drawing.Point(439, 32);
            this.ServerIPTextBox.Name = "ServerIPTextBox";
            this.ServerIPTextBox.Size = new System.Drawing.Size(264, 27);
            this.ServerIPTextBox.TabIndex = 7;
            this.ServerIPTextBox.Text = "127.0.0.1";
            // 
            // JoinServerButton
            // 
            this.JoinServerButton.BackColor = System.Drawing.Color.DarkSalmon;
            this.JoinServerButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.JoinServerButton.Font = new System.Drawing.Font("Comic Sans MS", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.JoinServerButton.Location = new System.Drawing.Point(248, 65);
            this.JoinServerButton.Name = "JoinServerButton";
            this.JoinServerButton.Size = new System.Drawing.Size(99, 47);
            this.JoinServerButton.TabIndex = 8;
            this.JoinServerButton.Text = "Join Server";
            this.JoinServerButton.UseVisualStyleBackColor = false;
            this.JoinServerButton.Click += new System.EventHandler(this.JoinServerButton_Click);
            // 
            // ChatTextBox
            // 
            this.ChatTextBox.Location = new System.Drawing.Point(31, 134);
            this.ChatTextBox.Multiline = true;
            this.ChatTextBox.Name = "ChatTextBox";
            this.ChatTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.ChatTextBox.Size = new System.Drawing.Size(672, 244);
            this.ChatTextBox.TabIndex = 9;
            // 
            // ChatLabel
            // 
            this.ChatLabel.AutoSize = true;
            this.ChatLabel.BackColor = System.Drawing.Color.LightGreen;
            this.ChatLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ChatLabel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ChatLabel.Font = new System.Drawing.Font("Gill Sans Nova Light", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.ChatLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.ChatLabel.Location = new System.Drawing.Point(31, 398);
            this.ChatLabel.Name = "ChatLabel";
            this.ChatLabel.Size = new System.Drawing.Size(60, 28);
            this.ChatLabel.TabIndex = 10;
            this.ChatLabel.Text = "Chat: ";
            // 
            // TypeTextBox
            // 
            this.TypeTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TypeTextBox.Location = new System.Drawing.Point(97, 398);
            this.TypeTextBox.Name = "TypeTextBox";
            this.TypeTextBox.Size = new System.Drawing.Size(488, 27);
            this.TypeTextBox.TabIndex = 11;
            // 
            // SendButton
            // 
            this.SendButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.SendButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SendButton.Location = new System.Drawing.Point(609, 397);
            this.SendButton.Name = "SendButton";
            this.SendButton.Size = new System.Drawing.Size(94, 30);
            this.SendButton.TabIndex = 12;
            this.SendButton.Text = "Send";
            this.SendButton.UseVisualStyleBackColor = false;
            this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
            // 
            // LogoPicBox
            // 
            this.LogoPicBox.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.LogoPicBox.BackgroundImage = global::NDS_Networking_Project.Properties.Resources.avatarLogo;
            this.LogoPicBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.LogoPicBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LogoPicBox.Location = new System.Drawing.Point(718, 134);
            this.LogoPicBox.Name = "LogoPicBox";
            this.LogoPicBox.Size = new System.Drawing.Size(89, 74);
            this.LogoPicBox.TabIndex = 13;
            this.LogoPicBox.TabStop = false;
            // 
            // ClientUsernameTextBox
            // 
            this.ClientUsernameTextBox.Location = new System.Drawing.Point(439, 85);
            this.ClientUsernameTextBox.Name = "ClientUsernameTextBox";
            this.ClientUsernameTextBox.Size = new System.Drawing.Size(154, 27);
            this.ClientUsernameTextBox.TabIndex = 14;
            // 
            // ClientUsernameLabel
            // 
            this.ClientUsernameLabel.AutoSize = true;
            this.ClientUsernameLabel.BackColor = System.Drawing.Color.Transparent;
            this.ClientUsernameLabel.Font = new System.Drawing.Font("Gill Sans Nova Light", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.ClientUsernameLabel.Location = new System.Drawing.Point(439, 60);
            this.ClientUsernameLabel.Name = "ClientUsernameLabel";
            this.ClientUsernameLabel.Size = new System.Drawing.Size(121, 22);
            this.ClientUsernameLabel.TabIndex = 15;
            this.ClientUsernameLabel.Text = "Client Username";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::NDS_Networking_Project.Properties.Resources.MSNbackground;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1208, 455);
            this.Controls.Add(this.ClientUsernameLabel);
            this.Controls.Add(this.ClientUsernameTextBox);
            this.Controls.Add(this.LogoPicBox);
            this.Controls.Add(this.SendButton);
            this.Controls.Add(this.TypeTextBox);
            this.Controls.Add(this.ChatLabel);
            this.Controls.Add(this.ChatTextBox);
            this.Controls.Add(this.JoinServerButton);
            this.Controls.Add(this.ServerIPTextBox);
            this.Controls.Add(this.ServerIPLabel);
            this.Controls.Add(this.ServerPortTextBox);
            this.Controls.Add(this.ServerPortLabel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.HostServerButton);
            this.Controls.Add(this.HostPortTextBox);
            this.Controls.Add(this.HostPortLabel);
            this.Name = "Form1";
            this.Text = "Chat Application";
            ((System.ComponentModel.ISupportInitialize)(this.LogoPicBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label HostPortLabel;
        private System.Windows.Forms.TextBox HostPortTextBox;
        private System.Windows.Forms.Button HostServerButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label ServerPortLabel;
        private System.Windows.Forms.TextBox ServerPortTextBox;
        private System.Windows.Forms.Label ServerIPLabel;
        private System.Windows.Forms.TextBox ServerIPTextBox;
        private System.Windows.Forms.Button JoinServerButton;
        private System.Windows.Forms.TextBox ChatTextBox;
        private System.Windows.Forms.Label ChatLabel;
        private System.Windows.Forms.TextBox TypeTextBox;
        private System.Windows.Forms.Button SendButton;
        private System.Windows.Forms.PictureBox LogoPicBox;
        private System.Windows.Forms.TextBox ClientUsernameTextBox;
        private System.Windows.Forms.Label ClientUsernameLabel;
    }
}

