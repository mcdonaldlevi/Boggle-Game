using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CellsClass
{
    public class Cells
    {
    Dictionary<string, object> cellDict = new Dictionary<string, object>();

    public void setCell(string cell, object value)
    {
        if (cellDict.ContainsKey(cell))
        {
            cellDict[cell] = value;
        }
        else
        {
            cellDict.Add(cell, value);
        }
    }

    public object getCell(string cell)
    {
        if (cellDict.ContainsKey(cell))
            return cellDict[cell];
         return "";
    }

    public IEnumerable<string> getUsedCells()
    {
        foreach (var cell in cellDict)
        {
            yield return cell.Key;
        }
    }
}
}
