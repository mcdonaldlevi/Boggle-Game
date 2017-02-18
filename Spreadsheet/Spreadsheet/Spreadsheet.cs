using System;
using System.Collections.Generic;
using Formulas;
using Dependencies;
using CellsClass;
using System.Text.RegularExpressions;

namespace SS
{
    public class Spreadsheet : AbstractSpreadsheet
    {
        Cells cells = new Cells();
        AbstractSpreadsheet sheet;
        DependencyGraph dg = new DependencyGraph();

        

        public Spreadsheet()
        {
        }

        public override object GetCellContents(string name)
        {
            return cells.getCell(name);
        }

        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            return cells.getUsedCells();
        }

        public override ISet<string> SetCellContents(string name, Formula formula)
        {
            if (!Regex.IsMatch(name, @"[A-Za-z]+[1-9]\d*"))
            {
                throw new InvalidNameException();
            }

            foreach (var variable in formula.GetVariables())
            {
                dg.AddDependency(name, variable);
            }
            cells.setCell(name, formula);
            ISet<string> set = new HashSet<string>();
            set.Add(name);
            foreach (var dependee in dg.GetDependees(name))
            {
                set.Add(dependee);
            }
            return set;
        }

        public override ISet<string> SetCellContents(string name, string text)
        {
            if (!Regex.IsMatch(name, @"[A-Za-z]+[1-9]\d*"))
            {
                throw new InvalidNameException();
            }

            cells.setCell(name, text);
            return new HashSet<string>();
        }

        public override ISet<string> SetCellContents(string name, double number)
        {

            if (!Regex.IsMatch(name, @"[A-Za-z]+[1-9]\d*"))
            {
                throw new InvalidNameException();
            }

            cells.setCell(name, number);
            return new HashSet<string>();
        }

        protected override IEnumerable<string> GetDirectDependents(string name)
        {
            throw new NotImplementedException();
        }
    }
}
