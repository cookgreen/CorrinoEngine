using CorrinoEngine.Cameras;
using CorrinoEngine.Core;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;

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
            if (world.IsInBuildingPlacementMode)
            {
                if (world.ConsumeLeftClick())
                {
                    world.TryConfirmPendingPlacement();
                    return;
                }

                if (world.ConsumeRightClick())
                {
                    world.CancelPendingPlacement();
                    return;
                }

                return;
            }

            if (ks.IsKeyDown(Keys.X) && world.ConsumeLeftClick())//X + Left Mouse = Place building
            {
                Order newOrder = new PlaceBuildingOrder(cam, ks, ms);
                newOrder.OrderExecuted += NewOrder_OrderExecuted;
                newOrder.Execute("atreides-barrack");
                historyOrders.Push(newOrder);
                return;
            }

            if (world.ConsumeLeftClick())
            {
                var actor = world.QueryActorAtCursor();
                Order newOrder = new SelectUnitOrder(cam, ks, ms);
                newOrder.OrderExecuted += NewOrder_OrderExecuted;
                newOrder.Execute(actor);
                historyOrders.Push(newOrder);
                return;
            }

            if (world.ConsumeRightClick())
            {
                Vector3 groundPosition = world.QueryGroundAtCursor();
                if (groundPosition != Vector3.Zero)
                {
                    Order newOrder = new MoveActorOrder(cam, ks, ms);
                    newOrder.OrderExecuted += NewOrder_OrderExecuted;
                    newOrder.Execute(groundPosition);
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
