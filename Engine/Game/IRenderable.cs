using CorrinoEngine.Cameras;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine.Game
{
    public interface IRenderable
    {
        void Draw(Camera camera);
        void Update(float time);
    }
}
