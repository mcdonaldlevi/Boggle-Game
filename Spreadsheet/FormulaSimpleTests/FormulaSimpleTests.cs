// Written by Joe Zachary for CS 3500, January 2017.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Formulas;
using System.Collections.Generic;

namespace FormulaTestCases
{
    /// <summary>
    /// These test cases are in no sense comprehensive!  They are intended to show you how
    /// client code can make use of the Formula class, and to show you how to create your
    /// own (which we strongly recommend).  To run them, pull down the Test menu and do
    /// Run > All Tests.
    /// </summary>
    [TestClass]
    public class UnitTests
    {

        /// <summary>
        /// The Formula consists of a single variable (x5).  The value of
        /// the Formula depends on the value of x5, which is determined by
        /// the delegate passed to Evaluate.  Since this delegate maps all
        /// variables to 22.5, the return value should be 22.5.
        /// </summary>
        [TestMethod]
        public void Evaluate2()
        {
            Formula f = new Formula("x5");
            Assert.AreEqual(f.Evaluate(v => 22.5), 22.5, 1e-6);
        }

        /// <summary>
        /// The delegate passed to Evaluate is defined below.  We check
        /// that evaluating the formula returns in 10.
        /// </summary>
        [TestMethod]
        public void Evaluate4()
        {
            Formula f = new Formula("x + y");
            Assert.AreEqual(f.Evaluate(Lookup4), 10.0, 1e-6);
        }

        /// <summary>
        /// This uses one of each kind of token.
        /// </summary>
        [TestMethod]
        public void Evaluate5()
        {
            Formula f = new Formula("(x + y) * (z / x) * 1.0");
            Assert.AreEqual(f.Evaluate(Lookup4), 20.0, 1e-6);
        }
        [TestMethod]
        public void Evaluate6()
        {
            Formula f = new Formula("5/(1+4)");
            Assert.AreEqual(f.Evaluate(Lookup4), 1, 1e-6);
        }
        [TestMethod]
        public void Evaluate7()
        {
            Formula f = new Formula("5-(1+4)");
            Assert.AreEqual(f.Evaluate(Lookup4), 0, 1e-6);
        }
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void ConstructTest()
        {
            Formula f = new Formula("1(a)");
        }
        [TestMethod]
        public void normAndValidTest()
        {
            Formula f = new Formula("a + b", Norm1, Valid1);
            ISet<string> myVaribles = f.GetVariables();
            Assert.IsTrue(myVaribles.Contains("A"));
            Assert.IsTrue(myVaribles.Contains("B"));
            Assert.IsTrue(myVaribles.Count == 2);
            
        }
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void falseValidTest()
        {
            Formula g = new Formula("a+b", formula => "a", Valid1);
        }
        [TestMethod]
        public void nullArgumentTest()
        {
            Formula f = new Formula(null);
            Assert.IsTrue(f.ToString() == "0");
        }
        [TestMethod]
        public void nullArgumentTestWithNormAndValid()
        {
            Formula f = new Formula(null, Norm1, Valid1);
            Assert.IsTrue(f.ToString() == "0");
        }
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void NormalizeCausesIncorrectSyntaxTest()
        {
            Formula f = new Formula("a+b", Norm2, Valid1);
        }
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void NormalizeCausesValidFailTest()
        {
            Formula f = new Formula("A+B", Norm3, Valid1);
        }
        [TestMethod]
        public void ValidIsNullTest()
        {
            Formula f = new Formula("a+b", Norm1, null);
            ISet<string> myVaribles = f.GetVariables();
            Assert.IsTrue(myVaribles.Contains("A"));
            Assert.IsTrue(myVaribles.Contains("B"));
        }
        [TestMethod]
        public void NewToStringTest()
        {
            Formula f = new Formula("a+b");
            Assert.IsTrue(f.ToString() == "a+b");
            Formula g = new Formula("a+b+c", Norm1, Valid1);
            Assert.IsTrue(g.ToString() == "A+B+C");
        }
        public string Norm1(string formula)
        {
            return formula.ToUpper();
        }
        public string Norm2(string formula)
        {
            return "^*&&";
        }
        public string Norm3(string formula)
        {
            return formula.ToLower();
        }
        public bool Valid1(string formula)
        {
            foreach (char x in formula)
            {
                if (char.IsLower(x))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// A Lookup method that maps x to 4.0, y to 6.0, and z to 8.0.
        /// All other variables result in an UndefinedVariableException.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public double Lookup4(String v)
        {
            switch (v)
            {
                case "x": return 4.0;
                case "y": return 6.0;
                case "z": return 8.0;
                default: throw new UndefinedVariableException(v);
            }
        }
    }
}
