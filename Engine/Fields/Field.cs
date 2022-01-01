using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine.Fields
{
    public abstract class Field : IField
    {
        public abstract string Name { get; }

        public abstract object Params { get; }

        public abstract void Execute(params object[] param);
    }
}
