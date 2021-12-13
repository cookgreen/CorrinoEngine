using CorrinoEngine.Cameras;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine.Orders
{
    public class Order
    {
        protected KeyboardState ks;
        protected MouseState ms;
        protected Camera cam;

        public virtual event Action<string, object> OrderExecuted;

        public Order(Camera cam, KeyboardState ks, MouseState ms)
        {
            this.cam = cam;
            this.ks = ks;
            this.ms = ms;
        }

        public virtual void Execute(object args)
        {

        }
    }
}
