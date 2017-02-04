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

            List<string> expected = new List<string> { "b", "c", "d", "e" };
            List<string> results = DG.GetDependees("a").ToList();

            for (int i = 0; i < expected.Count; i++)
            {
                Debug.Assert(results[i] == expected[i]);
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

            List<string> expected = new List<string> { "l", "o", "p" };
            DG.ReplaceDependents("b", expected);
            List<string> results = DG.GetDependents("b").ToList();
            
            for (int i = 0; i < expected.Count; i++)
            {
                Debug.Assert(results[i] == expected[i]);
            }

        }

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

        [TestMethod]
        [ExpectedException(typeof(InvalidFormatException))]
        public void AddInvalidDependent()
        {
            DependencyGraph DG = new DependencyGraph();
            DG.AddDependency("1", "c");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidFormatException))]
        public void AddInvalidDependee()
        {
            DependencyGraph DG = new DependencyGraph();
            DG.AddDependency("f", "c#");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidFormatException))]
        public void InvalidParameter1()
        {
            DependencyGraph DG = new DependencyGraph();
            DG.AddDependency("s", "c");
            DG.GetDependees("1a").First();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidFormatException))]
        public void InvalidParameter2()
        {
            DependencyGraph DG = new DependencyGraph();
            DG.AddDependency("vcx", "we32");
            DG.GetDependents("33").First();
        }
    }
}
