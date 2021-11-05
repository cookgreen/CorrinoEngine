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
        private string modFullPath;

        public string ID
        {
            get { return modID; }
        }
        public string FullPath
        {
            get { return modFullPath; }
        }
        public ModManifest Manifest {get;set;}

        public ModData(string modID, string modFullPath)
        {
            this.modID = modID;
            this.modFullPath = modFullPath;
        }
    }
}
