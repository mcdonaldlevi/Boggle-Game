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
        public Form1()
        {
            InitializeComponent();
        }

        public event Action<string, string> RegisterPressed;

        public event Action WordEntered;

        public event Action GameOver;
        public event Action<int, string> JoinGamePressed;

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
                WordEntered
            }
        }
    }
}
