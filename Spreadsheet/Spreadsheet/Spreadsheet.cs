using System;
using System.Collections.Generic;
using Formulas;
using Dependencies;
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
        DependencyGraph dg = new DependencyGraph();
        Dictionary<string, Cell> cellDict = new Dictionary<string, Cell>();
        Regex validation;
        Boolean changed = false;

        /// <summary>
        /// Returns boolean, whether or not if it has been changed from last save
        /// </summary>
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

        /// <summary>
        /// Constructor that takes no parameters
        /// </summary>
        public Spreadsheet()
        {
            validation = new Regex(@"(.*)?");
        }

        /// <summary>
        /// Constructor that takes only a Regex file as a parameter
        /// </summary>
        /// <param name="isValid"></param>
        public Spreadsheet(Regex isValid)
        {
            validation = isValid;
        }

        /// <summary>
        /// Constructor that takes a TextReader and Regex file as a constructor
        /// </summary>
        /// <param name="source"></param>
        /// <param name="newIsValid"></param>
        public Spreadsheet(TextReader source, Regex newIsValid)
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

                                    if (cellDict.ContainsKey(reader.GetAttribute("name")))

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
            name = name.ToUpper();
            checkCellNameValidity(name);

            if (cellDict.ContainsKey(name))
                return cellDict[name].getCellContent();
            return "";
        }

        /// <summary>
        /// Enumerates the names of all the non-empty cells in the spreadsheet.
        /// </summary>
        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            foreach (var cell in cellDict)
            {
                yield return cell.Key;
            }
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
            if(cellDict.ContainsKey(name))
                if (GetCellContents(name).GetType() == typeof(Formula))
                {
                    Formula old_formula = (Formula)GetCellContents(name);
                    foreach (var variable in old_formula.GetVariables())
                    {
                        dg.RemoveDependency(name, variable);
                    }
                }

            if (cellDict.ContainsKey(name))
            {
                cellDict[name] = new Cell(formula, lookUp);
            }
            else
            {
                cellDict.Add(name, new Cell(formula, lookUp));
            }

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

            if (GetCellContents(name).GetType() == typeof(Formula))
            {
                List<string> dependents = new List<string> { };
                foreach (var dep in dg.GetDependents(name))
                {
                    dependents.Add(dep);
                }
                foreach (var dep in dependents)
                {
                    dg.RemoveDependency(name, dep);
                }
            }

            if (text == null)
                throw new ArgumentException();
            checkCellNameValidity(name);

            if (!validation.IsMatch(text.ToUpper()))
            {
                throw new InvalidNameException();
            }
            double output_num;

            if (text.Length > 0)
            {
                if (text.Substring(0, 1).Equals("="))
                {
                    return SetCellContents(name, new Formula(text.Remove(0, 1), s => s.ToUpper(), s => true));
                }
                else if (Double.TryParse(text, out output_num))
                {
                    return SetCellContents(name, output_num);
                }
                return SetCellContents(name, text);
            }
            
            cellDict.Remove(name);
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
            set = getIndirectDependees(name, set);
            return set;
        }

        private ISet<string> getIndirectDependees(String name, ISet<string> set)
        {
            foreach (var dependee in dg.GetDependees(name))
            {
                set.Add(dependee);
                set = getIndirectDependees(dependee, set);
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

        /// <summary>
        /// Checks for circular dependency when cell is added with a formula
        /// </summary>
        private void EvaluateRecurse(String name, List<string> visited)
        {

            //Duplicate lists so it doesn't break foreach loop later from contents being changed
            List<string> revisit = new List<string> { };
            List<string> new_revisit = new List<string> { };
            //Iterates through dependees
            foreach (var dependee in dg.GetDependees(name))
            {
                if (!visited.Contains(dependee))
                {
                    visited.Add(dependee);
                    if (cellDict.ContainsKey(dependee))
                    {
                        //Tries to evaluate cell's value
                        cellDict[dependee].tryEvaluate(name, lookUp);
                        //If it didn't result in an error, it adds the dependee's dependees to the revisit list
                        if (cellDict[dependee].getCellValue().GetType() != typeof(FormulaError))
                            foreach (var dep in dg.GetDependees(dependee))
                            {
                                revisit.Add(dep);
                                new_revisit.Add(dep);
                            }
                    }
                }
            }

            //Iterates through values that need to be revisited
            foreach (var item in revisit)
            {
                //Attempts to evaluate
                cellDict[item].tryEvaluate(name, lookUp);
                //If it didn't result in an error, it adds its dependees to revisit list if not already visited
                if (cellDict[item].getCellValue().GetType() != typeof(FormulaError))
                {
                    foreach (var dep in dg.GetDependees(item))
                    {
                        if (!visited.Contains(dep))
                            new_revisit.Add(dep);
                    }
                    //Removes value from revisit list
                    new_revisit.Remove(item);
                }
            }
            
            //Recurses if values still need to be revisited
            if (new_revisit.Count > 0)
                foreach (var item in new_revisit)
                {
                    EvaluateRecurse(item, new List<string> { });
                }
        }

        /// <summary>
        /// Recurses until all dependents have been checked to see if they were used as a dependency earlier
        /// </summary>
        private void recurseEval(String name, List<string> visited)
        {
            foreach (var dependent in dg.GetDependents(name))
            {
                if (!visited.Contains(dependent))
                {
                    visited.Add(dependent);
                    //recurseEval(dependent, visited);
                    if(cellDict.ContainsKey(dependent))
                        cellDict[dependent].tryEvaluate(name, lookUp);
                }
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

        /// <summary>
        /// Saves the current SpreadSheet file to a text document
        /// </summary>
        /// <param name="dest"></param>
        public override void Save(TextWriter dest)
        {
            changed = false;
        }

        /// <summary>
        /// Returns the value of the cell specified
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public override object GetCellValue(string name)
        {
            name = name.ToUpper();
            if (cellDict.ContainsKey(name))
                return cellDict[name].getCellValue();
            return "";
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
            if (cellDict.ContainsKey(name))
            {
                dynamic old_cell = GetCellContents(name);
                if (old_cell.GetType() == typeof(Formula))
                {
                    Formula old_formula = (Formula)GetCellContents(name);
                    foreach (var variable in old_formula.GetVariables())
                    {
                        dg.RemoveDependency(name, variable);
                    }
                }
            }

            //Adds new dependencies
            foreach (var variable in formula.GetVariables())
            {
                dg.AddDependency(name, variable);
            }

            checkCircularDependency(name, formula);

            if (cellDict.ContainsKey(name))
            {
                cellDict[name] = new Cell(formula, lookUp);
            }
            else
            {
                cellDict.Add(name, new Cell(formula, lookUp));
            }

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

            if (cellDict.ContainsKey(name))
            {
                cellDict[name] = new Cell(text);
            }
            else
            {
                cellDict.Add(name, new Cell(text));
            }

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

            if (cellDict.ContainsKey(name))
            {
                cellDict[name] = new Cell(number);
            }
            else
            {
                cellDict.Add(name, new Cell(number));
            }
            var visited = new List<string> { name };
            EvaluateRecurse(name, visited);

            return returnSet(name);
        }

        private void ValidationFail(object sender, ValidationEventArgs e)
        {
            throw new SpreadsheetReadException("Spreadsheet did not match Schema");
        }

        /// <summary>
        /// returns the value of the cell that was specified
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public double lookUp(string s)
        {
            s = s.ToUpper();
            return (double)cellDict[s].getCellValue();
        }
    }

    class Cell
    {
        object value;
        object content;

        public Cell(string cont)
        {
            content = cont;
            value = cont;
        }

        public Cell(double cont)
        {
            content = cont;
            value = cont;
        }

        public Cell(Formula cont, Lookup lookUp)
        {
            content = cont;
            try
            {
                value = cont.Evaluate(lookUp);
            }
            catch
            {
                value = new FormulaError();
            }
        }

        public object getCellContent()
        {
            return content;
        }

        public object getCellValue()
        {
            return value;
        }

        public void tryEvaluate(string name, Lookup lookup)
        {
            var temp_cont = (Formula)content;
            try
            {
                value = temp_cont.Evaluate(lookup);
            }
            catch
            {
                value = new FormulaError();
            }
        }
    }
}



