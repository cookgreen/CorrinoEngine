using CorrinoEngine.Cameras;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;

namespace CorrinoEngine.Orders
{
    public class MoveActorOrder : Order
    {
        public override event Action<string, object> OrderExecuted;

        public MoveActorOrder(Camera cam, KeyboardState ks, MouseState ms)
            : base(cam, ks, ms)
        {
        }

        public override void Execute(object args)
        {
            Vector3 position = (Vector3)args;
            OrderExecuted?.Invoke("MoveActor", new object[] { position });
        }
    }
}
