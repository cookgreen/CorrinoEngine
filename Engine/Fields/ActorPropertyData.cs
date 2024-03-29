﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine.Fields
{
    public class ActorPropertyData
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

        public ActorPropertyData(string actorTypeName)
        {
            this.actorTypeName = actorTypeName;
            actorProperties = new Dictionary<string, object>();
        }

        public void AppendActorProperty(string propertyName, object propertyVal)
        {
            actorProperties[propertyName] = propertyVal;
        }

        public IEnumerable<KeyValuePair<string, object>> GetFields(string propertyName)
        {
            return actorProperties.Where(o => o.Key.Contains(propertyName));
        }
    }
}
