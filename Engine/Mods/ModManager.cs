using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine.Mods
{
    public class ModManager
    {
        private bool isLoaded;
        private Dictionary<string, ModData> mods;
        public Dictionary<string, ModData> Mods
        {
            get { return mods; }
        }

        private static ModManager instance;
        public static ModManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ModManager();
                }
                return instance;
            }
        }

        public ModManager()
        {
            mods = new Dictionary<string, ModData>();
            isLoaded = false;
        }

        public void LoadMods()
        {
            if(!isLoaded)
            {
                isLoaded = true;
            }

            mods.Clear();

            string modFolder = "mods";
            string modFullPath = Path.Combine(Environment.CurrentDirectory, modFolder);
            DirectoryInfo di = new DirectoryInfo(modFullPath);
            foreach (var folder in di.EnumerateDirectories())
            {
                string modYamlFile = Path.Combine(folder.FullName, "mod.yaml");
                if (File.Exists(modYamlFile))
                {
                    ModManifest modManifest = new ModManifest();
                    modManifest.Parse(modYamlFile);
                    ModData modData = new ModData(folder.Name);
                    modData.Manifest = modManifest;
                    mods[folder.Name] = modData;
                }
            }
        }

        public ModData LoadSpecificMod(string modID)
        {
            return mods[modID];
        }
    }
}