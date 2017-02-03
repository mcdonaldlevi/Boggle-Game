using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dependencies;
using System.Diagnostics;

namespace DependencyGraphTestCases
{
    [TestClass]
    public class DependencyGraphTestCases
    {
        [TestMethod]
        public void Construct1()
        {
        DependencyGraph DG = new DependencyGraph();
            DG.AddDependency("a", "b");
            Debug.Assert(DG.HasDependees("a").Equals("b"));
        }

        ///// <summary>
        ///// This tests that a syntactically incorrect parameter to Formula results
        ///// in a FormulaFormatException.
        ///// </summary>
        //[TestMethod]
        //[ExpectedException(typeof(FormulaFormatException))]
        //public void Construct1()
        //{
        //    Formula f = new Formula("_");
        //}
    }
}
