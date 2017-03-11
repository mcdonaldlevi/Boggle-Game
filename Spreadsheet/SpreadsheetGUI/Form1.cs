
using SSGui;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpreadsheetGUI
{
    public partial class Form1 : Form, ISpreadsheet
    {
        public Form1()
        {
            InitializeComponent();
            spreadsheetPanel1.SelectionChanged += SpreadsheetPanel1_SelectionChanged;
            spreadsheetPanel1.SetSelection(2, 3);
        }

        private void SpreadsheetPanel1_SelectionChanged(SpreadsheetPanel sender)
        {
            TextFinished(textBox1.Text);            
        }

        public event Action NewEvent;
        public event Action<string> TypeText;
        public event Action<string> TextFinished;
        public event Action<Stream> saveSpreadsheet;

        public void setCellValue(int col, int row, string value)
        {
            spreadsheetPanel1.SetValue(col, row, value);
        }
        public void setCellNameValue(string name, string value)
        {
            CellName.Text = name;
            CellValue.Text = value;
        }
        public int[] GetCoord()
        {
            int col, row;
            spreadsheetPanel1.GetSelection(out col, out row);
            int[] coordinates = new int[] { col, row };
            return coordinates;
        }
        public void setTextBoxValue(string value)
        {
            textBox1.Text = value;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            TypeText(textBox1.Text);
        }

        private void newFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewEvent();
        }

        public void OpenNew()
        {
            SpreadsheetContext.GetContext().RunNew(null);
        }

        public void clearTextBox()
        {
            textBox1.Text = "";
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == Convert.ToChar(Keys.Enter))
            {
                TextFinished(textBox1.Text);
            }
            
            else
            {
                TypeText(textBox1.Text);
            }
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == Convert.ToChar(Keys.Left))
            {
                TextFinished(textBox1.Text);
                int col, row;
                spreadsheetPanel1.GetSelection(out col, out row);
                if (col > 0)
                {
                    col -= 1;
                }
                spreadsheetPanel1.SetSelection(col, row);
            }
            else if (e.KeyChar == Convert.ToChar(Keys.Right))
            {
                TextFinished(textBox1.Text);
                int col, row;
                spreadsheetPanel1.GetSelection(out col, out row);
                if (col < 25)
                {
                    col += 1;
                }
                spreadsheetPanel1.SetSelection(col, row);
            }
            else if (e.KeyChar == Convert.ToChar(Keys.Up))
            {
                TextFinished(textBox1.Text);
                int col, row;
                spreadsheetPanel1.GetSelection(out col, out row);
                if (row > 0)
                {
                    col -= 1;
                }
                spreadsheetPanel1.SetSelection(col, row);
            }
            else if (e.KeyChar == Convert.ToChar(Keys.Left))
            {
                TextFinished(textBox1.Text);
                int col, row;
                spreadsheetPanel1.GetSelection(out col, out row);
                if (row < 99)
                {
                    col += 1;
                }
                spreadsheetPanel1.SetSelection(col, row);
            }
        }

        private void spreadsheetPanel1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == Convert.ToChar(Keys.Left))
            {
                TextFinished(textBox1.Text);
                int col, row;
                spreadsheetPanel1.GetSelection(out col, out row);
                if (col > 0)
                {
                    col -= 1;
                }
                spreadsheetPanel1.SetSelection(col, row);
            }
            else if (e.KeyValue == Convert.ToChar(Keys.Right))
            {
                TextFinished(textBox1.Text);
                int col, row;
                spreadsheetPanel1.GetSelection(out col, out row);
                if (col < 25)
                {
                    col += 1;
                }
                spreadsheetPanel1.SetSelection(col, row);
            }
            else if (e.KeyValue == Convert.ToChar(Keys.Up))
            {
                TextFinished(textBox1.Text);
                int col, row;
                spreadsheetPanel1.GetSelection(out col, out row);
                if (row > 0)
                {
                    col -= 1;
                }
                spreadsheetPanel1.SetSelection(col, row);
            }
            else if (e.KeyValue == Convert.ToChar(Keys.Left))
            {
                TextFinished(textBox1.Text);
                int col, row;
                spreadsheetPanel1.GetSelection(out col, out row);
                if (row < 99)
                {
                    col += 1;
                }
                spreadsheetPanel1.SetSelection(col, row);
            }
        }

       /* private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.ShowDialog();
            if (saveFileDialog1.FileName != "")
            {
                System.IO.StreamWriter fs =
                   saveFileDialog1.OpenFile();
                
                saveSpreadsheet(fs);
                fs.Close();
            }
            
        }*/

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.IO.StreamReader sr = new
                   System.IO.StreamReader(openFileDialog1.FileName);
                SpreadsheetContext.GetContext().RunNew(sr);
                sr.Close();
            }
        }
    }
}
