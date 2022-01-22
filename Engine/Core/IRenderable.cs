using CorrinoEngine.Cameras;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine.Core
{
    public interface IRenderable
    {
        Actor Actor { get; }
        Vector3 Position { get; }
        float BoundingRadius { get; }

        void Draw(Camera camera);
        void Update(float time);
    }
}
