using Formulas;
using System;
using System.Collections.Generic;

namespace CellsClass
{
    public class Cells
    {
        Dictionary<string, Cell> cellDict = new Dictionary<string, Cell>();

        public void setCell(string cell, string value)
        {
            if (cellDict.ContainsKey(cell))
            {
                cellDict[cell] = new Cell(value);
            }
            else
            {
                cellDict.Add(cell, new Cell(value));
            }
        }

        public void setCell(string cell, Double value)
        {
            if (cellDict.ContainsKey(cell))
            {
                cellDict[cell] = new Cell(value);
            }
            else
            {
                cellDict.Add(cell, new Cell(value));
            }
        }

        public void setCell(string cell, Formula value)
        {
            if (cellDict.ContainsKey(cell))
            {
                cellDict[cell] = new Cell(value, lookUp);
            }
            else
            {
                cellDict.Add(cell, new Cell(value, lookUp));
            }
        }

        public object getCell(string cell)
        {
            if (cellDict.ContainsKey(cell))
                return cellDict[cell].getCellContent();
            return "";
        }

        public Boolean containsCell(string name)
        {
            return cellDict.ContainsKey(name);
        }

        public IEnumerable<string> getUsedCells()
        {
            foreach (var cell in cellDict)
            {
                yield return cell.Key;
            }
        }

        public double lookUp(string s)
        {
            s = s.ToUpper();
            return (double)cellDict[s].getCellValue();
        }
    }
}
