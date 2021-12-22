using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine.Game
{
    public class FactionInfo
    {
        private string id;
        private string name;
        private string startActor;

        public string ID { get { return id; } }
        public string Name { get { return name; } }
        public string StartActor { get { return startActor; } }

        public FactionInfo(string id, string name, string startActor)
        {
            this.id = id;
            this.name = name;
            this.startActor = startActor;
        }
    }
}
