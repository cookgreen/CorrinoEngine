using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine.FileFormats
{
	public class MiniYamlNode
	{
		public string Name { get; set; }
		public string Value { get; set; }
		public MiniYamlNode ParentNode { get; set; }
		public List<MiniYamlNode> ChildNodes { get; set; }

		public MiniYamlNode()
		{
			ChildNodes = new List<MiniYamlNode>();
		}
	}
}
