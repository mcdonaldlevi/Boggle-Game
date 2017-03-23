﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BoggleClient
{
    public partial class Form1 : Form
    {
        private Timer myTime = new Timer();
        private List<TextBox> displayBoxes; 
        public Form1()
        {
            
            InitializeComponent();
            makeTextBoxDisplay();
            myTime.Interval = 1000;
            myTime.Start();
            myTime.Enabled = false;
            myTime.Tick += MyTime_Tick;
        }

        public event Action<string, string> RegisterPressed;
        public event Action<string, string> WordEntered;
        public event Action<int, string> JoinGamePressed;
        public event Action<string> CancelButtonPressed;
        public event Action<string> GameStatusRequest;
        
        public void GameEndScreen(JToken playerOneWords, JToken playerTwoWords)
        {
          foreach(JToken x in playerTwoWords)
            {
                Player2Words.AppendText(x["Word"].ToString());
                Player2Words.AppendText("    ");
                Player2Words.AppendText(x["Score"].ToString());
                Player2Words.AppendText("\n");
            }
            myTime.Enabled = false;
        }
        public void updateWordBox(string wordScore)
        {
            Player1Words.AppendText(WordInputBox.Text + "     " + wordScore + "\n");
            WordInputBox.Text = "";
        }
        public void updateView(string p1Score, string p2Score, string timeLeft)
        {
            Timer.Text = timeLeft;
            Player1ScoreBox.Text = p1Score;
            Player2ScoreBox.Text = p2Score;
        }
        public void startTime()
        {            
            myTime.Enabled = true;                 
        }

        private void MyTime_Tick(object sender, EventArgs e)
        {
            myTime.Enabled = false;
            GameStatusRequest(ServerAddressBox.Text);
            myTime.Enabled = true;
        }
        private void makeTextBoxDisplay()
        {
            List<TextBox> textBoxes = new List<TextBox> { textBox2, textBox3, textBox4, textBox5, textBox6, textBox7, textBox8, textBox9, textBox10,
                textBox11,textBox12,textBox13,textBox14,textBox15,textBox16,textBox17 };
            displayBoxes = textBoxes;
        }
        private void RegisterButton_Click(object sender, EventArgs e)
        {
            RegisterPressed(NameBox.Text, ServerAddressBox.Text);
        }

        private void JoinGameButton_Click(object sender, EventArgs e)
        {
            JoinGamePressed(int.Parse(TimeBox.Text), ServerAddressBox.Text);
        }

        private void WordInputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                WordEntered(ServerAddressBox.Text, WordInputBox.Text);
            }
        }
        private void CancelButton_Click(object sender, EventArgs e)
        {
            CancelButtonPressed(ServerAddressBox.Text);
        }

        public void displayLetters(string board)
        {
            int count = 0;
            foreach(TextBox y in displayBoxes)
            {
                y.Text = board[count].ToString();
                count += 1;
            }
        }
    }
}
