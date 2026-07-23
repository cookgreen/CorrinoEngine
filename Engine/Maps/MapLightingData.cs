using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Drawing;

namespace CorrinoEngine.Maps
{
    public class MapLightingData
    {
        public Vector3 Direction { get; set; } = Vector3.Zero;
        public List<Color> Colors { get; } = new();
    }
}
