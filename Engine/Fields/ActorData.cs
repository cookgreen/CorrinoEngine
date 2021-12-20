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
        private ActorDataField dataField;
        private ActorAnimSetting animSettings;

        public string TypeName { get { return typeName; } }
        public ActorDataField DataField { get { return dataField; } }
        public ActorAnimSetting AnimSettings { get { return animSettings; } }

        public ActorData(string typeName, ActorDataField dataField, ActorAnimSetting animSettings)
        {
            this.typeName = typeName;
            this.dataField = dataField;
            this.animSettings = animSettings;
        }

        public AnimSetting this[string animName]
        {
            get
            {
                return animSettings.AnimSettings.Where(o => o.AnimName == animName).FirstOrDefault();
            }
        }
    }
}
