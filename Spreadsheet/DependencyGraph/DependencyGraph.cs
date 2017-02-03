// Skeleton implementation written by Joe Zachary for CS 3500, January 2017.

using System;
using System.Collections.Generic;

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
        Dictionary<string, List<string>> dependees = new Dictionary<string, List<string>>();
        Dictionary<string, List<string>> dependents = new Dictionary<string, List<string>>();
        int size = 0;
        /// <summary>
        /// Creates a DependencyGraph containing no dependencies.
        /// </summary>
        public DependencyGraph()
        {
        }

        /// <summary>
        /// The number of dependencies in the DependencyGraph.
        /// </summary>
        public int Size()
        {
            return size;
        }

        /// <summary>
        /// Reports whether dependents(s) is non-empty.  Requires s != null.
        /// </summary>
        public bool HasDependents(string s)
        {
            if (dependees.ContainsKey(s))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Reports whether dependees(s) is non-empty.  Requires s != null.
        /// </summary>
        public bool HasDependees(string s)
        {
            if (dependents.ContainsKey(s))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Enumerates dependents(s).  Requires s != null.
        /// </summary>
        public IEnumerable<string> GetDependents(string s)
        {
            if (dependees.ContainsKey(s))
            {
                return (dependees[s]);

            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Enumerates dependees(s).  Requires s != null.
        /// </summary>
        public IEnumerable<string> GetDependees(string s)
        {
            if (dependents.ContainsKey(s))
            {
                return dependents[s];
            }
            else
                return null;
        }

        /// <summary>
        /// Adds the dependency (s,t) to this DependencyGraph.
        /// This has no effect if (s,t) already belongs to this DependencyGraph.
        /// Requires s != null and t != null.
        /// </summary>
        public void AddDependency(string s, string t)
        {
            if (dependees.ContainsKey(s))
            {
                if (dependees[s].Contains(t))
                {
                    return;
                }
                else
                {
                    dependees[s].Add(t);
                    if (dependents.ContainsKey(t))
                    {
                        dependents[t].Add(s);
                    }
                    else
                    {
                        List<string> myDependeeList = new List<string> { s };
                        dependents.Add(t, myDependeeList);
                    }
                    size += 1;
                }
            }
            else
            {
                List<string> dependentList = new List<string>();
                dependentList.Add(t);
                dependees.Add(s, dependentList);
                if (dependents.ContainsKey(t))
                {
                    dependents[t].Add(s);
                }
                else
                {
                    List<string> dependeeList = new List<string>();
                    dependeeList.Add(s);
                    dependents.Add(t, dependeeList);
                }
                size += 1;
            }
        }

        /// <summary>
        /// Removes the dependency (s,t) from this DependencyGraph.
        /// Does nothing if (s,t) doesn't belong to this DependencyGraph.
        /// Requires s != null and t != null.
        /// </summary>
        public void RemoveDependency(string s, string t)
        {
            if (dependees.ContainsKey(s))
            {
                if (dependees[s].Contains(t))
                {
                    dependees[s].Remove(t);
                    dependents[t].Remove(s);
                    if (dependees[s].Count == 0)
                    {
                        dependees.Remove(s);
                    }
                    if (dependents[t].Count == 0)
                    {
                        dependents.Remove(t);
                    }
                    size -= 1;
                }
            }
        }

        /// <summary>
        /// Removes all existing dependencies of the form (s,r).  Then, for each
        /// t in newDependents, adds the dependency (s,t).
        /// Requires s != null and t != null.
        /// </summary>
        public void ReplaceDependents(string s, IEnumerable<string> newDependents)
        {
            if (dependees.ContainsKey(s))
            {
                foreach(string x in dependees[s])
                {
                    if(dependents.ContainsKey(x))
                    {
                        dependents[x].Remove(s);
                        size -= 1;
                        if (dependents[x].Count == 0)
                        {
                            dependents.Remove(x);
                        }
                    }
                }
                List<string> dependentsList = new List<string>();
                foreach (string x in newDependents)
                    dependentsList.Add(x);
                dependees[s] = dependentsList;

                foreach (string x in newDependents)
                {
                    if (dependents.ContainsKey(x))
                    {
                        dependents[x].Add(s);
                    }
                    else
                    {
                        List<string> addList = new List<string> { s };
                        dependents.Add(x, addList);
                    }
                    size += 1;
                }
            }
        }

        /// <summary>
        /// Removes all existing dependencies of the form (r,t).  Then, for each 
        /// s in newDependees, adds the dependency (s,t).
        /// Requires s != null and t != null.
        /// </summary>
        public void ReplaceDependees(string t, IEnumerable<string> newDependees)
        {
            if (dependents.ContainsKey(t))
            {
                foreach (string x in dependents[t])
                {
                    if (dependees.ContainsKey(x))
                    {
                        dependees[x].Remove(t);
                        size -= 1;
                        if (dependees[x].Count == 0)
                        {
                            dependees.Remove(x);
                        }
                    }
                }
                List<string> dependeesList = new List<string>();
                foreach (string x in newDependees)
                    dependeesList.Add(x);
                dependents[t] = dependeesList;

                foreach (string x in newDependees)
                {
                    if (dependees.ContainsKey(x))
                    {
                        dependees[x].Add(t);
                    }
                    else
                    {
                        List<string> addList = new List<string> { t };
                        dependees.Add(x, addList);
                        
                    }
                    size += 1;
                }
            }
        }
    }
}
