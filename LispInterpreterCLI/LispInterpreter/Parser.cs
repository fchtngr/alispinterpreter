using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LispInterpreter
{
    class Parser
    {
        Scanner scanner;
        String curToken;

        public Parser(Scanner s)
        {
            this.scanner = s;
        }

        public Expression Parse()
        {
            curToken = scanner.NextToken();
            return Read();
        }

        private Expression Read()
        {
            switch (curToken)
            {
                case "(":
                    Expression exp = new LispExpressionImpl();
                    while ((curToken = scanner.NextToken())!= ")")
                    {
                        exp.Add(Read());
                    }
                    
                    return exp;
                case ")":
                    throw new LispException(LispErrorType.PARENTHESIS_MISMATCH, "unexpected closing parenthesis!");
                default:
                    return readAtom();
            }
        }

        // convert numbers, everything else is symbol
        private Expression readAtom()
        {
            try
            {
                //try to convert to a decimal, if it works it is a number, otherwise it must be a symbol
                Convert.ToDecimal(curToken);
                return new LispExpressionImpl(LispType.NUMBER, curToken);
            }
            catch (FormatException)
            {
                //obviously not a number, everything else is a symbol
                return new LispExpressionImpl(LispType.SYMBOL, curToken);
            }
        }
    }

    class Scanner
    {
        String input;
        char ch;
        int i;
        
        public void scan(string input)
        {
            ValidateInput(input);

            input = input.Replace("(", " ( ");
            input = input.Replace(")", " ) ");
            this.input = input;
            i = 0;

            

            NextChar();
        }

        public String NextToken()
        {
            //skip whitespaces
            while (ch <= ' ')
            {
                NextChar();
            }

            string token = "";
            do
            {
                token += ch;
                NextChar();
            } while (ch > ' ');
            return token;
        }
        
        private void NextChar()
        {
            if (i >= input.Length) throw new LispException(LispErrorType.UNEXPECTED_EOF, "unexpected eof!");
            ch = input.ElementAt(i++);
        }

        private void ValidateInput(string input)
        {
            if (!input.StartsWith("(") || !input.EndsWith(")"))
            {
                throw new LispException(LispErrorType.PARENTHESIS_MISMATCH, "input must start and end with a parenthesis!");
            }

            Stack<char> opens = new Stack<char>();
            foreach (char c in input)
            {
                if (c == '(')
                {
                    opens.Push(c);
                }
                else if (c == ')')
                {
                    if (opens.Count == 0)
                    {   //error, stack is empty therefore there are too many closing parentheses
                        throw new LispException(LispErrorType.PARENTHESIS_MISMATCH, "you closed too many parentheses!");
                    }
                    opens.Pop();
                }
            }
            if (opens.Count == 1)
            {   //error, one closing parenthesis is missing
                throw new LispException(LispErrorType.PARENTHESIS_MISMATCH, "missing closing parenthis!");
            }
            else if (opens.Count > 1)
            {   //error, more than one parentheses are missing
                throw new LispException(LispErrorType.PARENTHESIS_MISMATCH, "missing closing parentheses!");
            }
        }
    }
}
