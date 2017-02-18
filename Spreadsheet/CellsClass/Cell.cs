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

        public Cell(string cont)
        {
            content = cont;
            value = content;
        }

        public Cell(double cont)
        {
            content = cont;
            value = content;
        }

        public Cell(Formula cont)
        {
            content = cont;
            //Placeholder
            //value = cont.Evaluate();
        }

        public object getCellContent()
        {
            return content;
        }
    }
}
