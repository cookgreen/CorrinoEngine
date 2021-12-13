using CorrinoEngine.Cameras;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine.Orders
{
    public class PlaceBuildingOrder : Order
    {
        public override event Action<string, object> OrderExecuted;

        public PlaceBuildingOrder(Camera cam, KeyboardState ks, MouseState ms) : base(cam, ks, ms)
        {
        }

        public override void Execute(object args)
        {
            //Get Mouse Click Position
            var pos = cam.ToScene(new OpenTK.Mathematics.Vector2()
            {
                X = ms.X,
                Y = ms.Y
            });

            //Place a model
            object[] newArgs = new object[]
            {
                pos,
                args//model name
            };

            OrderExecuted?.Invoke("PlaceBuilding", newArgs);
        }
    }
}
