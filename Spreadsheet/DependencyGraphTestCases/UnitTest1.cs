using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dependencies;
using System.Diagnostics;
using System.Collections.Generic;
namespace DependencyGraphTestCases
{
    [TestClass]
    public class UnitTest1
    {/// <summary>
     /// This tests to make sure that if no dependencies are added then the size is 0
     /// </summary>
        [TestMethod]
        public void TestConstructionSize()
        {
            DependencyGraph testGraph = new DependencyGraph();
            Debug.Assert(testGraph.Size == 0);
        }
        /// <summary>
        /// Tests to make sure the expected size is returned after using the AddDependency method
        /// </summary>
        [TestMethod]
        public void TestAddDependecy()
        {
            DependencyGraph testGraph = new DependencyGraph();
            testGraph.AddDependency("a", "1");
            testGraph.AddDependency("b", "2");
            testGraph.AddDependency("c", "3");
            testGraph.AddDependency("d", "4");
            testGraph.AddDependency("e", "5");
            testGraph.AddDependency("a", "1");//this line should not add any to the size as it is already present
            testGraph.AddDependency("a", "10");
            testGraph.AddDependency("b", "10");
            Debug.Assert(testGraph.Size == 7);
        }/// <summary>
         /// Tests if the HasDependees method returns the correct bool value
         /// </summary>
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
            Debug.Assert(!testGraph.HasDependees("10"));// this line checks for a non-existant Dependee

        }
        /// <summary>
        /// Tests the HasDependents method and makes sure it returns the correct bool values
        /// </summary>
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
            Debug.Assert(!testGraph.HasDependents("ff"));// this line checks for a non existant Dependent

        }
        /// <summary>
        /// Tests the GetDependents Method and sees if Dependees with none, one, or more dependents 
        /// have those dependents returned properly in an IEnumerable
        /// </summary>
        [TestMethod]
        public void TestGetDependents()
        {
            DependencyGraph testGraph = new DependencyGraph();
            testGraph.AddDependency("a", "1");
            testGraph.AddDependency("b", "2");
            testGraph.AddDependency("c", "3");
            testGraph.AddDependency("d", "4");
            testGraph.AddDependency("e", "5");

            IEnumerable<string> dependantList = testGraph.GetDependents("a");
            foreach (string x in dependantList)//testing all the IEnuerables to make sure they have the right value
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
            testGraph.AddDependency("a", "100");//testing for mulitiple dependiencies
            dependantList = testGraph.GetDependents("a");
            string testString = "1";
            foreach (string x in dependantList)
            {
                Debug.Assert(x == testString);
                testString = testString + "0";
            }
            testGraph.AddDependency("b", "10");
            testGraph.AddDependency("b", "100");//testing for multiple dependencies and dependents
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
        /// <summary>
        /// Tests the GetDependees Method and sees if Dependents with none, one, or more dependenees 
        /// have those dependees returned properly in an IEnumerable
        /// </summary>
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
        /// <summary>
        /// Tests if the RemoveDependency method removes the dependencies and changes the size properly
        /// </summary>
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
            Debug.Assert(testGraph.Size == 4);
            Debug.Assert(testGraph.HasDependees("1") == false);
            Debug.Assert(testGraph.HasDependents("a") == false);

        }
        /// <summary>
        /// Tests the ReplaceDependents Method checking for dependents with more than one dependee and 
        /// dependees with more than one dependent. And if both of those are involved at the same time.
        /// </summary>
        [TestMethod]
        public void TestReplaceDependents()
        {
            DependencyGraph testGraph = new DependencyGraph();
            testGraph.AddDependency("a", "1");
            testGraph.AddDependency("b", "2");
            testGraph.AddDependency("c", "3");
            testGraph.AddDependency("d", "4");
            testGraph.AddDependency("e", "5");
            testGraph.AddDependency("a", "10");// "a" has multiple dependents
            testGraph.AddDependency("a", "100");
            List<string> newDependents = new List<string> { "20", "200", "2000" };
            testGraph.ReplaceDependents("a", newDependents);//gave "a" new dependents
            IEnumerable<string> returnedDependents = testGraph.GetDependents("a");
            string testString = "20";
            foreach (string x in returnedDependents)//loop making sure "a" has all three of the right dependants
            {
                Debug.Assert(x == testString);
                testString = testString + "0";
            }
            IEnumerable<string> returnedDependees = testGraph.GetDependees("1");

            testGraph.AddDependency("d", "5");//testing for a dependent having more than one dependee
            newDependents.Clear();
            newDependents.Add("5");
            newDependents.Add("50");
            newDependents.Add("500");
            newDependents.Add("5000");
            testGraph.ReplaceDependents("e", newDependents);//"e" gets new dependents
            returnedDependees = testGraph.GetDependees("5");
            newDependents.Clear();
            foreach (string x in returnedDependees)
            {
                newDependents.Add(x);
            }
            Debug.Assert(newDependents[0] == "d");
            Debug.Assert(newDependents[1] == "e");
        }
        /// <summary>
        /// Test the ReplaceDependees method on checking for dependees with more than one dependent and 
        /// dependents with more than one dependee. And if both of those are involved at the same time.
        /// </summary>
        [TestMethod]
        public void TestReplaceDependees()
        {
            DependencyGraph testGraph = new DependencyGraph();
            testGraph.AddDependency("a", "1");
            testGraph.AddDependency("b", "2");
            testGraph.AddDependency("c", "3");
            testGraph.AddDependency("d", "4");
            testGraph.AddDependency("d", "5");//this is to check to have a dependant with multiple dependees
            testGraph.AddDependency("e", "5");
            IEnumerable<string> returnedDependees = testGraph.GetDependees("dog");//checking random string to make sure it returns null

            List<string> newDependees = new List<string> { "a", "aa", "aaa" };
            testGraph.ReplaceDependees("1", newDependees);
            returnedDependees = testGraph.GetDependees("1");
            string testString = "a";
            foreach (string x in returnedDependees)
            {
                Debug.Assert(x == testString);
                testString = testString + "a";
            }
            testGraph.ReplaceDependees("5", newDependees);
            returnedDependees = testGraph.GetDependees("5");
            testString = "a";
            foreach (string x in returnedDependees)
            {
                Debug.Assert(x == testString);
                testString = testString + "a";
            }


        }
        /// <summary>
        /// This will stress test the ReplaceDependents and the AddDependency methods just to see how long they take
        /// </summary>
        [TestMethod]
        public void StressTestReplaceDepenents()
        {
            DependencyGraph testGraph = new DependencyGraph();
            int theDependant = 0;
            for (int i = 0; i < 10000; i++)
            {
                testGraph.AddDependency("a", theDependant.ToString());
                theDependant += 1;
            }
            List<string> newDependentList = new List<string>();
            for (int i = 15000; i > 0; i--)
            {
                newDependentList.Add(i.ToString());
            }
            testGraph.ReplaceDependents("a", newDependentList);
        }
        [TestMethod]
        public void StressTestReplaceDepenees()
        {
            DependencyGraph testGraph = new DependencyGraph();
            int theDependee = 0;
            for (int i = 0; i < 10000; i++)
            {
                testGraph.AddDependency(theDependee.ToString(), "a");
                theDependee += 1;
            }
            List<string> newDependeeList = new List<string>();
            for (int i = 15000; i > 0; i--)
            {
                newDependeeList.Add(i.ToString());
            }
            testGraph.ReplaceDependees("a", newDependeeList);
        }
        [TestMethod]
        public void TestCopyGraph()
        {
            DependencyGraph f = new DependencyGraph();
            f.AddDependency("a", "1");
            f.AddDependency("b", "2");
            f.AddDependency("c", "3");
            DependencyGraph g = new DependencyGraph(f);
            Assert.IsTrue(g.ToString() == f.ToString());
            Assert.IsTrue(g.GetDependents("a") == f.GetDependents("a"));

        }
        [TestMethod]
        public void NullGetDependentsDependeesTest()
        {
            DependencyGraph f = new DependencyGraph();
            f.AddDependency("a", "1");
            f.AddDependency("a", "2");
            f.AddDependency("a", "3");
            Assert.IsTrue(f.GetDependents("b").GetEnumerator().MoveNext() == false);
            Assert.IsTrue(f.GetDependees("4").GetEnumerator().MoveNext() == false);

        }

        }
    }
