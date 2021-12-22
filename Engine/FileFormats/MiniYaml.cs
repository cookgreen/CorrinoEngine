using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CorrinoEngine.FileFormats
{
	public class MiniYaml
	{
		private string miniYamlFilePath;
		public List<MiniYamlNode> Nodes { get; set; }

		public MiniYaml(string miniYamlFilePath)
		{
			this.miniYamlFilePath = miniYamlFilePath;
			Nodes = new List<MiniYamlNode>();
			Parse();
		}

		public void Parse()
		{
			MiniYamlNode currentNode = null;
			using (StreamReader reader = new StreamReader(miniYamlFilePath))
			{
				int lastCount = -1;
				while (reader.Peek() > -1)
				{
					string line = reader.ReadLine();
					if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
					{
						continue;
					}
					int count = line.StartsWithCharCount('\t');
					if (count == 0)
					{
						currentNode = new MiniYamlNode();
						var tokens = line.Split(':');
						currentNode.Name = tokens[0].Replace("\t", null);
						if (tokens.Length > 1)
						{
							currentNode.Value = tokens[1].Trim();
						}
						Nodes.Add(currentNode);
					}
					else
					{
						int cunt = line.StartsWithCharCount('\t');

						MiniYamlNode yamlNode = new MiniYamlNode();
						var tokens = line.Split(':');
						yamlNode.Name = tokens[0].Replace("\t", null);
						if (tokens.Length > 1)
						{
							yamlNode.Value = tokens[1].Trim();
						}
						if (cunt == lastCount)
						{
							yamlNode.ParentNode = currentNode.ParentNode;
							currentNode.ParentNode.ChildNodes.Add(yamlNode);
						}
						else if (cunt == lastCount + 1)
						{
							yamlNode.ParentNode = currentNode;
							currentNode.ChildNodes.Add(yamlNode);
						}
						else if (cunt == lastCount - 1)
						{
							yamlNode.ParentNode = currentNode.ParentNode.ParentNode;
							currentNode.ParentNode.ParentNode.ChildNodes.Add(yamlNode);
						}
						else if (cunt < lastCount)
						{
							if (cunt - 1 == 0)
							{
								var lastRootNode = Nodes.Last();
								yamlNode.ParentNode = lastRootNode;
								lastRootNode.ChildNodes.Add(yamlNode);
							}
                            else
                            {
								var parentNode = getParentNode(currentNode, lastCount - 1);
								yamlNode.ParentNode = parentNode;
								parentNode.ChildNodes.Add(yamlNode);
                            }
						}
						currentNode = yamlNode;
					}
					lastCount = count;
				}
			}
		}

        private MiniYamlNode getParentNode(MiniYamlNode currentNode, int upTimes)
        {
			return getSubParentNode(currentNode, upTimes);
		}

		private MiniYamlNode getSubParentNode(MiniYamlNode currentNode, int upTimes)
		{
			if (upTimes == 0)
			{
				return currentNode;
			}
			else
			{
				return getSubParentNode(currentNode.ParentNode, upTimes - 1);
			}
		}

		public void Save()
		{
			if(File.Exists(miniYamlFilePath))
			{
				File.Delete(miniYamlFilePath);
			}

			using (StreamWriter writer = new StreamWriter(miniYamlFilePath))
			{
				foreach (var node in Nodes)
				{
					writer.WriteLine(string.Format("{0}: {1}", node.Name, node.Value));
					foreach (var subNode in node.ChildNodes)
					{
						writer.WriteLine(string.Format("\t{0}: {1}", subNode.Name, subNode.Value));
						writeSubNode(writer, subNode, 1);
					}
				}
			}
		}
		private void writeSubNode(StreamWriter writer, MiniYamlNode node, int lastTabCount)
		{
			int count = lastTabCount + 1;
			foreach (var subNode in node.ChildNodes)
			{
				writer.WriteLine(generateStringByCount("\t", count) + string.Format("{0}: {1}", subNode.Name, subNode.Value));
				writeSubNode(writer, subNode, count + 1);
			}
		}

		private string generateStringByCount(string ch, int count)
		{
			string str = string.Empty;
			for (int i = 0; i < count; i++)
			{
				str += ch;
			}
			return str;
		}
	}
}
