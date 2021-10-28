using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine.PathFind
{
    public interface IPathFind
    {
        List<Waypoint> GetWaypoints(Waypoint startLocation, Waypoint endLocation);
    }
}
