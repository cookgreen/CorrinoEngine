using CorrinoEngine.Fields;
using CorrinoEngine.FileFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine.Mods
{
    public class ModManifestMetaData
    {
        public string Name { get; set; }
        public string Author { get; set; }
    }

    public class ModManifest
    {
        private ModData modData;
        private List<ActorData> actorDatas;

        public ModManifestMetaData MetaData { get; set; }
        public ModManifestAsset Asset { get; set; }
        public List<string> UnitSettings { get; set; }
        public List<string> AnimSettings { get; set; }
        public string TranslationDirectory { get; set; }

        public List<ActorData> ActorDataList
        {
            get { return actorDatas; }
        }

        public ModManifest(ModData modData)
        {
            this.modData = modData;
            actorDatas = new List<ActorData>();
        }

        public void Load(string modYamlFile)
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

            parse();
        }

        private void parse()
        {
            List<ActorDataField> actorDataFields = new List<ActorDataField>();
            List<ActorAnimSetting> actorAnimSettings = new List<ActorAnimSetting>();

            foreach(string path in UnitSettings)
            {
                actorDataFields.AddRange(parseActorDataField(path));
            }

            foreach (string path in AnimSettings)
            {
                actorAnimSettings.AddRange(parseActorAnimSettings(path));
            }

            foreach(var actorDataField in actorDataFields)
            {
                var findResult = actorAnimSettings.Where(o => o.Name == actorDataField.TypeName);
                if (findResult.Count() == 1)
                {
                    ActorData actorData = new ActorData(actorDataField.TypeName, actorDataField, findResult.First());
                    actorDatas.Add(actorData);
                }
            }
        }

        private List<ActorDataField> parseActorDataField(string path)
        {
            List<ActorDataField> actorDataFields = new List<ActorDataField>();
            MiniYaml miniYaml = new MiniYaml(Path.Combine(modData.FullPath, path));
            foreach (var node in miniYaml.Nodes)
            {
                ActorDataField actorDataField = new ActorDataField(node.Name);
                foreach(var subNode in node.ChildNodes)
                {
                    if (subNode.ChildNodes.Count > 0)
                    {
                        actorDataField.AppendActorProperty(subNode.Name, parseActorSubDataField(subNode));
                    }
                    else
                    {
                        actorDataField.AppendActorProperty(subNode.Name, subNode.Value);
                    }
                }
                actorDataFields.Add(actorDataField);
            }
            return actorDataFields;
        }

        private string parseActorSubDataField(MiniYamlNode node)
        {
            string dataVal = null;
            foreach(var subNode in node.ChildNodes)
            {
                if (subNode.ChildNodes.Count > 0)
                {
                    dataVal += parseActorSubDataField(subNode);
                }
                else
                {
                    dataVal +="^" + subNode.Name + ":" + subNode.Value;
                }
            }
            return dataVal;
        }

        private List<ActorAnimSetting> parseActorAnimSettings(string path)
        {
            List<ActorAnimSetting> actorAnimSettings = new List<ActorAnimSetting>();
            MiniYaml miniYaml = new MiniYaml(Path.Combine(modData.FullPath, path));
            foreach(var node in miniYaml.Nodes)
            {
                ActorAnimSetting actorAnimSetting = new ActorAnimSetting();
                actorAnimSetting.Name = node.Name;
                foreach (var subNode in node.ChildNodes)
                {
                    AnimSetting animSetting = new AnimSetting();
                    animSetting.AnimName = subNode.Name;
                    animSetting.Resource = subNode.Value;
                    if (subNode.ChildNodes.Count > 0)
                    {
                        foreach(var iNode in subNode.ChildNodes)
                        {
                            if(iNode.Name == "Start")
                            {
                                animSetting.AnimDataSetting.Start = int.Parse(iNode.Value);
                            }
                            else if(iNode.Name == "Length")
                            {
                                animSetting.AnimDataSetting.Length = int.Parse(iNode.Value);
                            }
                        }
                    }
                    actorAnimSetting.AnimSettings.Add(animSetting);
                }
                actorAnimSettings.Add(actorAnimSetting);
            }
            return actorAnimSettings;
        }
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
