using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpreadsheetGUI
{
    public interface ISpreadsheet
    {
        event Action<string> TextFinished;
        event Action<string> TypeText;
        event Action NewEvent;
        event Action<StreamWriter> saveSpreadsheet;
        event Action spreadSheetClose;
        bool isSaved();
        void setSaved(bool value);
        void OpenNew();
        int[] GetCoord();
        void setCellValue(int col, int row, string value);
        void setTextBoxValue(string value);
        void clearTextBox();
        void setCellNameValue(string name, string value);
    }   
}
