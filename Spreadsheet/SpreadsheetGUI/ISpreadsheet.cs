using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpreadsheetGUI
{
    interface ISpreadsheet
    {
        event Action<string> TextFinished;
        event Action<string> TypeText;
        event Action NewEvent;
        event Action<Stream> saveSpreadsheet;

        void OpenNew();
        int[] GetCoord();
        void setCellValue(int col, int row, string value);
        void setTextBoxValue(string value);
        void clearTextBox();
        void setCellNameValue(string name, string value);
    }   
}
