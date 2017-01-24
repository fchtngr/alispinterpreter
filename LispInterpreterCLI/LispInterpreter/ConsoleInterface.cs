using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Globalization;

namespace LispInterpreter
{
    class ConsoleInterface
    {
        static void Main(string[] args)
        {
            Interpreter interpreter = new InterpreterImpl();
            while (true)
            {
                try
                {
                    Console.Write("ttl>");
                    string input = Console.ReadLine();
                    Console.WriteLine(interpreter.Interpret(input));
                }
                catch (Exception e)
                {   
                    Console.WriteLine("error> " + e.Message);
                }
            }
        }
    }
}
