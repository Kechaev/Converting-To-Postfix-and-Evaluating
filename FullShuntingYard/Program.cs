using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FullShuntingYard
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter the expression with spaces between tokens");
            Console.WriteLine("Example:");
            Console.WriteLine("> 5 * ( 3 + 2 )");
            Console.WriteLine("===============================================");
            while (true)
            {
                Console.Write("> ");
                string input = Console.ReadLine();

                if (!ValidInput(input))
                {
                    Console.WriteLine($"Invalid entry. Try again");
                }
                else
                {
                    string[] expression = input.Split(' ');

                    string[] postfix = ConvertToRPN(expression);

                    foreach (string token in postfix)
                    {
                        Console.Write($"{token} ");
                    }
                    Console.WriteLine();

                    string evaluation = Evaluate(postfix);

                    Console.WriteLine($"Evaluation: {evaluation}");
                }
            }
        }

        static bool ValidInput(string expression)
        {
            string pattern = @"^([-+/*]\d+(\.\d+)?)*";
            Regex rg = new Regex(pattern);
            return rg.IsMatch(expression);
        }

        static bool IsNumber(string input)
        {
            string pattern = @"^-?[0-9]+(\.[0-9]+)?$";
            Regex rg = new Regex(pattern);
            return rg.IsMatch(input);
        }

        static bool IsBoolean(string input)
        {
            return input.ToLower() == "true" || input.ToLower() == "false";
        }

        static string[] ConvertToRPN(string[] expression)
        {
            List<string> output = new List<string>();
            Stack<string> stack = new Stack<string>();

            for (int i = 0; i < expression.Length; i++)
            {
                string token = expression[i];
                if (IsNumber(token))
                {
                    output.Add(token);
                }
                else if (IsUnary(token))
                {
                    stack.Push(token);
                }
                else if (IsBinary(token))
                {
                    while ((stack.Count > 0) && ((Precedence(token) <= Precedence(stack.Peek())) || IsUnary(stack.Peek()) && (stack.Peek() != "(") && IsLeftAssociative(token)))
                    {
                        output.Add(stack.Pop());
                    }
                    stack.Push(token);
                }
                else if (token == "(")
                {
                    stack.Push("(");
                }
                else if (token == ")" && stack.Count > 0)
                {
                    while (stack.Peek() != "(")
                    {
                        output.Add(stack.Pop());
                    }
                    stack.Pop(); 
                    if (IsUnary(stack.Peek()))
                    {
                        output.Add(stack.Pop());
                    }
                }
            }

            while (stack.Count > 0)
            {
                if (stack.Peek() == "(")
                {
                    Console.WriteLine("Closing bracket is missing");
                }

                output.Add(stack.Pop());
            }

            return output.ToArray();
        }

        static bool IsLeftAssociative(string op)
        {
            return op != "^";
        }

        static int Precedence(string op)
        {
            switch (op)
            {
                case "!":
                    return 8;
                case "^":
                    return 7;
                case "*":
                case "/":
                case "%":
                     return 6;
                case "+":
                case "-":
                    return 5;
                case "<=":
                case "<":
                case ">=":
                case ">":
                    return 4;
                case "==":
                case "!=":
                    return 3;
                case "&":
                    return 2;
                case "|":
                    return 1;
                default:
                    return -1;
            }
        }

        static bool IsUnary(string op)
        {
            string[] unaryOps = { "!", "sin", "cos", "tan", "cot", "sec", "cosec", "arcsin", "arccos", "arctan", "sinh", "cosh", "tanh" };
            return unaryOps.Contains(op);
        }

        static bool IsBinary(string op)
        {
            string[] binaryOp = { "+", "-", "*", "/", "^", "==", "!=", ">=", "<=", "<", ">", "%", "&", "|" };
            return binaryOp.Contains(op);
        }

        static string Evaluate(string[] expression)
        {
            string object1, object2;
            Stack<string> stack = new Stack<string>();
            string result;

            for (int i = 0; i < expression.Length; i++)
            {
                string token = expression[i];
                if (IsNumber(token))
                {
                    stack.Push(token);
                }
                else if (IsBinary(token))
                {
                    object2 = stack.Pop();
                    object1 = stack.Pop();
                    if (IsNumber(object1) && IsNumber(object2))
                    {
                        string[] mathematicalOperation = { "+", "-", "*", "/", "^", "%" };
                        string[] comparisonOperation = { "==", "!=", ">", "<", ">=", "<=" };
                        if (mathematicalOperation.Contains(token))
                        {
                            result = NumericalBinaryOperation(Convert.ToDouble(object1), Convert.ToDouble(object2), token).ToString();
                        }
                        else if (comparisonOperation.Contains(token))
                        {
                            result = ComparisonOperation(Convert.ToDouble(object1), Convert.ToDouble(object2), token).ToString();
                        }
                        else
                        {
                            result = "ERROR";
                            throw new Exception("ERROR: Invalid operation");
                        }
                    }
                    else if (IsBoolean(object1) && IsBoolean(object2))
                    {
                        result = BooleanBinaryOperation(Convert.ToBoolean(object1), Convert.ToBoolean(object2), token).ToString();
                    }
                    else
                    {
                        result = "ERROR";
                        throw new Exception("ERROR: Invalid values");
                    }
                    stack.Push(result);
                }
                else if (IsUnary(token))
                {
                    object1 = stack.Pop();
                    if (IsNumber(object1))
                    {
                        result = NumericalUnaryOperation(Convert.ToDouble(object1), token).ToString();
                    }
                    else if (IsBoolean(object1))
                    {
                        result = BooleanUnaryOperation(Convert.ToBoolean(object1), token).ToString();
                    }
                    else
                    {
                        result = "ERROR";
                        throw new Exception("ERROR: Invalid values");
                    }
                    stack.Push(result);
                }

                try
                {
                    Convert.ToDouble(stack.Peek());

                    double num = Convert.ToDouble(stack.Pop());

                    if (num < 0.0001)
                    {
                        stack.Push("0");
                    }
                    else
                    {
                        stack.Push(num.ToString());
                    }
                }
                catch
                {
                    // Do nothing
                }
            }

            return stack.Pop();
        }

        static double NumericalBinaryOperation(double a, double b, string op)
        {
            switch (op)
            {
                case "+":
                    return a + b;
                case "-":
                    return a - b;
                case "*":
                    return a * b;
                case "/":
                    return a / b;
                case "%":
                    return a % b;
                case "^":
                    return Math.Pow(a, b);
                default:
                    throw new Exception("ERROR: Invalid operation");
            }
        }

        static double NumericalUnaryOperation(double a, string op)
        {
            double result;
            string[] trigonometricOperations = { "sin", "cos", "tan", "sec", "cosec", "cot" };
            string[] hyperbolicTrigonometricOperations = { "sinh", "cosh", "tanh" };
            string[] inverseTrigonometricOperations = { "arcsin", "arccos", "arctan" };
            // Conver Degree -> Radians
            if (trigonometricOperations.Contains(op) || hyperbolicTrigonometricOperations.Contains(op)) 
            {
                a = a / 180 * Math.PI;
            }
            else if (!inverseTrigonometricOperations.Contains(op))
            {
                throw new Exception($"ERROR: Invalid operation");
            }
            switch (op)
            {
                case "sin":
                    result = Math.Sin(a);
                    break;
                case "cos":
                    result = Math.Cos(a);
                    break;
                case "tan":
                    result = Math.Tan(a);
                    break;
                case "sec":
                    result = 1 / Math.Cos(a);
                    break;
                case "cosec":
                    result = 1 / Math.Sin(a);
                    break;
                case "cot":
                    result = 1 / Math.Tan(a);
                    break;
                case "arcsin":
                    result = Math.Asin(a);
                    break;
                case "arccos":
                    result = Math.Acos(a);
                    break;
                case "arctan":
                    result = Math.Atan(a);
                    break;
                case "sinh":
                    return Math.Sinh(a);
                case "cosh":
                    result = Math.Cosh(a);
                    break;
                case "tanh":
                    result = Math.Tanh(a);
                    break;
                default:
                    throw new Exception($"ERROR: Invalid operation");
            }
            if (inverseTrigonometricOperations.Contains(op))
            {
                a = a / Math.PI * 180;
            }
            return a;
        }

        static bool ComparisonOperation(double a, double b, string op)
        {
            switch (op)
            {
                case "==":
                    return a == b;
                case "!=":
                    return a != b;
                case ">=":
                    return a >= b;
                case ">":
                    return a > b;
                case "<=":
                    return a <= b;
                case "<":
                    return a > b;
                default:
                    throw new Exception("ERROR: Invalid operation");

            }
        }

        static bool BooleanBinaryOperation(bool a, bool b, string op)
        {
            switch (op)
            {
                case "&":
                    return a & b;
                case "|":
                    return a | b;
                default:
                    throw new Exception("ERROR: Invalid operation");
            }
        }

        static bool BooleanUnaryOperation(bool a, string op)
        {
            switch (op)
            {
                case "!":
                    return !a;
                default:
                    throw new Exception($"ERROR: Invalid operation");
            }
        }
    }
}
