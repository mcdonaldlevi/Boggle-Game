using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dependencies;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace DependencyGraphTestCases
{
    [TestClass]
    public class DependencyGraphTestCases
    {
        /// <summary>
        /// Checks if correct output is given with HasDependents function
        /// </summary>
        [TestMethod]
        public void HasDependentsTest1()
        {
        DependencyGraph DG = new DependencyGraph();
            DG.AddDependency("a", "b");
            Debug.Assert(DG.HasDependents("a"));
        }

        /// <summary>
        /// Checks if undefined token is given correct output
        /// </summary>
        [TestMethod]
        public void HasDependentsFailTest1()
        {
        DependencyGraph DG = new DependencyGraph();
            DG.AddDependency("a", "b");
            Debug.Assert(!DG.HasDependents("c"));
        }

        /// <summary>
        /// Checks if Dependents and Dependees are not mixed up
        /// </summary>
        [TestMethod]
        public void HasDependentsFailTest2()
        {
            DependencyGraph DG = new DependencyGraph();
            DG.AddDependency("c", "b");
            Debug.Assert(!DG.HasDependents("b"));
        }

        /// <summary>
        /// Checks if correct output is given with HasDependees function
        /// </summary>
        [TestMethod]
        public void HasDependeesTest()
        {
            DependencyGraph DG = new DependencyGraph();
            DG.AddDependency("e", "f");
            Debug.Assert(DG.HasDependees("f"));
        }

        /// <summary>
        /// Checks if correct output is given with HasDependees function
        /// </summary>
        [TestMethod]
        public void HasDependeesFailTest1()
        {
            DependencyGraph DG = new DependencyGraph();
            DG.AddDependency("e", "f");
            Debug.Assert(!DG.HasDependees("n"));
        }

        /// <summary>
        /// Checks if Dependents and Dependees are not mixed up
        /// </summary>
        [TestMethod]
        public void HasDependeesFailTest2()
        {
            DependencyGraph DG = new DependencyGraph();
            DG.AddDependency("g", "f");
            Debug.Assert(!DG.HasDependees("g"));
        }

        /// <summary>
        /// Checks if correct output is given with GetDependees function
        /// </summary>
        [TestMethod]
        public void GetDependentsTest()
        {
            DependencyGraph DG = new DependencyGraph();
            DG.AddDependency("a", "b");
            DG.AddDependency("a", "c");
            DG.AddDependency("a", "d");
            DG.AddDependency("a", "e");

            List<string> expected = new List<string> { "b", "c", "d", "e" };
            List<string> results = DG.GetDependents("a").ToList();

            for (int i = 0; i < expected.Count; i++)
            {
                Debug.Assert(results[i] == expected[i]);
            }
        }

        /// <summary>
        /// Checks if correct output is given with GetDependees function
        /// </summary>
        [TestMethod]
        public void GetDependeesTest()
        {
            DependencyGraph DG = new DependencyGraph();
            DG.AddDependency("b", "a");
            DG.AddDependency("c", "a");
            DG.AddDependency("d", "a");
            DG.AddDependency("e", "a");
            List<string> expected = new List<string> { "b", "c", "d", "e" };
            List<string> results = DG.GetDependees("a").ToList();

            for (int i = 0; i < expected.Count; i++)
            {
                Debug.Assert(results[i] == expected[i]);
            }
        }

        /// <summary>
        /// Checks if dependees have been removed from Dependees list
        /// </summary>
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

        /// <summary>
        /// Checks if dependents have been removed from Dependents list
        /// </summary>
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

        /// <summary>
        /// Checks if correct size is given after adding, removing, and replacing
        /// </summary>
        [TestMethod]
        public void CorrectSizeTest()
        {
            DependencyGraph DG = new DependencyGraph();
            DG.AddDependency("c", "b");
            DG.AddDependency("a", "f");
            DG.AddDependency("l", "d");
            DG.AddDependency("a", "p");

            DG.RemoveDependency("c", "b");
            DG.RemoveDependency("l", "d");

            DG.ReplaceDependents("a", new List<string> { "d", "m", "z" });

            Debug.Assert(DG.Size == 3);
        }

        /// <summary>
        /// Checks if correct output is given with GetDependents function
        /// </summary>
        [TestMethod]
        public void ReplaceDependentsTest()
        {
            DependencyGraph DG = new DependencyGraph();
            DG.AddDependency("a", "b");
            DG.AddDependency("c", "b");
            DG.AddDependency("d", "e");
            DG.AddDependency("f", "e");

            List<string> expected = new List<string> { "l", "o", "p" };
            DG.ReplaceDependents("b", expected);
            List<string> results = DG.GetDependents("b").ToList();
            
            for (int i = 0; i < expected.Count; i++)
            {
                Debug.Assert(results[i] == expected[i]);
            }
        }

        /// <summary>
        /// Checks if correct output is given with GetDependenes function
        /// </summary>
        [TestMethod]
        public void ReplaceDependeesTest()
        {
            DependencyGraph DG = new DependencyGraph();
            DG.AddDependency("a", "b");
            DG.AddDependency("a", "c");
            DG.AddDependency("d", "e");
            DG.AddDependency("d", "f");

            List<string> expected = new List<string> { "p", "q", "z", "o", "y" };
            DG.ReplaceDependees("a", expected);
            List<string> results = DG.GetDependees("a").ToList();

            for (int i = 0; i < expected.Count; i++)
            {
                Debug.Assert(results[i] == expected[i]);
            }

        }
    }
}
