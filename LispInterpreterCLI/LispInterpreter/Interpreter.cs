using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Globalization;

namespace LispInterpreter
{
    public interface Interpreter
    {
        string Interpret(string input);
        
        Environment Env { get; }
        Expression LastExpression { get; }
    }

    public class InterpreterImpl : Interpreter
    {
        private Environment env;
        private Expression lastExpression;
        private Scanner scanner;
        private Parser parser;
        private delegate Expression ProcDelegate(List<Expression> exp);
       
        public InterpreterImpl()
        {
            env = new EnvironmentImpl(null);
            addGlobals(env);
            scanner = new Scanner();
            parser = new Parser(scanner);
        }

        public Environment Env
        {
            get { return env; }
        }

        public Expression LastExpression
        {
            get { return lastExpression; }
        }

        public string Interpret(string input)
        {
            scanner.scan(input);
            lastExpression = parser.Parse();
            return Eval(lastExpression, env).ToString();
        }
        
        private Expression Eval(Expression exp, Environment env)
        {
            if (exp.Type == LispType.SYMBOL)
            {
                //find containing environment
                Environment container = env.Find(exp.Val);
                //find symbol in current environment
                return container[exp.Val];
            }

            if (exp.Type == LispType.NUMBER)
            {
                //numbers evaluate to themselves
                return exp;
            }

            if (exp[0].Type == LispType.SYMBOL)
            {
                if (exp[0].Val == "quote")
                {
                    // (quote exp) return expression as is
                    return exp[1];
                }

                if (exp[0].Val == "if") // (if test cons alt)
                {
                    //conditional
                    return Eval(Eval(exp[1], env).Val == "true" ? exp[2] : exp[3], env);
                }

                if (exp[0].Val == "set")
                {
                    // find the correct environment
                    Environment e = env.Find(exp[1].Val);
                    // update the value 
                    e[exp[1].Val] = Eval(exp[2], env);
                    // return the new value
                    return e[exp[1].Val];
                }

                if (exp[0].Val == "define")
                {
                    //define a variable/lambda expression in the current environment
                    return env[exp[1].Val] = Eval(exp[2], env);
                }

                if (exp[0].Val == "lambda")
                {
                    exp.Type = LispType.LAMBDA;
                    //save environment
                    exp.Env = env;
                    return exp;
                }
            }

            if (exp.Type == LispType.LIST)
            {
                // first expression must be either a lambda expression or a built in procedure
                Expression e = Eval(exp[0], env);

                // the remaining list elements are the parameters for the lambda expression / procedure
                List<Expression> pars = new List<Expression>();
                for (int i = 1/*!*/; i < exp.ChildCount; i++)
                {
                    pars.Add(Eval(exp[i], env));
                }

                if (e.Type == LispType.LAMBDA)
                {
                    //eval the body of the lambda expression
                    //create the environment for the lambda expression
                    return Eval(e[2], new EnvironmentImpl(e[1].Childs, pars, e.Env));
                }
                else if (e.Type == LispType.PROC)
                {
                    //execute procedure
                    ProcDelegate proc = (ProcDelegate)e.Proc;
                    return proc(pars);
                }
            }
            
            throw new Exception("undefined type " + exp[0].Val);
        }

        private void addGlobals(Environment env)
        {
            // arithmetic
            env["+"] = new LispExpressionImpl(LispType.PROC, new ProcDelegate(AddProc), "add multiple values"); //add
            env["-"] = new LispExpressionImpl(LispType.PROC, new ProcDelegate(SubProc), "subtract multiple values from first value"); //sub
            env["*"] = new LispExpressionImpl(LispType.PROC, new ProcDelegate(MulProc), "multiply multiple values"); //mul
            env["/"] = new LispExpressionImpl(LispType.PROC, new ProcDelegate(DivProc), "divide first value through the remaining values"); //div

            env["sin"] = new LispExpressionImpl(LispType.PROC, 
                new ProcDelegate((list) => {return new LispExpressionImpl(LispType.NUMBER, Math.Sin(Convert.ToDouble(list[0].Val)) + "");}), 
                "calculate sinus, takes one argument"); //sin
            env["cos"] = new LispExpressionImpl(LispType.PROC, 
                new ProcDelegate((list) => {return new LispExpressionImpl(LispType.NUMBER, Math.Cos(Convert.ToDouble(list[0].Val)) + "");}), 
                "calculate cosinus, takes one argument"); //cos
            
            // comparisons
            env[">"] = new LispExpressionImpl(LispType.PROC, new ProcDelegate(GrtProc), "value 1 greater than value 2"); //greater
            env["<"] = new LispExpressionImpl(LispType.PROC, new ProcDelegate(LetProc), "value 1 lesser than value 2"); //lesser
            env[">="] = new LispExpressionImpl(LispType.PROC, new ProcDelegate(GeqProc), "value 1 greater or equal value 2"); //greater or equal
            env["<="] = new LispExpressionImpl(LispType.PROC, new ProcDelegate(LeqProc), "value 1 lesser or equal value 2"); //lesser or equal
            env["="] = new LispExpressionImpl(LispType.PROC, new ProcDelegate(EquProc), "value 1 equals value 2"); //equal

            //list processing
            env["list"] = new LispExpressionImpl(LispType.PROC, ListProc , "create a list of the parameters"); //make a list
            env["first"] = new LispExpressionImpl(LispType.PROC, new ProcDelegate((list) => { return list[0].Childs[0]; }), "return first element of a list"); //return first
            env["rest"] = new LispExpressionImpl(LispType.PROC, RestProc, "return the rest of a list (without the head"); //return rest
            env["cons"] = new LispExpressionImpl(LispType.PROC, ConsProc, "add values to the beginning of a list"); //add elem to list
        }

