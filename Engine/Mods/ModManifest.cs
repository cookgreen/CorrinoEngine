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
        public List<string> UnitSettings { get; set; }
        public List<string> AnimSettings { get; set; }
        public string TranslationDirectory { get; set; }

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
                    foreach (var subNode in node.ChildNodes)
                    {
                        Asset.Assets.Add(subNode.Name);
                    }
                }
                else if (node.Name == "Settings")
                {
                    UnitSettings = new List<string>();
                    foreach (var subNode in node.ChildNodes)
                    {
                        UnitSettings.Add(subNode.Name);
                    }
                }
                else if (node.Name == "Anims")
                {
                    AnimSettings = new List<string>();
                    foreach (var subNode in node.ChildNodes)
                    {
                        AnimSettings.Add(subNode.Name);
                    }
                }
                else if (node.Name == "Translation")
                {
                    TranslationDirectory = node.ChildNodes[0].Name;
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
