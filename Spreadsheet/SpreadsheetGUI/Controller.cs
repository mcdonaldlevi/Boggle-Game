using SS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpreadsheetGUI
{
    public class Controller
    {
        // The window being controlled
        private ISpreadsheet window;

        // The Spreadsheet being used
        private Spreadsheet spreadsheet;
        /// <summary>
        /// maps numbers to strings for easy refrence
        /// </summary>
        private Dictionary<int, string> valueToLetterMap = new Dictionary<int, string>
        {
            {0, "A" }, {1,"B" }, {2,"C" }, {3,"D" }, {4,"E" }, {5,"F" }, {6,"G" }, {7,"H" }, {8,"I" }, {9,"J" }
            , {10,"K" }, {11,"L" }, {12,"M" }, {13,"N" }, {14,"O" }, {15,"P" }, {16,"Q" }, {17,"R" }, {18,"S" }, {19,"T" }
            , {20,"U" }, {21,"V" }, {22,"W" }, {23,"X" }, {24,"Y" }, {25,"Z" }
        };
        /// <summary>
        /// Keeps track of last box for when selection changes
        /// </summary>
        private int[] lastCellCoord = { 2, 3 };
        /// <summary>
        /// Begins controlling window.
        /// </summary>
        public Controller(ISpreadsheet window, TextReader source)
        {
            this.window = window;
            if (source == null)
            {
                this.spreadsheet = new Spreadsheet();
            }
            else
            {
                Regex allValid = new Regex(@"(.*)?");
                this.spreadsheet = new Spreadsheet(source, allValid);
            }
          
            window.NewEvent += HandleNew;
            window.TypeText += Window_TypeText;
            window.TextFinished += Window_TextFinished;
            window.saveSpreadsheet += Window_saveSpreadsheet;
            window.spreadSheetClose += Window_spreadSheetClose;
        }

        private void Window_spreadSheetClose()
        {
            window.setSaved(!spreadsheet.Changed);              
        }

        private void Window_saveSpreadsheet(StreamWriter dest)
        {
            spreadsheet.Save(dest);
        }

        private int[] deComposeName(string cellName)
        {
            int col = cellName[0] - 65;
            int row = int.Parse(cellName[1].ToString()) - 1;
            int[] nameDecom = new int[] { col, row };
            return nameDecom;
        }


        private void Window_TextFinished(string boxText)
        {
            string col = valueToLetterMap[lastCellCoord[0]];
            string row = (1 + lastCellCoord[1]).ToString(); //added to make a valid cell name
            string lastCell = col + row;
            ISet<string> cellsToUpdate = spreadsheet.SetContentsOfCell(lastCell, spreadsheet.GetCellValue(lastCell).ToString());
            window.setCellValue(lastCellCoord[0], lastCellCoord[1], boxText);
            lastCellCoord = window.GetCoord();
            col = valueToLetterMap[lastCellCoord[0]];
            row = (1 + lastCellCoord[1]).ToString(); //added to make a valid cell name
            lastCell = col + row;
            string equation = spreadsheet.GetCellContents(lastCell).ToString();
            window.setTextBoxValue(equation);
            window.setCellNameValue(lastCell, equation);
            foreach (string x in cellsToUpdate)
            {
                window.setCellValue(deComposeName(x)[0], deComposeName(x)[1], spreadsheet.GetCellValue(x).ToString());
            }
        }

        private void Window_TypeText(string boxText)
        {
            lastCellCoord = window.GetCoord();
            string col = valueToLetterMap[lastCellCoord[0]];
            string row = (1 + lastCellCoord[1]).ToString(); //added to make a valid cell name
            string lastCell = col + row;
            window.setCellNameValue(lastCell, boxText);
        }

        private void HandleNew()
        {
            window.OpenNew();
        }

            
            
    }
}
