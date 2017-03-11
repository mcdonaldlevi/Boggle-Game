using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Formulas;
using Dependencies;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;
using System.Xml.Schema;

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
        Dictionary<string, Cell> cells = new Dictionary<string, Cell>();
        DependencyGraph dependency = new DependencyGraph();
        Regex IsValid;
        /// <summary>
        /// Constructs a Spreadsheet with a Validity checker.
        /// </summary>
        /// <param name="IsValid"></param>
        public Spreadsheet(Regex IsValid)
        {
            this.IsValid = IsValid;
        }
        /// <summary>
        /// Creates a spreadsheet where anything is valid.
        /// </summary>
        public Spreadsheet()
        {
            Regex valid = new Regex(@"(.*)?");
            IsValid = valid;
        }
        /// <summary>
        /// Makes a spreadsheet by reading from a source file.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="newIsValid"></param>
        public Spreadsheet(TextReader source, Regex newIsValid)
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.Add(null, "Spreadsheet.xsd");
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.Schemas = sc;
            settings.ValidationEventHandler += ValidationFail;
            Regex oldIsValid = newIsValid;
            this.IsValid = newIsValid;
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
                                if (cells.ContainsKey(reader.GetAttribute("name")))
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
        struct Cell
        {            
            public object myValue;
            public object myContents;
            public Cell(object value, Lookup lookUp)
            {
                if (value.GetType() == typeof(string))
                {
                    myValue = value;
                    myContents = value;
                }
                else if (value.GetType() == typeof(double))
                {
                    myValue = value;
                    myContents = value;
                }
                else
                {
                    Formula f = (Formula)value;
                    try
                    {
                        myValue = f.Evaluate(lookUp);
                        string contentsWithEqual = value.ToString();
                        if(contentsWithEqual[0] != '=')
                            contentsWithEqual = contentsWithEqual.Insert(0, "=");
                        myContents = contentsWithEqual;
                         
                    }
                    catch
                    {
                        myValue = new FormulaError("Cells have no valid value");
                        string contentsWithEqual = value.ToString();
                        if(contentsWithEqual[0] != '=')
                            contentsWithEqual = contentsWithEqual.Insert(0, "=");
                        myContents = contentsWithEqual;
                    }                    
                }
            }
        }
        /// <summary>
        /// makes a lookup function to check the spreadsheet and retreive values of cells.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public double lookUp(string s)
        {

            if (cells[s].myValue.GetType() == typeof(string))
            {
                throw new FormulaEvaluationException("Cell refrenced was a string");
            }
            else
            {
                return (double)cells[s].myValue;
            }
        }
        /// <summary>
        /// turns the Regex input into a valid bool to pass to a Formula
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>      
        public bool valid(string s)
        {
            return IsValid.IsMatch(s);
        }
        /// <summary>
        /// True if this spreadsheet has been modified since it was created or saved
        /// (whichever happened most recently); false otherwise.
        /// </summary>
        public override bool Changed { get; protected set; }

        // ADDED FOR PS6
        /// <summary>
        /// Writes the contents of this spreadsheet to dest using an XML format.
        /// The XML elements should be structured as follows:
        ///
        /// <spreadsheet IsValid="IsValid regex goes here">
        ///   <cell name="cell name goes here" contents="cell contents go here"></cell>
        ///   <cell name="cell name goes here" contents="cell contents go here"></cell>
        ///   <cell name="cell name goes here" contents="cell contents go here"></cell>
        /// </spreadsheet>
        ///
        /// The value of the IsValid attribute should be IsValid.ToString()
        /// 
        /// There should be one cell element for each non-empty cell in the spreadsheet.
        /// If the cell contains a string, the string (without surrounding double quotes) should be written as the contents.
        /// If the cell contains a double d, d.ToString() should be written as the contents.
        /// If the cell contains a Formula f, f.ToString() with "=" prepended should be written as the contents.
        ///
        /// If there are any problems writing to dest, the method should throw an IOException.
        /// </summary>
        public override void Save(TextWriter dest)
        {
            using (XmlWriter writer = XmlWriter.Create(dest))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("spreadsheet");
                writer.WriteAttributeString("IsValid", IsValid.ToString());
                IEnumerable<string> myCells = GetNamesOfAllNonemptyCells();

                foreach (string x in myCells)
                {
                    writer.WriteStartElement("cell");
                    writer.WriteAttributeString("name", x);
                    writer.WriteAttributeString("contents", cells[x].myContents.ToString());
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }

        }
        // ADDED FOR PS6
        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        ///
        /// Otherwise, returns the value (as opposed to the contents) of the named cell.  The return
        /// value should be either a string, a double, or a FormulaError.
        /// </summary>
        public override object GetCellValue(String name)
        {
            if (cells.ContainsKey(name))
            {
                return cells[name].myValue;
            }
            else
            {
                throw new InvalidNameException();
            }
        }
        /// <summary>
        /// Enumerates the names of all the non-empty cells in the spreadsheet.
        /// </summary>
        public override IEnumerable<String> GetNamesOfAllNonemptyCells()
        {
            IEnumerable<String> names = cells.Keys;
            return names;
        }

        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the contents (as opposed to the value) of the named cell.  The return
        /// value should be either a string, a double, or a Formula.
        /// </summary>
        public override object GetCellContents(String name)
        {
            if (!IsValid.IsMatch(name))
            {
                throw new InvalidNameException();
            }
            if (cells.ContainsKey(name))
            {
                return cells[name].myContents;
            }
            else
            {
                return "";
            }
        }
        // ADDED FOR PS6
        /// <summary>
        /// If content is null, throws an ArgumentNullException.
        ///
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        ///
        /// Otherwise, if content parses as a double, the contents of the named
        /// cell becomes that double.
        ///
        /// Otherwise, if content begins with the character '=', an attempt is made
        /// to parse the remainder of content into a Formula f using the Formula
        /// constructor with s => s.ToUpper() as the normalizer and a validator that
        /// checks that s is a valid cell name as defined in the AbstractSpreadsheet
        /// class comment.  There are then three possibilities:
        ///
        ///   (1) If the remainder of content cannot be parsed into a Formula, a
        ///       Formulas.FormulaFormatException is thrown.
        ///
        ///   (2) Otherwise, if changing the contents of the named cell to be f
        ///       would cause a circular dependency, a CircularException is thrown.
        ///
        ///   (3) Otherwise, the contents of the named cell becomes f.
        ///
        /// Otherwise, the contents of the named cell becomes content.
        ///
        /// If an exception is not thrown, the method returns a set consisting of
        /// name plus the names of all other cells whose value depends, directly
        /// or indirectly, on the named cell.
        ///
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        public override ISet<String> SetContentsOfCell(String name, String content)
        {
            if(!IsValid.IsMatch(name))
            {
                throw new InvalidNameException();
            }
            if(content == "")
            {
                HashSet<string> nullSet = new HashSet<string>();
                return nullSet;
            }
            Changed = true;
            name = name.ToUpper();
            double myDouble;
            if (double.TryParse(content, out myDouble))
            {
                ISet<string> returnSet = SetCellContents(name, myDouble);
                IEnumerable<string> cellsToRecalculate = GetCellsToRecalculate(name);

                foreach (string x in cellsToRecalculate.Skip(1))
                {
                    string form1 = cells[x].myContents.ToString();//modified
                    form1 = form1.Remove(0, 1);
                    Formula f = new Formula(form1);
                    cells[x] = new Cell(f, lookUp);
                }
                
                return returnSet;
            }
            
            else if (content[0] == '=')
            {
                content = content.Remove(0,1);
                Formula form;
                try
                {
                    form = new Formula(content, s => s.ToUpper(), valid);
                }
                catch
                {
                    throw new FormulaFormatException("formula was not valid for Spreadsheet Validitor");
                }
                ISet<string> returnSet = SetCellContents(name, form);
                IEnumerable<string> cellsToRecalculate = GetCellsToRecalculate(name);
                if (cellsToRecalculate.Count() > 1)
                {
                    foreach (string x in cellsToRecalculate)
                    {
                        string form1 = cells[x].myContents.ToString(); //modified
                        form1 = form1.Remove(0, 1);
                        Formula f = new Formula(form1);
                        cells[x] = new Cell(f, lookUp);
                    }
                }

                return returnSet;
            }
            else
            {
                ISet<string> returnSet = SetCellContents(name, content);
                IEnumerable<string> cellsToRecalculate = GetCellsToRecalculate(name);
                if (cellsToRecalculate.Count() > 1)
                {
                    foreach (string x in cellsToRecalculate)
                    {
                        string form1 = cells[x].myContents.ToString();// modifided
                        form1 = form1.Remove(0, 1);
                        Formula f = new Formula(form1);
                        cells[x] = new Cell(f, lookUp);
                    }
                }

                return returnSet;
            }
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
        protected override ISet<String> SetCellContents(String name, double number)
        {
            string pattern = @"[a-zA-Z]{1,2}[1-9][0-9]*";
            if (!Regex.IsMatch(name, pattern))
            {
                throw new InvalidNameException();
            }
            if (cells.ContainsKey(name))
            {
                cells[name] = new Cell(number, lookUp);
                IEnumerable<string> dependents = dependency.GetDependees(name);
                foreach (string x in dependents)
                {
                    dependency.RemoveDependency(name, x);
                }
                IEnumerable<string> dependentEnumerable = GetCellsToRecalculate(name);
                ISet<string> dependentSet = new HashSet<string>();
                foreach (string x in dependentEnumerable)
                {
                    dependentSet.Add(x);
                }
                return dependentSet;
            }
            else
            {
                Cell myCell = new Cell(number, lookUp);
                cells.Add(name, myCell);
                IEnumerable<string> dependents = dependency.GetDependees(name);
                foreach (string x in dependents)
                {
                    dependency.RemoveDependency(name, x);
                }
                IEnumerable<string> dependentEnumerable = GetCellsToRecalculate(name);
                ISet<string> dependentSet = new HashSet<string>();
                foreach(string x in dependentEnumerable)
                {
                    dependentSet.Add(x);
                }
                return dependentSet;
            }

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
        protected override ISet<String> SetCellContents(String name, String text)
        {
            string pattern = @"[a-zA-Z]{1,2}[1-9][0-9]*";
            if (!Regex.IsMatch(name, pattern))
            {
                throw new InvalidNameException();
            }
            if (cells.ContainsKey(name))
            {
                cells[name] = new Cell(text, lookUp);
                IEnumerable<string> dependents = dependency.GetDependees(name);
                foreach (string x in dependents)
                {
                    dependency.RemoveDependency(name, x);
                }
                IEnumerable<string> dependentEnumerable = GetCellsToRecalculate(name);
                ISet<string> dependentSet = new HashSet<string>();
                foreach (string x in dependentEnumerable)
                {
                    dependentSet.Add(x);
                }
                return dependentSet;

            }
            else
            {
                Cell myCell = new Cell(text, lookUp);
                cells.Add(name, myCell);
                IEnumerable<string> dependents = dependency.GetDependees(name);
                foreach (string x in dependents)
                {
                    dependency.RemoveDependency(name, x);
                }
                IEnumerable<string> dependentEnumerable = GetCellsToRecalculate(name);
                ISet<string> dependentSet = new HashSet<string>();
                foreach (string x in dependentEnumerable)
                {
                    dependentSet.Add(x);
                }
                return dependentSet;
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
        protected override ISet<String> SetCellContents(String name, Formula formula)
        {
            string pattern = @"[a-zA-Z]{1,2}[1-9][0-9]*";
            if (!Regex.IsMatch(name, pattern))
            {
                throw new InvalidNameException();
            }
            if (cells.ContainsKey(name))
            {
                cells[name] = new Cell(formula, lookUp);
                IEnumerable<string> dependents = dependency.GetDependees(name);
                IEnumerable<string> varibles = formula.GetVariables();
                foreach (string x in varibles)
                {
                    foreach(string z in dependency.GetDependees(x))
                    {
                        if (z == name)
                            throw new CircularException();
                    }
                    dependency.AddDependency(x, name);                    
                }
                foreach (string y in dependents)
                {
                    dependency.RemoveDependency(name, y);
                }

                ISet<string> dependentSet = new HashSet<string>();
                foreach (string x in dependents)
                {
                    dependentSet.Add(x);
                }
                return dependentSet;
            }
            else
            {
                Cell myCell = new Cell(formula, lookUp);
                cells.Add(name, myCell);
                IEnumerable<string> dependents = GetCellsToRecalculate(name);
                IEnumerable<string> varibles = formula.GetVariables();
                foreach (string x in varibles)
                {
                    dependency.AddDependency(x, name);
                }
                foreach (string y in dependents)
                {
                    dependency.RemoveDependency(name, y);
                }
                foreach (string z in varibles)
                {
                    if (dependents.Contains(z))
                    {
                        throw new CircularException();
                    }
                }
                ISet<string> dependentSet = new HashSet<string>();
                foreach (string x in dependents)
                {
                    dependentSet.Add(x);
                }
                return dependentSet;
            }

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
        protected override IEnumerable<String> GetDirectDependents(String name)
        {
            return dependency.GetDependents(name);
        }

        /// <summary>
        /// Requires that names be non-null.  Also requires that if names contains s,
        /// then s must be a valid non-null cell name.
        /// 
        /// If any of the named cells are involved in a circular dependency,
        /// throws a CircularException.
        /// 
        /// Otherwise, returns an enumeration of the names of all cells whose values must
        /// be recalculated, assuming that the contents of each cell named in names has changed.
        /// The names are enumerated in the order in which the calculations should be done.  
        /// 
        /// For example, suppose that 
        /// A1 contains 5
        /// B1 contains 7
        /// C1 contains the formula A1 + B1
        /// D1 contains the formula A1 * C1
        /// E1 contains 15
        /// 
        /// If A1 and B1 have changed, then A1, B1, and C1, and D1 must be recalculated,
        /// and they must be recalculated in either the order A1,B1,C1,D1 or B1,A1,C1,D1.
        /// The method will produce one of those enumerations.
        /// 
        /// PLEASE NOTE THAT THIS METHOD DEPENDS ON THE ABSTRACT GetDirectDependents.
        /// IT WON'T WORK UNTIL GetDirectDependents IS IMPLEMENTED CORRECTLY.  YOU WILL
        /// NOT NEED TO MODIFY THIS METHOD.
        /// </summary>
        protected IEnumerable<String> GetCellsToRecalculate(ISet<String> names)
        {
            LinkedList<String> changed = new LinkedList<String>();
            HashSet<String> visited = new HashSet<String>();
            foreach (String name in names)
            {
                if (!visited.Contains(name))
                {
                    Visit(name, name, visited, changed);
                }
            }
            return changed;
        }

        /// <summary>
        /// A convenience method for invoking the other version of GetCellsToRecalculate
        /// with a singleton set of names.  See the other version for details.
        /// </summary>
        protected IEnumerable<String> GetCellsToRecalculate(String name)
        {
            return GetCellsToRecalculate(new HashSet<String>() { name });
        }

        /// <summary>
        /// A helper for the GetCellsToRecalculate method.
        /// </summary>
        private void Visit(String start, String name, ISet<String> visited, LinkedList<String> changed)
        {
            visited.Add(name);
            foreach (String n in GetDirectDependents(name))
            {
                if (n.Equals(start))
                {
                    throw new CircularException();
                }
                else if (!visited.Contains(n))
                {
                    Visit(start, n, visited, changed);
                }
            }
            changed.AddFirst(name);
        }
        private string upper(string s)
        {
            return s.ToUpper();
        }
        private void ValidationFail(object sender, ValidationEventArgs e)
        {
            throw new SpreadsheetReadException("Spreadsheet did not match Schema");
        }
    }
}


