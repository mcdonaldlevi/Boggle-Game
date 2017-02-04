using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dependencies;
using System.Diagnostics;
using System.Collections.Generic;

namespace DependencyGraphTestCases
{
    [TestClass]
    public class DependencyGraphTestCases
    {
        [TestMethod]
        public void HasDependentsTest()
        {
        DependencyGraph DG = new DependencyGraph();
            DG.AddDependency("a", "b");
            Debug.Assert(DG.HasDependents("a"));
        }

        [TestMethod]
        public void HasDependentsFailTest()
        {
            DependencyGraph DG = new DependencyGraph();
            DG.AddDependency("c", "b");
            Debug.Assert(!DG.HasDependents("b"));
        }

        [TestMethod]
        public void HasDependeesTest()
        {
            DependencyGraph DG = new DependencyGraph();
            DG.AddDependency("e", "f");
            Debug.Assert(DG.HasDependees("f"));
        }

        [TestMethod]
        public void HasDependeesFailTest()
        {
            DependencyGraph DG = new DependencyGraph();
            DG.AddDependency("g", "f");
            Debug.Assert(!DG.HasDependees("g"));
        }

        [TestMethod]
        public void GetDependeesTest()
        {
            DependencyGraph DG = new DependencyGraph();
            DG.AddDependency("a", "b");
            DG.AddDependency("a", "c");
            DG.AddDependency("a", "d");
            DG.AddDependency("a", "e");
            int i = 0;
            string[] test_list = { "b", "c", "d", "e" };
            foreach(string dependee in DG.GetDependees("a"))
            {
                Debug.Assert(dependee.Equals(test_list[i]));
                i++;
            }
        }

        [TestMethod]
        public void GetDependentsTest()
        {
            DependencyGraph DG = new DependencyGraph();
            DG.AddDependency("b", "a");
            DG.AddDependency("c", "a");
            DG.AddDependency("d", "a");
            DG.AddDependency("e", "a");
            int i = 0;
            string[] test_list = { "b", "c", "d", "e" };
            foreach (string dependent in DG.GetDependents("a"))
            {
                Debug.Assert(dependent.Equals(test_list[i]));
                i++;
            }
        }

        [TestMethod]
        public void RemoveDependeeTest()
        {
            DependencyGraph DG = new DependencyGraph();
            DG.AddDependency("a", "b");
            DG.AddDependency("a", "c");
            DG.AddDependency("a", "d");
            DG.AddDependency("a", "e");

            DG.RemoveDependency("a", "c");
            foreach(string dependee in DG.GetDependees("a"))
            {
                Debug.Assert(dependee != "c");
            }
        }

        [TestMethod]
        public void RemoveDependentTest()
        {
            DependencyGraph DG = new DependencyGraph();
            DG.AddDependency("b", "a");
            DG.AddDependency("c", "a");
            DG.AddDependency("d", "a");
            DG.AddDependency("e", "a");

            DG.RemoveDependency("c", "a");
            foreach (string dependent in DG.GetDependents("a"))
            {
                Debug.Assert(dependent != "c");
            }
        }

        [TestMethod]
        public void CorrectSizeTest()
        {
            DependencyGraph DG = new DependencyGraph();
            DG.AddDependency("c", "b");
            DG.AddDependency("a", "f");
            DG.AddDependency("l", "d");
            DG.AddDependency("a", "p");

            Debug.Assert(DG.Size == 4);

            DG.RemoveDependency("c", "b");
            DG.RemoveDependency("l", "d");

            Debug.Assert(DG.Size == 2);
        }

        [TestMethod]
        public void ReplaceDependentsTest()
        {
            DependencyGraph DG = new DependencyGraph();
            DG.AddDependency("a", "b");
            DG.AddDependency("c", "b");
            DG.AddDependency("d", "e");
            DG.AddDependency("f", "e");

            DG.ReplaceDependents("b", new List<string> { "l", "o", "p" });

            Debug.Assert(DG.GetDependents("b") == new List<string> { "l", "o", "p" });

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
