// Skeleton written by Joe Zachary for CS 3500, January 2017

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
    public class Formula
    {
        private Stack<string> operators = new Stack<string>();
        private Stack<double> values = new Stack<double>();
        private IEnumerable<string> formula_tokens;
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
            int left_paren = 0;
            int right_paren = 0;
            String last_val = "";
            Boolean first_value = true;
            formula_tokens = GetTokens(formula);
            foreach (var token in formula_tokens)
            {
                if (first_value)
                {
                    if (Regex.IsMatch(token, @"[-+/*)]"))
                    {
                        throw new FormulaFormatException("Cannot start formula with operand");
                    }
                    first_value = false;
                }
                if (Regex.IsMatch(token, @"[^a-z A-Z\d/*+\-()]") && !Regex.IsMatch(token, @"\d+.\d+"))
                {
                    throw new FormulaFormatException("Invalid token: " + token);
                }
                else if (Regex.IsMatch(token, @"[-+/*]") && Regex.IsMatch(last_val, @"[-+/*]"))
                {
                    throw new FormulaFormatException("Too many operators: " + last_val + token);
                }
                else if (Regex.IsMatch(token, @"[\da-zA-z]") && Regex.IsMatch(last_val, @"[\da-zA-z]"))
                {
                    throw new FormulaFormatException("Too many operands: " + last_val + token);
                }
                else if (Regex.IsMatch(last_val, @"[-+/*(]") && Regex.IsMatch(token, @"[-+/*)]"))
                {
                    throw new FormulaFormatException("Must have operand after operator or closing parenthesis");
                }
                else
                {
                    last_val = token;
                }
                if (token == "(")
                {
                    left_paren++;
                }
                else if (token == ")")
                {
                    right_paren++;
                }
            }
            if (last_val.Equals(""))
            {
                throw new FormulaFormatException("There must be at least one token");
            }
            if (right_paren != left_paren)
            {
                throw new FormulaFormatException("Too many right parentheses");
            }
            if (Regex.IsMatch(last_val, @"[-+/*]"))
            {
                throw new FormulaFormatException("Cannot end formula with operator");
                }
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
            double num;
            int paren = 0;
            String temp_op = "";
            
            foreach (String token in formula_tokens)
            {
                if (double.TryParse(token, out num))
                {
                    values.Push(num);
                }
                else if (token == "+" || token == "-" || token == "*" || token == "/" || token == "(")
                {
                    operators.Push(token);
                    if (token == "(")
                    {
                        paren++;
                    }
                }
                else if (token == ")")
                {
                    temp_op = operators.Pop();
                    while (temp_op != "(")
                    {
                        values.Push(OperateOnStacks(temp_op));
                        temp_op = operators.Pop();
                    }
                    paren--;
                    if (operators.Count > 0 && values.Count > (operators.Count - paren % 2))
                    {
                        if (operators.Peek().Equals("*") || operators.Peek().Equals("/"))
                        {
                            values.Push(OperateOnStacks(operators.Pop()));
                        }
                    }
                }
                else
                {
                    try
                    {
                        values.Push(lookup(token));
                    }
                    catch (UndefinedVariableException)
                    {
                        throw new FormulaEvaluationException("Variable not found");
                    }
                }
                if (operators.Count > 0 && values.Count > (operators.Count - paren % 2))
                {
                    if (operators.Peek().Equals("*") || operators.Peek().Equals("/"))
                    {
                        values.Push(OperateOnStacks(operators.Pop()));
                    }
                }
            }
            return EndOperation();
        }

        /// <summary>
        /// Private function takes in an operator string
        /// The last value of the stack is saved to use after, to not affect results from - and /
        /// Switch statements picks the right operation and returns the value
        /// </summary>
        private double OperateOnStacks(String op)
        {
            double temp_val;
            double last_val = values.Pop();
            switch (op)
            {
                case "+":
                    temp_val = values.Pop() + last_val;
                    return temp_val;
                case "-":
                    return values.Pop() - last_val;
                case "*":
                    return values.Pop() * last_val;
                case "/":
                    temp_val = values.Pop() / last_val;
                    if (Double.IsInfinity(temp_val))
                    {
                        throw new FormulaEvaluationException("Cannot divide by 0");
                    }
                    return temp_val;
            }
            return 0;
        }

        /// <summary>
        /// Private function takes in an operator string
        /// The last value of the stack is saved to use after, to not affect results from - and /
        /// Switch statements picks the right operation and returns the value
        /// </summary>
        private double EndOperation()
        {
            double temp_val;
            if (values.Count < 2)
            {
                return values.Pop();
            }
            temp_val = values.Pop();
            switch (operators.Pop())
            {
                case "+":
                    return EndOperation() + temp_val;
                case "-":
                    return EndOperation() - temp_val;
            }
            return 0;
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
