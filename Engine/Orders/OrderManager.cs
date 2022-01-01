using CorrinoEngine.Cameras;
using CorrinoEngine.Game;
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
        private World world;
        private Stack<Order> historyOrders;
        private KeyboardState ks;
        private MouseState ms;
        private Camera cam;

        public event Action<string, object> OrderExecuted;

        public OrderManager(World world, Camera cam, KeyboardState ks, MouseState ms)
        {
            historyOrders = new Stack<Order>();
            this.world = world;
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
                newOrder.Execute("atreides-barrack");
                historyOrders.Push(newOrder);
            }
            if(ms.WasButtonDown(MouseButton.Button1))
            {
                MouseRay mouseRay = new MouseRay((int)ms.X, (int)ms.Y);
                var queryResult = mouseRay.RayCastQuery();
                if (queryResult.Count > 0)
                {
                    Order newOrder = new SelectUnitOrder(cam, ks, ms);
                    newOrder.OrderExecuted += NewOrder_OrderExecuted;
                    newOrder.Execute(queryResult);
                    historyOrders.Push(newOrder);
                }
            }
        }

        private void NewOrder_OrderExecuted(string arg1, object arg2)
        {
            OrderExecuted?.Invoke(arg1, arg2);
        }
    }
}
