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
        DependencyGraph dg = new DependencyGraph();
        

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

            cells.setCell(name, formula);

            foreach (var variable in formula.GetVariables())
            {
                dg.AddDependency(name, variable);
                foreach (var dependent in dg.GetDependents(variable))
                {
                    if (dependent == name)
                        throw new CircularException();
                }
            }
            
            return returnSet(name);
        }

        public override ISet<string> SetCellContents(string name, string text)
        {
            if (!Regex.IsMatch(name, @"[A-Za-z]+[1-9]\d*"))
            {
                throw new InvalidNameException();
            }

            cells.setCell(name, text);

            return returnSet(name);
        }

        public override ISet<string> SetCellContents(string name, double number)
        {

            if (!Regex.IsMatch(name, @"[A-Za-z]+[1-9]\d*"))
            {
                throw new InvalidNameException();
            }

            cells.setCell(name, number);

            return returnSet(name);
        }

        protected override IEnumerable<string> GetDirectDependents(string name)
        {
            throw new NotImplementedException();
        }

        private ISet<string> returnSet(String name)
        {
            ISet<string> set = new HashSet<string>();
            set.Add(name);
            foreach (var dependee in dg.GetDependees(name))
            {
                set.Add(dependee);
            }
            return set;
        }
    }
}
