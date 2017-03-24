using Newtonsoft.Json.Linq;
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
            Player1Words.Text = "";
            Player2Words.Text = "";
            foreach (JToken y in playerOneWords)
            {
                
                Player1Words.AppendText(y["Word"].ToString());
                Player1Words.AppendText("    ");
                Player1Words.AppendText(y["Score"].ToString());
                Player1Words.AppendText("\n");

            }  
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
        public void updateView(string p1Score, string p2Score, string timeLeft, string p1Name, string p2Name)
        {
            Timer.Text = timeLeft;
            Player1Nick.Text = p1Name;
            Player2Nick.Text = p2Name;
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

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string line1 = "Enter your name and server address and click register to get started\n";
            string line2 = "Click Join game and waint until the server finds someone to pair you against\n";
            string line3 = "Type words in the bottom text box and hit enter to add them\n";
            string line4 = "At the end of the game, click cancel with you are finished looking at the scores\n";
            string line5 = "Then just click to join a new game to start again!\n";
            string line6 = "Your words may show up in the other players box as you enter them but at the end of the screen\n";
            string line7 = "They will be placed correctly. These are just a refrence so you know how well you are doing";
            MessageBox.Show(line1 + line2 + line3 + line4 + line5+line6+line7);
        }
    }
}
