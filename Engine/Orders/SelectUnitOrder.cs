using CorrinoEngine.Cameras;
using CorrinoEngine.Core;
using CorrinoEngine.Renderer;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine.Orders
{
    public class SelectUnitOrder : Order
    {
        public override event Action<string, object> OrderExecuted;

        public SelectUnitOrder(Camera cam, KeyboardState ks, MouseState ms) : base(cam, ks, ms)
        {
        }

        public override void Execute(object args)
        {
            var queryResult = args as List<IRenderable>;
            if (queryResult.Count == 1)
            {
            }
        }
    }
}
