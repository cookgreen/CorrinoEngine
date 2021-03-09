using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine
{
    public class Argument
    {
        private Dictionary<string, string> argDic;

        public int Length
        {
            get { return argDic.Count; }
        }

        public Argument(string[] args)
        {
            argDic = new Dictionary<string, string>();
            foreach (var arg in args)
            {
                string[] tokens = arg.Split('=');
                argDic[tokens[0]] = tokens[1];
            }
        }

        public bool Contains(string key)
        {
            return argDic.ContainsKey(key);
        }

        public string GetArgumentParameter(string key)
        {
            return argDic[key];
        }
    }
}
