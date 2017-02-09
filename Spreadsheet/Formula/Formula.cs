// Skeleton written by Joe Zachary for CS 3500, January 2017
// Code filled in by Levi McDonald u1039824 Jan 2017

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Formulas
{
    /// <summary>
    /// Represents formulas written in standard infix notation using standard precedence
    /// rules.  Provides a means to evaluate Formulas.  Formulas can be composed of
    /// non-negative floating-point numbers, variables, left and right parentheses, and
    /// the four binary operator symbols +, -, *, and /.  (The unary operators + and -
    /// are not allowed.)
    /// </summary>
    public struct Formula
    {
        List<string> baseList;
        /// <summary>
        /// Creates a Formula from a string that consists of a standard infix expression composed
        /// from non-negative floating-point numbers (using C#-like syntax for double/int literals), 
        /// variable symbols (a letter followed by zero or more letters and/or digits), left and right
        /// parentheses, and the four binary operator symbols +, -, *, and /.  White space is
        /// permitted between tokens, but is not required.
        /// 
        /// Examples of a valid parameter to this constructor are:
        ///     "2.5e9 + x5 / 17"
        ///     "(5 * 2) + 8"
        ///     "x*y-2+35/9"
        ///     
        /// Examples of invalid parameters are:
        ///     "_"
        ///     "-5.3"
        ///     "2 5 + 3"
        /// 
        /// If the formula is syntacticaly invalid, throws a FormulaFormatException with an 
        /// explanatory Message.
        /// </summary>
        public Formula(String formula)
        {
            if (formula == null)
            {
                baseList = new List<string> { "0" };
            }
            else
            {
                baseList = new List<string>();
                baseList = syntaxCheck(formula);
            }
        }
        public Formula(String formula, Normalizer norm, Validator valid)
        {
            if (formula == null)
            {
                baseList = new List<string> { "0" };
                try
                {
                    baseList = syntaxCheck(norm("0"));
                }
                catch
                {
                    throw new FormulaFormatException("Incorrect syntax for normalized formula");
                }
                if (valid == null)
                { }
                else if (!valid(norm("0")))
                {
                    throw new FormulaFormatException("formula not valid for Validator input");
                }
            }
            else
            {
                baseList = new List<string>();
                try
                {
                    baseList = syntaxCheck(norm(formula));
                }
                catch
                {
                    throw new FormulaFormatException("Incorrect syntax for normalized formula");
                }
                if (valid == null)
                { }
                else if (!valid(norm(formula)))
                {
                    throw new FormulaFormatException("formula not valid for Validator input");
                }
            }
            
        }
        public ISet<string> GetVariables()
        {
            string allLetters = @"[a-z]|[A-Z]";
            Regex letters = new Regex( allLetters);
            HashSet<string> myVariables = new HashSet<string>();
            foreach(string x in baseList)                
            {
                string firstElement = char.ToString(x[0]);
                if(letters.IsMatch(firstElement))
                {
                    myVariables.Add(x);
                }
            }
            return myVariables;
        }
        public List<string> syntaxCheck(string formula)
        {
            IEnumerable<string> tokens = GetTokens(formula);
            List<string> tokenList = new List<string>();
            foreach (string x in tokens)//Turned the Ienumerable tokens into a more malleable List
            {
                tokenList.Add(x);
            }
            int leftParenthCounter = 0;//ints to keep track of the number of ()
            int rightParenthCount = 0;
            if (tokenList.Count == 0)//checks to make sure there was input
                throw new FormulaFormatException("Incorrect Syntax for Formula");
            bool lastTokenWasOperator = true;//A bool to keep track of the type of the last token
            double value;
            for (int i = 0; i < tokenList.Count; i++)//loop to go through all the tokens
            {
                if (double.TryParse(tokenList[i], out value))//checks to see if the token is a double
                {
                    if (lastTokenWasOperator)
                        lastTokenWasOperator = false;
                    else
                        throw new FormulaFormatException("Incorrect Syntax for Formula");
                }
                else if (char.IsLetter(tokenList[i][0]))//checks to see if the first char in the token is a letter to denote a variable
                {
                    if (lastTokenWasOperator)
                        lastTokenWasOperator = false;
                    else
                        throw new FormulaFormatException("Incorrect Syntax for Formula");
                }
                else if ((tokenList[i] == "/") || (tokenList[i] == "*") || (tokenList[i] == "+") || (tokenList[i] == "-"))
                {//Checks for operators
                    if (lastTokenWasOperator)
                        throw new FormulaFormatException("Incorrect Syntax for Formula");
                    else
                        lastTokenWasOperator = true;
                }
                else if ((tokenList[i] == "("))//Checks for Parentheses and adds to the appropriate counter
                {
                    if (!lastTokenWasOperator)
                        throw new FormulaFormatException("Incorrect Syntax for Formula");
                    leftParenthCounter++;
                }
                else if (tokenList[i] == ")")
                {
                    if (lastTokenWasOperator)
                        throw new FormulaFormatException("Incorrect Syntax for Formula");
                    rightParenthCount++;
                    if (rightParenthCount > leftParenthCounter)
                        throw new FormulaFormatException("Too many right Parentheses");
                }
                else//if the token fell into none of these catagories it is not recognized
                    throw new FormulaFormatException("Incorrect Syntax for Formula");
                
            }
            if (leftParenthCounter != rightParenthCount)//after the loop is done, it makes sure the parentheses match up
            {
                throw new FormulaFormatException("Incorrect Parentheses");
            }
            if (lastTokenWasOperator)
            {
                throw new FormulaFormatException("Cannot end formula with an operator");
            }
            return tokenList;
        }
        public override string ToString()
        {
            string returnString = "";
            foreach(string x in baseList)
            {
                returnString = returnString + x;
            }
            return returnString;
        }
        /// <summary>
        /// Evaluates this Formula, using the Lookup delegate to determine the values of variables.  (The
        /// delegate takes a variable name as a parameter and returns its value (if it has one) or throws
        /// an UndefinedVariableException (otherwise).  Uses the standard precedence rules when doing the evaluation.
        /// 
        /// If no undefined variables or divisions by zero are encountered when evaluating 
        /// this Formula, its value is returned.  Otherwise, throws a FormulaEvaluationException  
        /// with an explanatory Message.
        /// </summary>
        public double Evaluate(Lookup lookup)
        {
            Stack<double> rands = new Stack<double>();
            Stack<string> rators = new Stack<string>();
            double value;
            foreach (string x in this.baseList)//a loop to move through the baseList
            {
                if (double.TryParse(x, out value))//checks for doubles, if found, checks for higher order operators and 
                    if(rators.Count > 0 && rands.Count > 0)//does the correct function
                    {
                        double value2 = rands.Pop();
                        string oper = rators.Pop();
                        if(oper == "*")
                        {
                            double pushValue = value2 * value;
                            rands.Push(pushValue);
                        }
                        else if(oper == "/")
                        {
                            double pushValue = value2 / value;
                            if (double.IsInfinity(pushValue))
                            {
                                throw new FormulaEvaluationException("Can't divide by 0");
                            }
                            rands.Push(pushValue);                          
                        }
                        else
                        {
                            rands.Push(value2);
                            rators.Push(oper);
                            rands.Push(value);
                        }
                    }
                    else
                    {
                        rands.Push(value);
                    }

                else if (x == "+" || x == "-")
                {
                    
                    if (rators.Count > 0)
                    {
                        string oper = rators.Pop();
                        if(oper == "+")
                        {
                            double num1 = rands.Pop();
                            double num2 = rands.Pop();
                            double pushValue = num2 + num1;
                            rands.Push(pushValue);
                        }
                        else if (oper == "-")
                        {
                            double num1 = rands.Pop();
                            double num2 = rands.Pop();
                            double pushValue = num2 - num1;
                            rands.Push(pushValue);
                        }
                        else
                        {
                            rators.Push(oper);
                        }
                        
                    }
                    rators.Push(x);
                }
                else if (x == "*" || x == "/" || x == "(")
                {
                    rators.Push(x);
                }
                else if (x == ")")
                {
                    string oper = rators.Pop();
                    if (oper == "+")
                    {
                        double num1 = rands.Pop();
                        double num2 = rands.Pop();
                        double pushValue = num2 + num1;
                        rands.Push(pushValue);
                        rators.Pop();                      
                    }
                    else if (oper == "-")
                    {
                        double num1 = rands.Pop();
                        double num2 = rands.Pop();
                        double pushValue = num2 - num1;
                        rands.Push(pushValue);
                        rators.Pop();
                    }
                    else if (oper == "(")
                    {}
                    if (rators.Count > 0)
                    {
                        oper = rators.Pop();
                        if (oper == "*")
                        {
                            double num1 = rands.Pop();
                            double num2 = rands.Pop();
                            double pushValue = num2 * num1;
                            rands.Push(pushValue);
                        }
                        else if (oper == "/")
                        {
                            double num1 = rands.Pop();
                            double num2 = rands.Pop();
                            double pushValue = num2 / num1;
                            if (double.IsInfinity(pushValue))
                            {
                                throw new FormulaEvaluationException("Can't divide by 0");
                            }
                            rands.Push(pushValue);
                        }
                        else if(oper == "(")
                        {
                            rators.Push(oper);
                        }
                        else if (oper == "+")
                        {
                            rators.Push(oper);
                        }
                        else if (oper == "-")
                        {
                            rators.Push(oper);
                        }
                    }

                }
                else
                {
                    try {lookup(x); }//at this point in the code if it throws a varible exception it turns it into
                    //an evaluation exeption
                    catch
                    {
                        throw new FormulaEvaluationException(x);
                    }
                    double myValue = lookup(x);
                    
                    if (rators.Count > 0 && rands.Count > 0)
                    {
                        double value2 = rands.Pop();
                        string oper = rators.Pop();
                        if (oper == "*")
                        {
                            double pushValue = value2 * myValue;
                            rands.Push(pushValue);
                        }
                        else if (oper == "/")
                        {
                            double pushValue = value2 / myValue;
                            rands.Push(pushValue);
                        }
                        else
                        {
                            rands.Push(value2);
                            rators.Push(oper);
                            rands.Push(myValue);
                        }

                    }
                    else
                    {
                        rands.Push(myValue);
                    }
                }
            }
            if (rators.Count == 0)//finishes by popping the last value or doing the last operation
                return rands.Pop();
            else
            {
                double returnValue = 0;
                double num1 = rands.Pop();
                double num2 = rands.Pop();
                string oper = rators.Pop();
                if (oper == "+")
                {
                    returnValue = num2 + num1;
                }
                else
                {
                    returnValue = num2 - num1;
                }
                return returnValue;
            }
                      
        }

        /// <summary>
        /// Given a formula, enumerates the tokens that compose it.  Tokens are left paren,
        /// right paren, one of the four operator symbols, a string consisting of a letter followed by
        /// zero or more digits and/or letters, a double literal, and anything that doesn't
        /// match one of those patterns.  There are no empty tokens, and no token contains white space.
        /// </summary>
        private static IEnumerable<string> GetTokens(String formula)
        {
            // Patterns for individual tokens
            String lpPattern = @"\(";
            String rpPattern = @"\)";
            String opPattern = @"[\+\-*/]";
            String varPattern = @"[a-zA-Z][0-9a-zA-Z]*";
            // PLEASE NOTE:  I have added white space to this regex to make it more readable.
            // When the regex is used, it is necessary to include a parameter that says
            // embedded white space should be ignored.  See below for an example of this.
            String doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: e[\+-]?\d+)?";
            String spacePattern = @"\s+";

            // Overall pattern.  It contains embedded white space that must be ignored when
            // it is used.  See below for an example of this.
            String pattern = String.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
                                            lpPattern, rpPattern, opPattern, varPattern, doublePattern, spacePattern);

            // Enumerate matching tokens that don't consist solely of white space.
            // PLEASE NOTE:  Notice the second parameter to Split, which says to ignore embedded white space
            /// in the pattern.
            foreach (String s in Regex.Split(formula, pattern, RegexOptions.IgnorePatternWhitespace))
            {
                if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline))
                {
                    yield return s;
                }
            }
        }
    }

    /// <summary>
    /// A Lookup method is one that maps some strings to double values.  Given a string,
    /// such a function can either return a double (meaning that the string maps to the
    /// double) or throw an UndefinedVariableException (meaning that the string is unmapped 
    /// to a value. Exactly how a Lookup method decides which strings map to doubles and which
    /// don't is up to the implementation of the method.
    /// </summary>
    public delegate string Normalizer (string s);
    public delegate bool Validator(string s);
    public delegate double Lookup(string var);

    /// <summary>
    /// Used to report that a Lookup delegate is unable to determine the value
    /// of a variable.
    /// </summary>
    [Serializable]
    public class UndefinedVariableException : Exception
    {
        /// <summary>
        /// Constructs an UndefinedVariableException containing whose message is the
        /// undefined variable.
        /// </summary>
        /// <param name="variable"></param>
        public UndefinedVariableException(String variable)
            : base(variable)
        {
        }
    }

    /// <summary>
    /// Used to report syntactic errors in the parameter to the Formula constructor.
    /// </summary>
    [Serializable]
    public class FormulaFormatException : Exception
    {
        /// <summary>
        /// Constructs a FormulaFormatException containing the explanatory message.
        /// </summary>
        public FormulaFormatException(String message) : base(message)
        {
        }
    }

    /// <summary>
    /// Used to report errors that occur when evaluating a Formula.
    /// </summary>
    [Serializable]
    public class FormulaEvaluationException : Exception
    {
        /// <summary>
        /// Constructs a FormulaEvaluationException containing the explanatory message.
        /// </summary>
        public FormulaEvaluationException(String message) : base(message)
        {
        }
    }    
}