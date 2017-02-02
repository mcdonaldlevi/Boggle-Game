using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dependencies;
using System.Diagnostics;
using System.Collections.Generic;
namespace DependencyGraphTestCases
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestConstructionSize()
        {
            DependencyGraph testGraph = new DependencyGraph();
            Debug.Assert(testGraph.Size() == 0);
        }
        [TestMethod]
        public void TestAddDependecy()
        {
            DependencyGraph testGraph = new DependencyGraph();
            testGraph.AddDependency("a", "1");
            testGraph.AddDependency("b", "2");
            testGraph.AddDependency("c", "3");
            testGraph.AddDependency("d", "4");
            testGraph.AddDependency("e", "5");
            testGraph.AddDependency("a", "1");
            testGraph.AddDependency("a", "10");
            testGraph.AddDependency("b", "10");
            Debug.Assert(testGraph.Size() == 7);
        }
        [TestMethod]
        public void TestHasDependees()
        {
            DependencyGraph testGraph = new DependencyGraph();
            testGraph.AddDependency("a", "1");
            testGraph.AddDependency("b", "2");
            testGraph.AddDependency("c", "3");
            testGraph.AddDependency("d", "4");
            testGraph.AddDependency("e", "5");
            Debug.Assert(testGraph.HasDependees("1"));
            Debug.Assert(testGraph.HasDependees("2"));
            Debug.Assert(testGraph.HasDependees("3"));
            Debug.Assert(testGraph.HasDependees("4"));
            Debug.Assert(testGraph.HasDependees("5"));

        }
        [TestMethod]
        public void TestHasDependents()
        {
            DependencyGraph testGraph = new DependencyGraph();
            testGraph.AddDependency("a", "1");
            testGraph.AddDependency("b", "2");
            testGraph.AddDependency("c", "3");
            testGraph.AddDependency("d", "4");
            testGraph.AddDependency("e", "5");
            Debug.Assert(testGraph.HasDependents("a"));
            Debug.Assert(testGraph.HasDependents("b"));
            Debug.Assert(testGraph.HasDependents("c"));
            Debug.Assert(testGraph.HasDependents("d"));
            Debug.Assert(testGraph.HasDependents("e"));

        }
        [TestMethod]
        public void TestGetDependents()
        {
            DependencyGraph testGraph = new DependencyGraph();
            testGraph.AddDependency("a", "1");
            testGraph.AddDependency("b", "2");
            testGraph.AddDependency("c", "3");
            testGraph.AddDependency("d", "4");
            testGraph.AddDependency("e", "5");
            IEnumerable<string> nullList = testGraph.GetDependents("dog");
            Debug.Assert(nullList == null);
            IEnumerable<string> dependantList = testGraph.GetDependents("a");
            foreach (string x in dependantList)
                Debug.Assert(x == "1");
            dependantList = testGraph.GetDependents("b");
            foreach (string x in dependantList)
                Debug.Assert(x == "2");
            dependantList = testGraph.GetDependents("c");
            foreach (string x in dependantList)
                Debug.Assert(x == "3");
            dependantList = testGraph.GetDependents("d");
            foreach (string x in dependantList)
                Debug.Assert(x == "4");
            dependantList = testGraph.GetDependents("e");
            foreach (string x in dependantList)
                Debug.Assert(x == "5");
            testGraph.AddDependency("a", "10");
            testGraph.AddDependency("a", "100");
            dependantList = testGraph.GetDependents("a");
            string testString = "1";
            foreach (string x in dependantList)
            {
                Debug.Assert(x == testString);
                testString = testString + "0";
            }
            testGraph.AddDependency("b", "10");
            testGraph.AddDependency("b", "100");
            dependantList = testGraph.GetDependents("b");
            List<string> dependantIndexList = new List<string>();
            foreach (string x in dependantList)
            {
                dependantIndexList.Add(x);
            }
            Debug.Assert(dependantIndexList[0] == "2");
            Debug.Assert(dependantIndexList[1] == "10");
            Debug.Assert(dependantIndexList[2] == "100");

        }
        [TestMethod]
        public void TestGetDependees()
        {
            DependencyGraph testGraph = new DependencyGraph();
            testGraph.AddDependency("1", "a");
            testGraph.AddDependency("2", "b");
            testGraph.AddDependency("3", "c");
            testGraph.AddDependency("4", "d");
            testGraph.AddDependency("5", "e");
            IEnumerable<string> dependeeList = testGraph.GetDependees("a");
            foreach (string x in dependeeList)
                Debug.Assert(x == "1");
            dependeeList = testGraph.GetDependees("b");
            foreach (string x in dependeeList)
                Debug.Assert(x == "2");
            dependeeList = testGraph.GetDependees("c");
            foreach (string x in dependeeList)
                Debug.Assert(x == "3");
            dependeeList = testGraph.GetDependees("d");
            foreach (string x in dependeeList)
                Debug.Assert(x == "4");
            dependeeList = testGraph.GetDependees("e");
            foreach (string x in dependeeList)
                Debug.Assert(x == "5");
            testGraph.AddDependency("10", "a");
            testGraph.AddDependency("100", "a");
            dependeeList = testGraph.GetDependees("a");
            string testString = "1";
            foreach (string x in dependeeList)
            {
                Debug.Assert(x == testString);
                testString = testString + "0";
            }
        }
        [TestMethod]
        public void TestRemoveDependency()
        {
            DependencyGraph testGraph = new DependencyGraph();
            testGraph.AddDependency("a", "1");
            testGraph.AddDependency("b", "2");
            testGraph.AddDependency("c", "3");
            testGraph.AddDependency("d", "4");
            testGraph.AddDependency("e", "5");
            testGraph.RemoveDependency("a", "1");
            Debug.Assert(testGraph.Size() == 4);
            Debug.Assert(testGraph.HasDependees("1") == false);
            Debug.Assert(testGraph.HasDependents("a") == false);
            
        }
        [TestMethod]
        public void TestReplaceDependents()
        {
            DependencyGraph testGraph = new DependencyGraph();
            testGraph.AddDependency("a", "1");
            testGraph.AddDependency("b", "2");
            testGraph.AddDependency("c", "3");
            testGraph.AddDependency("d", "4");
            testGraph.AddDependency("e", "5");
            testGraph.AddDependency("a", "10");
            testGraph.AddDependency("a", "100");
            List<string> newDependents = new List<string> { "20", "200", "2000" };
            testGraph.ReplaceDependents("a", newDependents);
            IEnumerable<string> returnedDependents = testGraph.GetDependents("a");
            string testString = "20";
            foreach(string x in returnedDependents)
            {
                Debug.Assert(x == testString);
                testString = testString + "0";
            }
            IEnumerable<string> returnedDependees = testGraph.GetDependees("1");
            Debug.Assert(returnedDependees == null);
        }
        [TestMethod]
        public void TestReplaceDependees()
        {
            DependencyGraph testGraph = new DependencyGraph();
            testGraph.AddDependency("a", "1");
            testGraph.AddDependency("b", "2");
            testGraph.AddDependency("c", "3");
            testGraph.AddDependency("d", "4");
            testGraph.AddDependency("e", "5");
            testGraph.AddDependency("a", "10");
            testGraph.AddDependency("a", "100");
            List<string> newDependees = new List<string> { "aa", "aaa", "aaaa" };
            testGraph.ReplaceDependees("1", newDependees);
            IEnumerable<string> returnedDependees = testGraph.GetDependees("1");
            string testString = "aa";
            foreach (string x in returnedDependees)
            {
                Debug.Assert(x == testString);
                testString = testString + "a";
            }
        }
    }
    }
