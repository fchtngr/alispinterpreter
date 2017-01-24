using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LispInterpreter
{
    public interface Environment
    {
        Expression this[string key] { get; set; }
        Environment Find(string var);
        Dictionary<string, Expression> Dictionary { get; }
    }

    class EnvironmentImpl : Environment
    {
        private Environment _outer;
        private Dictionary<string, Expression> env = new Dictionary<string, Expression>();

        public Dictionary<string, Expression> Dictionary
        {
            get { return this.env; }
        }

        public EnvironmentImpl(Environment outer)
        {
            _outer = outer;
        }

        public EnvironmentImpl(List<Expression> pars, List<Expression> values, Environment outer) : this(outer)
        {
            for (int i = 0; i < pars.Count; i++)
            {
                env[pars[i].Val] = values[i];
            }
        }

        /// <summary>
        /// Find the environment which holds the variable.
        /// </summary>
        /// <param name="var"></param>
        /// <returns>The environment containing the variable.</returns>
        public Environment Find(string var) 
        {
            if (env.ContainsKey(var))
            {
                return this;
            }
            else
            {
                if (_outer == null)
                {   // var can not be found anywhere
                    throw new LispException(LispErrorType.SYMBOL_NOT_FOUND, "symbol " + var +" can not be found in the environment!");
                }
                return _outer.Find(var);
            }
        }

        public Expression this[string key]
        {
            set 
            {
                if (env.ContainsKey(key))
                {
                    //overwrite
                    env.Remove(key);
                }
                env.Add(key, value); 
            }
            get { return env[key]; }
        }
    }
}
