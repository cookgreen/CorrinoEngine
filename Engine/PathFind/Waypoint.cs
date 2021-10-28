using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine.PathFind
{
    public class Waypoint
    {
        private List<VPos> posList;
        public List<VPos> PosList
        {
            get { return posList; }
        }

        public Waypoint()
        {
            posList = new List<VPos>();
        }
    }
}
