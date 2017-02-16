﻿// Skeleton implementation written by Joe Zachary for CS 3500, January 2017.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Dependencies
{
    /// <summary>
    /// A DependencyGraph can be modeled as a set of dependencies, where a dependency is an ordered 
    /// pair of strings.  Two dependencies (s1,t1) and (s2,t2) are considered equal if and only if 
    /// s1 equals s2 and t1 equals t2.
    /// 
    /// Given a DependencyGraph DG:
    /// 
    ///    (1) If s is a string, the set of all strings t such that the dependency (s,t) is in DG 
    ///    is called the dependents of s, which we will denote as dependents(s).
    ///        
    ///    (2) If t is a string, the set of all strings s such that the dependency (s,t) is in DG 
    ///    is called the dependees of t, which we will denote as dependees(t).
    ///    
    /// The notations dependents(s) and dependees(s) are used in the specification of the methods of this class.
    ///
    /// For example, suppose DG = {("a", "b"), ("a", "c"), ("b", "d"), ("d", "d")}
    ///     dependents("a") = {"b", "c"}
    ///     dependents("b") = {"d"}
    ///     dependents("c") = {}
    ///     dependents("d") = {"d"}
    ///     dependees("a") = {}
    ///     dependees("b") = {"a"}
    ///     dependees("c") = {"a"}
    ///     dependees("d") = {"b", "d"}
    ///     
    /// All of the methods below require their string parameters to be non-null.  This means that 
    /// the behavior of the method is undefined when a string parameter is null.  
    ///
    /// IMPORTANT IMPLEMENTATION NOTE
    /// 
    /// The simplest way to describe a DependencyGraph and its methods is as a set of dependencies, 
    /// as discussed above.
    /// 
    /// However, physically representing a DependencyGraph as, say, a set of ordered pairs will not
    /// yield an acceptably efficient representation.  DO NOT USE SUCH A REPRESENTATION.
    /// 
    /// You'll need to be more clever than that.  Design a representation that is both easy to work
    /// with as well acceptably efficient according to the guidelines in the PS3 writeup. Some of
    /// the test cases with which you will be graded will create massive DependencyGraphs.  If you
    /// build an inefficient DependencyGraph this week, you will be regretting it for the next month.
    /// </summary>
    public class DependencyGraph
    {
        private int size;
        private Dictionary<string, List<string>> Dependents = new Dictionary<string, List<string>>();
        private Dictionary<string, List<string>> Dependees = new Dictionary<string, List<string>>();
        /// <summary>
        /// Creates a DependencyGraph containing no dependencies.
        /// </summary>
        public DependencyGraph()
        {
            size = 0;
        }

        /// <summary>
        /// The number of dependencies in the DependencyGraph.
        /// </summary>
        public int Size
        {
            get { return size; }
        }

        /// <summary>
        /// Reports whether dependents(s) is non-empty.  Requires s != null.
        /// </summary>
        public bool HasDependents(string s)
        {
            checkInputString(s);

            //Returns true if key is in Dependents and it is not empty
            if (Dependents.ContainsKey(s))
                if (Dependents[s].Count != 0)
                    return true;
            return false;
        }

        /// <summary>
        /// Reports whether dependees(s) is non-empty.  Requires s != null.
        /// </summary>
        public bool HasDependees(string s)
        {
            checkInputString(s);

            //Returns true if key is in Dependees and it is not empty
            if (Dependees.ContainsKey(s))
                if (Dependees[s].Count != 0)
                    return true;
            return false;
        }

        /// <summary>
        /// Enumerates dependents(s).  Requires s != null.
        /// </summary>
        public IEnumerable<string> GetDependents(string s)
        {
            checkInputString(s);

            foreach (string dependent in Dependents[s])
            {
                yield return dependent;
            }
        }

        /// <summary>
        /// Enumerates dependees(s).  Requires s != null.
        /// </summary>
        public IEnumerable<string> GetDependees(string s)
        {
            checkInputString(s);

            foreach (string dependee in Dependees[s])
            {
                yield return dependee;
            }
        }

        /// <summary>
        /// Adds the dependency (s,t) to this DependencyGraph.
        /// This has no effect if (s,t) already belongs to this DependencyGraph.
        /// Requires s != null and t != null.
        /// </summary>
        public void AddDependency(string s, string t)
        {
            checkInputString(s);
            checkInputString(t);

            //If first parameter is not added before, it creates a dependents and dependees list
            if (!Dependents.ContainsKey(s))
                Dependents.Add(s, new List<string>());
            if (!Dependees.ContainsKey(s))
                Dependees.Add(s, new List<string>());

            //If second parameter is not added before, it creates a dependents and dependees list
            if (!Dependents.ContainsKey(t))
                Dependents.Add(t, new List<string>());
            if (!Dependees.ContainsKey(t))
                Dependees.Add(t, new List<string>());

            //If it has not been added to the Dependents list yet, it is added
            if (!Dependents[s].Contains(t))
                Dependents[s].Add(t);
            //If it has not been added to the Dependees list yet, it is added
            if (!Dependees[t].Contains(s))
                Dependees[t].Add(s);

            size++;
        }

        /// <summary>
        /// Removes the dependency (s,t) from this DependencyGraph.
        /// Does nothing if (s,t) doesn't belong to this DependencyGraph.
        /// Requires s != null and t != null.
        /// </summary>
        public void RemoveDependency(string s, string t)
        {
            checkInputString(s);
            checkInputString(t);

            //Removed from both lists
            Dependents[s].Remove(t);
            Dependees[t].Remove(s);

            //If either value has no dependents or dependeees remaining, it is removed from the lists
            if (Dependents[t].Count == 0 && Dependees[t].Count == 0)
            {
                Dependents.Remove(t);
                Dependees.Remove(t);
            }
            if (Dependents[s].Count == 0 && Dependees[s].Count == 0)
            {
                Dependents.Remove(s);
                Dependees.Remove(s);
            }

            size--;
        }

        /// <summary>
        /// Removes all existing dependencies of the form (s,r).  Then, for each
        /// t in newDependents, adds the dependency (s,t).
        /// Requires s != null and t != null.
        /// </summary>
        public void ReplaceDependents(string s, IEnumerable<string> newDependents)
        {
            checkInputString(s);

            //Removes all dependees and decrements size
            foreach (string dependee in Dependees[s])
            {
                Dependents[dependee].Remove(s);
                size--;
            }

            //Clears list
            Dependees[s] = new List<string>();

            //Iterates ands every value from newDependents after checking input string is valid
            foreach (string dependent in newDependents)
            {
                checkInputString(dependent);
                AddDependency(dependent, s);
            }
        }

        /// <summary>
        /// Removes all existing dependencies of the form (r,t).  Then, for each 
        /// s in newDependees, adds the dependency (s,t).
        /// Requires s != null and t != null.
        /// </summary>
        public void ReplaceDependees(string t, IEnumerable<string> newDependees)
        {
            checkInputString(t);

            //Removes all dependents and decrements size
            foreach (string dependent in Dependents[t])
            {
                Dependees[dependent].Remove(t);
                size--;
            }

            //Clears list
            Dependents[t] = new List<string>();

            //Iterates ands every value from newDependees after checking input string is valid
            foreach (string dependee in newDependees)
            {
                checkInputString(dependee);
                AddDependency(t, dependee);
            }
        }

        /// <summary>
        /// If both dependent and dependency don't start with a letter,
        /// or contain a value that is not a letter or number,
        /// it will throw an exception
        /// </summary>
        /// <param name="input"></param>
        private void checkInputString(string input)
        {
            //if (input.Length == 0)
            //    throw new InvalidFormatException("Empty token");
            ////First regex statement verifies the first value of the string is a letter, second verfies that there is only numbers and letters
            //else if(!Regex.IsMatch(input.Substring(0, 1), @"[a-zA-Z]") || Regex.IsMatch(input, @"[^a-z A-Z\d]"))
            //    throw new InvalidFormatException("Invalid token");
        }
    }


    [Serializable]
    public class InvalidFormatException : Exception
    {
        /// <summary>
        /// Constructs an InvalidFormatException containing whose message is the
        /// undefined variable.
        /// </summary>
        /// <param name="variable"></param>
        public InvalidFormatException(String variable)
            : base(variable)
        {
        }
    }
}
