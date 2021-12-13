using CorrinoEngine.Cameras;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine.Orders
{
    public class OrderManager
    {
        private Stack<Order> orders;
        private KeyboardState ks;
        private MouseState ms;
        private Camera cam;

        public event Action<string, object> OrderExecuted;

        public OrderManager(Camera cam, KeyboardState ks, MouseState ms)
        {
            orders = new Stack<Order>();
            this.ks = ks;
            this.ms = ms;
            this.cam = cam;
        }

        public void Update()
        {
            if (ks.IsKeyDown(Keys.X) && ms.WasButtonDown(MouseButton.Button1))//X + Left Mouse = Place building
            {
                Order newOrder = new PlaceBuildingOrder(cam, ks, ms);
                newOrder.OrderExecuted += NewOrder_OrderExecuted;
                newOrder.Execute("Buildings/AK_IN_hungfigure2_H0.xbf");
                orders.Push(newOrder);
            }
        }

        private void NewOrder_OrderExecuted(string arg1, object arg2)
        {
            OrderExecuted?.Invoke(arg1, arg2);
        }
    }
}
