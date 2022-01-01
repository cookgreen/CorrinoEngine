using CorrinoEngine.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine.Fields
{
    public class ProvideBuildingsField : Field, ISelectable
    {
        private object paramArr;

        public override string Name
        {
            get { return "ProvideBuildings"; }
        }

        public override object Params
        {
            get { return paramArr; }
        }

        public ProvideBuildingsField()
        {
            paramArr = new List<string>();
        }

        public override void Execute(params object[] param)
        {
            frmInGameUnitQueue.Instance.Show();
        }
    }
}
