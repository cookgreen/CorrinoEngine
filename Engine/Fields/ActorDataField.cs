using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine.Fields
{
    public class ActorDataField
    {
        private string actorTypeName;
        private Dictionary<string, object> actorProperties;

        public string TypeName
        {
            get { return actorTypeName; }
        }
        public Dictionary<string, object> Properties
        {
            get { return actorProperties; }
        }

        public ActorDataField(string actorTypeName)
        {
            this.actorTypeName = actorTypeName;
            actorProperties = new Dictionary<string, object>();
        }

        public void AppendActorProperty(string propertyName, object propertyVal)
        {
            actorProperties[propertyName] = propertyVal;
        }
    }
}
