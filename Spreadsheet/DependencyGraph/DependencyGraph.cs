// Skeleton implementation written by Joe Zachary for CS 3500, January 2017.
// Skeleton fleshed out by Levi McDonald January 2017
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
        private Dictionary<string, List<string>> dependees = new Dictionary<string, List<string>>();
        private Dictionary<string, List<string>> dependents = new Dictionary<string, List<string>>();
        private int size = 0;
        //dependees is a dictionary where the keys are Dependees and they connect to a list of their dependents
        //dependents is a dictionary where the keys are dependents and they connect to a list of their dependees
        //every time a dependency is added or removed, size is altered accordingly.
        /// <summary>
        /// Creates a DependencyGraph containing no dependencies.
        /// </summary>
        public DependencyGraph()
        {
        }
        public DependencyGraph(DependencyGraph copyFromGraph)
        {
            this.dependees = copyFromGraph.dependees;
            this.dependents = copyFromGraph.dependents;            
        }

        /// <summary>
        /// The number of dependencies in the DependencyGraph.
        /// </summary>
        public int Size
        {
            get
            {
                return size;
            }
            
        }

        /// <summary>
        /// Reports whether dependents(s) is non-empty. if s == null throws an Argument Null Exception
        /// </summary>
        public bool HasDependents(string s)
        {
            if(s == null)
            {
                throw new ArgumentNullException();
            }
            if (dependees.ContainsKey(s))//checks to see if s is a dependee
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Reports whether dependees(s) is non-empty.  if s == null throws an Argument Null Exception
        /// </summary>
        public bool HasDependees(string s)
        {

            if (s == null)
            {
                throw new ArgumentNullException();
            }
            if (dependents.ContainsKey(s))//checks to see if s is a dependent
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Enumerates dependents(s).  if s == null throws an Argument Null Exception
        /// </summary>
        public IEnumerable<string> GetDependents(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException();
            }
            if (dependees.ContainsKey(s))//checks to see if s is a dependee and then returns s's list of dependents
            {
                return (dependees[s]);
            }
            else
            {
                List<string> nullList = new List<string>();
                IEnumerable<string> nullEnumerable = nullList;
                return nullEnumerable;
            }
        }

        /// <summary>
        /// Enumerates dependees(s).  if s == null throws an Argument Null Exception
        /// </summary>
        public IEnumerable<string> GetDependees(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException();
            }
            if (dependents.ContainsKey(s))//checks to see if s is a dependent and if so returns its list of dependees
            {
                return dependents[s];
            }
            else
            {
                List<string> nullList = new List<string>();
                IEnumerable<string> nullEnumerable = nullList;
                return nullEnumerable;
            }
        }

        /// <summary>
        /// Adds the dependency (s,t) to this DependencyGraph.
        /// This has no effect if (s,t) already belongs to this DependencyGraph.
        /// if s or t == null throws an Argument Null Exception
        /// </summary>
        public void AddDependency(string s, string t)
        {
            if ((s == null)|| (t==null))
            {
                throw new ArgumentNullException();
            }
            if (dependees.ContainsKey(s))//checks to see if s already has a dependent
            {
                if (dependees[s].Contains(t))//if it does checks to see if it is the one already being asked for
                {
                    return;
                }
                else
                {
                    dependees[s].Add(t);//adds to the the dependent list attached to s
                    if (dependents.ContainsKey(t))//checks to see if t is already a dependent, if so adds s to its
                    {//dependee list
                        dependents[t].Add(s);
                    }
                    else//if not makes a new list composed of s and makes a new keyvalue pair for the dictionary
                    {
                        List<string> myDependeeList = new List<string> { s };
                        dependents.Add(t, myDependeeList);
                    }
                    size += 1;//increases size
                }
            }
            else//if s does not already exist, it creates a place in the dictionary for s
            {
                List<string> dependentList = new List<string>();
                dependentList.Add(t);
                dependees.Add(s, dependentList);
                if (dependents.ContainsKey(t))//same checks as above
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
        ///if s or t == null throws an Argument Null Exception
        /// </summary>
        public void RemoveDependency(string s, string t)
        {
            if ((s == null) || (t == null))
            {
                throw new ArgumentNullException();
            }
            if (dependees.ContainsKey(s))//checks to make sure s exists otherwise does nothing
            {
                if (dependees[s].Contains(t))//checks to make sure t exists in s
                {
                    dependees[s].Remove(t);//removes both from their realitive lists in the dictionaries
                    dependents[t].Remove(s);
                    if (dependees[s].Count == 0)//if the list is empty, this entirely removes the key value pair
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
        /// if s or t == null throws an Argument Null Exception
        /// </summary>
        public void ReplaceDependents(string s, IEnumerable<string> newDependents)
        {/// to remove all the dependents from s, first we need to take those dependents and remove their 
         /// connection to s in the dependent dictionary then remove them from s in the dependee dictionary
         ///  Afterwards, s gets connected
         /// to a new set of dependents, and then those dependents are all added to the dependent dictionary
            if ((s == null) || (newDependents == null))
            {
                throw new ArgumentNullException();
            }
            foreach(string x in newDependents)
            {
                if(x== null)
                {
                    throw new ArgumentNullException();
                }
            }
            if (dependees.ContainsKey(s))
            {
                
                foreach (string x in dependees[s])//going through s's dependents and finding them in the dependent 
                {//dictionary and removes s from their list
                    if(dependents.ContainsKey(x))
                    {
                        dependents[x].Remove(s);
                        size -= 1;
                        if (dependents[x].Count == 0)//if they were only dependent on s, it removes them from the 
                        {                              //dictionary entirely
                            dependents.Remove(x);
                        }
                    }
                }
                List<string> dependentsList = new List<string>();
                foreach (string x in newDependents)//takes the enumerable and makes it a list as required for the 
                    dependentsList.Add(x);          //keyvalue pair in the dictionary
                dependees[s] = dependentsList;       //the list that s is a key of it overwritten with the new Dependents

                foreach (string x in newDependents)//goes through the newDependents and adds them to the Dependent dictionary
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
        /// if s or t == null throws an Argument Null Exception
        /// </summary>
        public void ReplaceDependees(string t, IEnumerable<string> newDependees)
        {// this does the same exact thing as above, just in reverse
            if ((t == null) || (newDependees == null))
            {
                throw new ArgumentNullException();
            }
            foreach (string x in newDependees)
            {
                if (x == null)
                {
                    throw new ArgumentNullException();
                }
            }
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
                {
                    dependeesList.Add(x);
                    size += 1;
                }
             
                dependents[t] = dependeesList;

                foreach (string x in newDependees)
                {
                    if (dependees.ContainsKey(x))
                    {
                        dependents[x].Add(t);
                    }
                    size += 1;
                }
            }
        }
    }
}
