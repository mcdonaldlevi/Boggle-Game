using System;
using System.Collections.Generic;
using Formulas;
using Dependencies;
using CellsClass;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml.Schema;
using System.Xml;

namespace SS
{
    /// <summary>
    /// An AbstractSpreadsheet object represents the state of a simple spreadsheet.  A 
    /// spreadsheet consists of an infinite number of named cells.
    /// 
    /// A string s is a valid cell name if and only if it consists of one or more letters, 
    /// followed by a non-zero digit, followed by zero or more digits.
    /// 
    /// For example, "A15", "a15", "XY32", and "BC7" are valid cell names.  On the other hand, 
    /// "Z", "X07", and "hello" are not valid cell names.
    /// 
    /// A spreadsheet contains a cell corresponding to every possible cell name.  
    /// In addition to a name, each cell has a contents and a value.  The distinction is
    /// important, and it is important that you understand the distinction and use
    /// the right term when writing code, writing comments, and asking questions.
    /// 
    /// The contents of a cell can be (1) a string, (2) a double, or (3) a Formula.  If the
    /// contents is an empty string, we say that the cell is empty.  (By analogy, the contents
    /// of a cell in Excel is what is displayed on the editing line when the cell is selected.)
    /// 
    /// In an empty spreadsheet, the contents of every cell is the empty string.
    ///  
    /// The value of a cell can be (1) a string, (2) a double, or (3) a FormulaError.  
    /// (By analogy, the value of an Excel cell is what is displayed in that cell's position
    /// in the grid.)
    /// 
    /// If a cell's contents is a string, its value is that string.
    /// 
    /// If a cell's contents is a double, its value is that double.
    /// 
    /// If a cell's contents is a Formula, its value is either a double or a FormulaError.
    /// The value of a Formula, of course, can depend on the values of variables.  The value 
    /// of a Formula variable is the value of the spreadsheet cell it names (if that cell's 
    /// value is a double) or is undefined (otherwise).  If a Formula depends on an undefined
    /// variable or on a division by zero, its value is a FormulaError.  Otherwise, its value
    /// is a double, as specified in Formula.Evaluate.
    /// 
    /// Spreadsheets are never allowed to contain a combination of Formulas that establish
    /// a circular dependency.  A circular dependency exists when a cell depends on itself.
    /// For example, suppose that A1 contains B1*2, B1 contains C1*2, and C1 contains A1*2.
    /// A1 depends on B1, which depends on C1, which depends on A1.  That's a circular
    /// dependency.
    /// </summary>
    public class Spreadsheet : AbstractSpreadsheet
    { 
        Cells cells = new Cells();
        DependencyGraph dg = new DependencyGraph();
        Regex validation;
        Boolean changed = false;


        public override bool Changed
        {
            get
            {
                return changed;
            }

            protected set
            {
                changed = value;
            }
        }

        public Spreadsheet()
        {
            validation = new Regex(@"(.*)?");
        }

        public Spreadsheet(Regex isValid)
        {
            validation = isValid;
        }

        public Spreadsheet(StringReader source, Regex newIsValid)
        {

            {

                XmlSchemaSet sc = new XmlSchemaSet();

                sc.Add(null, "Spreadsheet.xsd");

                XmlReaderSettings settings = new XmlReaderSettings();

                settings.ValidationType = ValidationType.Schema;

                settings.Schemas = sc;

                settings.ValidationEventHandler += ValidationFail;

                Regex oldIsValid = newIsValid;

                this.validation = newIsValid;

                using (XmlReader reader = XmlReader.Create(source, settings))

                {

                    while (reader.Read())

                    {

                        if (reader.IsStartElement())

                        {

                            switch (reader.Name)

                            {

                                case "spreadsheet":

                                    try

                                    {

                                        oldIsValid = new Regex(reader.GetAttribute("IsValid"));

                                    }

                                    catch

                                    {

                                        throw new SpreadsheetReadException("IsValid Regex of source not valid");

                                    }

                                    break;

                                case "cell":

                                    if (cells.containsCell(reader.GetAttribute("name")))

                                    {

                                        throw new SpreadsheetReadException("Duplicate Cell names in source");

                                    }

                                    else if (!oldIsValid.IsMatch(reader.GetAttribute("name")))

                                    {

                                        throw new SpreadsheetReadException("cell name not valid by Old IsValid");

                                    }

                                    else if (!newIsValid.IsMatch(reader.GetAttribute("name")))

                                    {

                                        throw new SpreadsheetReadException("cell name not valid by New IsValid");

                                    }

                                    this.SetContentsOfCell(reader.GetAttribute("name"), reader.GetAttribute("contents"));

                                    break;

                            }

                        }

                    }

                }

            }
        }

        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the contents (as opposed to the value) of the named cell.  The return
        /// value should be either a string, a double, or a Formula.
        /// </summary>
        public override object GetCellContents(string name)
        {
            checkCellNameValidity(name);

            return cells.getCell(name);
        }

        /// <summary>
        /// Enumerates the names of all the non-empty cells in the spreadsheet.
        /// </summary>
        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            return cells.getUsedCells();
        }