        // built in procedures --------------------------------------------

        // try some lambda expressions
        // target list is last param
        ProcDelegate ConsProc = (list) =>
        {
            Expression exp = new LispExpressionImpl(LispType.LIST, "list");
            exp.Childs = list[list.Count - 1].Childs;
            for (int i = list.Count - 2; i >= 0; i--)
            {
                exp.Childs.Insert(0, list[i]);
            }
            return exp;
        };

        //return rest of a list (the list without the head element that is)
        ProcDelegate RestProc = (list) =>
        {
            Expression exp = new LispExpressionImpl(LispType.LIST, "list");
            list[0].Childs.RemoveAt(0);
            exp.Childs = list[0].Childs;
            return exp;
        };
        
        // list -> return a new lisp expression containing the arguments as childs
        ProcDelegate ListProc = (list) =>
        {
            Expression exp = new LispExpressionImpl(LispType.LIST, "list");
            exp.Childs = list;
            return exp;
        };
        
        private Expression AddProc(List<Expression> exps)
        {
            decimal sum = 0;
            foreach (Expression l in exps)
            {
                sum += Convert.ToDecimal(l.Val);
            }
            return new LispExpressionImpl(LispType.NUMBER, sum + "");
        }

        private Expression MulProc(List<Expression> exps)
        {
           decimal prod = Convert.ToDecimal(exps[0].Val);
            for (int i = 1; i < exps.Count; i++)
            {
                prod *= Convert.ToDecimal(exps[i].Val);
            }
            return new LispExpressionImpl(LispType.NUMBER, prod + "");
        }

        private Expression DivProc(List<Expression> exps)
        {
            decimal prod = Convert.ToDecimal(exps[0].Val);
            for (int i = 1; i < exps.Count; i++)
            {
               decimal dividend = Convert.ToDecimal(exps[i].Val);
                prod /= dividend;
            }
            return new LispExpressionImpl(LispType.NUMBER, prod + "");
        }

        private Expression SubProc(List<Expression> exps)
        {
            decimal sum = Convert.ToDecimal(exps[0].Val);
            for (int i = 1; i < exps.Count; i++)
            {
              sum -= Convert.ToDecimal(exps[i].Val);
            }
            return new LispExpressionImpl(LispType.NUMBER, sum + "");
        }

        private Expression GrtProc(List<Expression> exps)
        {
            decimal val1 = Convert.ToDecimal(exps[0].Val);
            decimal val2 = Convert.ToDecimal(exps[1].Val);
            return val1 > val2 ? new LispExpressionImpl(LispType.SYMBOL, "true") : new LispExpressionImpl(LispType.SYMBOL, "false");
        }

        public Expression GeqProc(List<Expression> exps)
        {
            decimal val1 = Convert.ToDecimal(exps[0].Val);
            decimal val2 = Convert.ToDecimal(exps[1].Val);
            return val1 >= val2 ? new LispExpressionImpl(LispType.SYMBOL, "true") : new LispExpressionImpl(LispType.SYMBOL, "false");
        }

        private Expression LeqProc(List<Expression> exps)
        {
            decimal val1 = Convert.ToDecimal(exps[0].Val);
            decimal val2 = Convert.ToDecimal(exps[1].Val);
            return val1 <= val2 ? new LispExpressionImpl(LispType.SYMBOL, "true") : new LispExpressionImpl(LispType.SYMBOL, "false");
        }

        private Expression LetProc(List<Expression> exps)
        {
           decimal val1 = Convert.ToDecimal(exps[0].Val);
            decimal val2 = Convert.ToDecimal(exps[1].Val);
            return val1 < val2 ? new LispExpressionImpl(LispType.SYMBOL, "true") : new LispExpressionImpl(LispType.SYMBOL, "false");
        }

        private Expression EquProc(List<Expression> exps)
        {
            decimal val1 = Convert.ToDecimal(exps[0].Val);
            decimal val2 = Convert.ToDecimal(exps[1].Val);
            return val1 == val2 ? new LispExpressionImpl(LispType.SYMBOL, "true") : new LispExpressionImpl(LispType.SYMBOL, "false");
        }
    }
}
