using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine.Mods
{
    public class ModManager
    {
        private Dictionary<string, ModData> mods;
        public Dictionary<string, ModData> Mods
        {
            get { return mods; }
        }

        public ModManager()
        {
            mods = new Dictionary<string, ModData>();
        }

        public void LoadMods()
        {

        }
    }
}