using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine.Mods
{
    public class ModData
    {
        private string modID;
        public string ID
        {
            get { return modID; }
        }
        public ModManifest Manifest {get;set;}

        public ModData(string modID)
        {
            this.modID = modID;
        }
    }
}
