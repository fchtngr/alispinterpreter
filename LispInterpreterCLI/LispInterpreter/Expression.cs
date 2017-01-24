using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LispInterpreter
{
    public enum LispType { LIST, NUMBER, SYMBOL, PROC, LAMBDA }

    public interface Expression
    {
        LispType Type { get; set; }

        Delegate Proc { get; set; }

        Environment Env { get; set; }

        List<Expression> Childs { get; set; }

        string Documentation { get; }

        Expression this[int index] { get; }

        int ChildCount { get; }

        string Val { get; }

        void Add(Expression exp);
    }

    class LispExpressionImpl : Expression
    {
        private LispType _type;
        private string _val;
        private Delegate _proc;
        private Environment _env;
        private List<Expression> _childs = new List<Expression>();
        private string _doc;

        public LispType Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public string Val
        {
            get { return _val; }
        }

        public Environment Env
        {
            set { _env = value; }
            get { return _env; }
        }

        public Delegate Proc
        {
            get { return _proc; }
            set { _proc = value; }
        }

        public List<Expression> Childs
        {
            get { return _childs; }
            set { _childs = value; }
        }

        public int ChildCount
        {
            get { return _childs != null ? _childs.Count : 0; }
        }

        public string Documentation
        {
            get { return _doc; }
        }

        public LispExpressionImpl() { }

        public LispExpressionImpl(LispType type, string val)
        {
            _type = type;
            _val = val;
        }

        public LispExpressionImpl(LispType type, Delegate proc, string doc): this(type, proc)
        {
            _doc = doc;
        }

        public LispExpressionImpl(LispType type, Delegate proc)
        {
            _type = type;
            _proc = proc;
        }

        public Expression this[int index]
        {
            get { return _childs.ElementAt(index); }
        }

        public void Add(Expression exp)
        {
            _childs.Add(exp);
        }

        public override string ToString()
        {
            if (Type == LispType.LIST)
            {
                string s = "(";
                foreach (Expression e in Childs)
                {
                    s += e.ToString() + " ";
                }
                return s.Remove(s.Length - 1) + ')';
            }
            else if (Type == LispType.LAMBDA)
            {
                return "lambda";
            }
            else if (Type == LispType.PROC)
            {
                return "proc";
            }

            return Val;
        }
    }

    // just here to try out the expansion methods feature
    public static class ExtensionMethods
    {
        public static void PrettyPrint(this Expression exp, string prefix)
        {
            if (exp.Type == LispType.LIST)
            {
                Console.WriteLine("{0} + {1}", prefix, "Expression");
                foreach (Expression s in exp.Childs)
                    if (exp.Childs.IndexOf(s) == exp.Childs.Count - 1)
                        s.PrettyPrint(prefix + "    ");
                    else
                        s.PrettyPrint(prefix + "   |");
            }
            else
            {
                Console.WriteLine("{0} + {1}:{2}", prefix,exp.Type, exp.Val);
            }
        }
    }
}