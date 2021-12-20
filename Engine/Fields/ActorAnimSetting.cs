using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine.Fields
{
    public class ActorAnimSetting
    {
        public string Name { get; set; }
        public List<AnimSetting> AnimSettings { get; set; }

        public ActorAnimSetting()
        {
            AnimSettings = new List<AnimSetting>();
        }
    }

    public class AnimSetting
    {
        public string AnimName { get; set; }
        public string Resource { get; set; }
        public AnimDataSetting AnimDataSetting { get; set; }

        public AnimSetting()
        {
            AnimDataSetting = new AnimDataSetting();
        }
    }

    public class AnimDataSetting
    {
        public int Start;
        public int Length;
    }
}
