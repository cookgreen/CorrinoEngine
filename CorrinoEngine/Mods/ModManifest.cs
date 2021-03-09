using CorrinoEngine.FileFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine.Mods
{
    public class ModManifest
    {
        public ModManifestMetaData MetaData { get; set; }

        public ModManifestAsset Asset { get; set; }

        public void Parse(string modYamlFile)
        {
            MiniYaml miniYaml = new MiniYaml(modYamlFile);
            foreach(var node in miniYaml.Nodes)
            {
                if (node.Name == "MetaData")
                {
                    MetaData = new ModManifestMetaData();
                    MetaData.Name = node.ChildNodes[0].Value;
                    MetaData.Author = node.ChildNodes[1].Value;
                }
                else if (node.Name == "Assets")
                {
                    Asset = new ModManifestAsset();
                    foreach(var subNode in node.ChildNodes)
                    {
                        Asset.Assets.Add(subNode.Name);
                    }
                }
            }
        }
    }

    public class ModManifestMetaData
    {
        public string Name { get; set; }
        public string Author { get; set; }
    }

    public class ModManifestAsset
    {
        public List<string> Assets { get; set; }

        public ModManifestAsset()
        {
            Assets = new List<string>();
        }
    }
}
