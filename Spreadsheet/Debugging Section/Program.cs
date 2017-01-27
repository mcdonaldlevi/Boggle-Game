using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Formulas;

namespace Debugging_Section
{
    class Program
    {
        static void Main(string[] args)
        {
            Formula myFormula = new Formula("x+y");
            double value = myFormula.Evaluate("z" = 3);
            Console.WriteLine(value);
        }

        Lookup myLook(String z)
        {
            switch (z)
            {
                case "z": return 10.0;
                default: throw new UndefinedVariableException(z);
            }
        }
    }
}
