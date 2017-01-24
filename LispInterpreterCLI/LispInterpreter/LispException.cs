using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LispInterpreter
{
    public enum LispErrorType
    {
        SYMBOL_NOT_FOUND, PARENTHESIS_MISMATCH, UNEXPECTED_EOF, UNEXPECTED_LISPTYPE
    }

    public class LispException : SystemException
    {
        public LispErrorType ErrorType
        {
            get;
            set;
        }

        public LispException(LispErrorType type, string message) : base(message)
        {
            ErrorType = type;
        }
    }
}
