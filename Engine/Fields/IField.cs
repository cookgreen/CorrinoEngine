using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine.Fields
{
    public interface IField
    {
        string Name { get; }
        object Params { get; }
        void Execute(params object[] param);
    }
}
