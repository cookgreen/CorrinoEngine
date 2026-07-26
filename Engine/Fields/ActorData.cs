using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine.Fields
{
    public class ActorData
    {
        private string typeName;
        private ActorPropertyData dataField;
        private ActorAnimSetting actorAnimSettings;

        public string TypeName { get { return typeName; } }
        public ActorPropertyData DataField { get { return dataField; } }
        public ActorAnimSetting ActorAnimSettings { get { return actorAnimSettings; } }

        public ActorData(string typeName, ActorPropertyData dataField, ActorAnimSetting animSettings)
        {
            this.typeName = typeName;
            this.dataField = dataField;
            this.actorAnimSettings = animSettings;
        }

        public AnimSetting this[string animName]
        {
            get
            {
                return actorAnimSettings.AnimSettings.Where(o => o.AnimName == animName).FirstOrDefault();
            }
        }
    }
}
