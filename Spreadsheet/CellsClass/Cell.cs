using Formulas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CellsClass
{
    class Cell
    {
        object value;
        object content;

        public Cell(string content)
        {
            value = content;
        }

        public Cell(double content)
        {
            value = content;
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
    }

    /// <summary>
    /// A possible value of a cell.
    /// </summary>
    public struct FormulaError
    {
        /// <summary>
        /// Constructs a FormulaError containing the explanatory reason.
        /// </summary>
        public FormulaError(String reason)
            : this()
        {
            Reason = reason;
        }

        /// <summary>
        ///  The reason why this FormulaError was created.
        /// </summary>
        public string Reason { get; private set; }
    }
}