        /// <summary>
        /// Requires that all of the variables in formula are valid cell names.
        /// 
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if changing the contents of the named cell to be the formula would cause a 
        /// circular dependency, throws a CircularException.
        /// 
        /// Otherwise, the contents of the named cell becomes formula.  The method returns a
        /// Set consisting of name plus the names of all other cells whose value depends,
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        public ISet<string> SetCellFormula(string name, Formula formula)
        {
            //Checks validity of each variable in formula
            foreach(var variable in formula.GetVariables())
            {
                checkCellNameValidity(variable);
            }

            //Removes old dependencies if last value was a formula
            if(cells.containsCell(name))
                if (cells.getCell(name).GetType() == typeof(Formula))
                {
                    Formula old_formula = (Formula)cells.getCell(name);
                    foreach (var variable in old_formula.GetVariables())
                    {
                        dg.RemoveDependency(name, variable);
                    }
                }

            cells.setCell(name, formula);

            //Adds new dependencies
            foreach (var variable in formula.GetVariables())
            {
                dg.AddDependency(name, variable);
            }
            
            checkCircularDependency(name, formula);

            return returnSet(name);
        }

        /// <summary>
        /// If text is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes text.  The method returns a
        /// set consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        public override ISet<string> SetContentsOfCell(string name, string text)
        {
            changed = true;
            name = name.ToUpper();

            if (text == null)
                throw new ArgumentException();
            checkCellNameValidity(name);

             if(!validation.IsMatch(text.ToUpper()))
            {
                throw new InvalidNameException();
            }
            double output_num;

            if(text.Substring(0, 1).Equals("="))
            {
                return SetCellContents(name, new Formula(text.Remove(0, 1)));
            } 
            else if(Double.TryParse(text, out output_num))
            {
                return SetCellContents(name, output_num);
            }
            else
            {
                return SetCellContents(name, text);
            }
             
            return returnSet(name);
        }

        /// <summary>
        /// If name is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name isn't a valid cell name, throws an InvalidNameException.
        /// 
        /// Otherwise, returns an enumeration, without duplicates, of the names of all cells whose
        /// values depend directly on the value of the named cell.  In other words, returns
        /// an enumeration, without duplicates, of the names of all cells that contain
        /// formulas containing name.
        /// 
        /// For example, suppose that
        /// A1 contains 3
        /// B1 contains the formula A1 * A1
        /// C1 contains the formula B1 + A1
        /// D1 contains the formula B1 - C1
        /// The direct dependents of A1 are B1 and C1
        /// </summary>
        protected override IEnumerable<string> GetDirectDependents(string name)
        {
            checkCellNameValidity(name);

            return dg.GetDependees(name);
        }

        /// <summary>
        /// Returns a Set of dependees, used after setting cell contents
        /// </summary>
        private ISet<string> returnSet(String name)
        {
            ISet<string> set = new HashSet<string>();
            set.Add(name);
            //Adds dependees to set
            foreach (var dependee in dg.GetDependees(name))
            {
                set.Add(dependee);
            }
            return set;
        }

        /// <summary>
        /// Checks for circular dependency when cell is added with a formula
        /// </summary>
        private void checkCircularDependency(String name, Formula formula)
        {
            foreach(var variable in formula.GetVariables())
            {
                recurseThroughDependencies(variable, new List<string> { name });
            }
            
        }

        /// <summary>
        /// Recurses until all dependents have been checked to see if they were used as a dependency earlier
        /// </summary>
        private void recurseThroughDependencies(String name, List<string> visited)
        {
            foreach (var dependent in dg.GetDependents(name))
            {
                if (visited.Contains(dependent))
                    throw new CircularException();
                visited.Add(dependent);
                recurseThroughDependencies(dependent, visited);
            }
        }

        //Throws exception if input string is null or not a valid cell name
        private void checkCellNameValidity(String name)
        {
            if (name == null)
                throw new InvalidNameException();
            if (!Regex.IsMatch(name, @"[A-Za-z]+[1-9]\d*"))
                throw new InvalidNameException();
        }

        public override void Save(TextWriter dest)
        {
            changed = false;
        }

        public override object GetCellValue(string name)
        {
            return cells.getCell(name);
        }

        /// <summary>
        /// Requires that all of the variables in formula are valid cell names.
        /// 
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if changing the contents of the named cell to be the formula would cause a 
        /// circular dependency, throws a CircularException.
        /// 
        /// Otherwise, the contents of the named cell becomes formula.  The method returns a
        /// Set consisting of name plus the names of all other cells whose value depends,
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        protected override ISet<string> SetCellContents(string name, Formula formula)
        {
            checkCellNameValidity(name);
            //Checks validity of each variable in formula
            foreach (var variable in formula.GetVariables())
            {
                checkCellNameValidity(variable);
            }

            //Removes old dependencies if last value was a formula
            if (cells.containsCell(name))
                if (cells.getCell(name).GetType() == typeof(Formula))
                {
                    Formula old_formula = (Formula)cells.getCell(name);
                    foreach (var variable in old_formula.GetVariables())
                    {
                        dg.RemoveDependency(name, variable);
                    }
                }

            //Adds new dependencies
            foreach (var variable in formula.GetVariables())
            {
                dg.AddDependency(name, variable);
            }

            checkCircularDependency(name, formula);

            cells.setCell(name, formula);

            return returnSet(name);
        }

        /// <summary>
        /// If text is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes text.  The method returns a
        /// set consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        protected override ISet<string> SetCellContents(string name, string text)
        {
            if (text == null)
                throw new ArgumentException();
            checkCellNameValidity(name);

            cells.setCell(name, text);

            return returnSet(name);
        }

        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes number.  The method returns a
        /// set consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        protected override ISet<string> SetCellContents(string name, double number)
        {
            checkCellNameValidity(name);

            cells.setCell(name, number);

            return returnSet(name);
        }

        private void ValidationFail(object sender, ValidationEventArgs e)
        {
            throw new SpreadsheetReadException("Spreadsheet did not match Schema");
        }
    }
}



