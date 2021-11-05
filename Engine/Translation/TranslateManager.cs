using CorrinoEngine.FileFormats;
using CorrinoEngine.Mods;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine.Translation
{
    public class TranslateManager
    {
        private List<TranslateLocate> translableStringDictory;

        private static TranslateManager instance;
        public static TranslateManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new TranslateManager();
                }
                return instance;
            }
        }

        public TranslateManager()
        {
            translableStringDictory = new List<TranslateLocate>();
        }

        public void Init(ModData modData)
        {
            //Load all translable texts
            var transRelativeDir = modData.Manifest.TranslationDirectory;
            var transFullPath = Path.Combine(modData.FullPath, transRelativeDir);

            DirectoryInfo di = new DirectoryInfo(transFullPath);
            foreach (var folder in di.EnumerateDirectories())
            {
                string transYamlFile = Path.Combine(folder.FullName, "locate.yaml");
                if (File.Exists(transYamlFile))
                {
                    MiniYaml yaml = new MiniYaml(transYamlFile);
                    if (yaml.Nodes[0].Value == folder.Name) //check
                    {
                        TranslateLocate translateLocate = new TranslateLocate(folder.Name);

                        var subNodes = yaml.Nodes[0].ChildNodes;
                        foreach(var subNode in subNodes)
                        {
                            translateLocate.AppendLocateString(subNode.Name, subNode.Value);
                        }

                        translableStringDictory.Add(translateLocate);
                    }
                }
            }
        }

        public string GetTranslableString(string translateLocateID, string id)
        {
            var result = translableStringDictory.Where(o => o.ID == translateLocateID);
            if (result.Count() == 1)
            {
                var pair = result.FirstOrDefault();
                if (pair.ContainsKey(id))
                {
                    return pair[id];
                }
            }
            return null;
        }
    }
}
