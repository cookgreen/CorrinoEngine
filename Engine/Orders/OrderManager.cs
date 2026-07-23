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

        public void UpdateInput(MouseState mouseState, KeyboardState keyboardState)
        {
            ms = mouseState;
            ks = keyboardState;
        }

        public void Update()
        {
            if (world.IsWorldInputBlocked())
            {
                if (!ms.IsButtonDown(MouseButton.Button1) && world.IsSelectionDragging)
                {
                    world.EndSelectionDrag();
                }

                return;
            }

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

            if (ms.IsButtonDown(MouseButton.Button1))
            {
                if (!world.IsSelectionDragging)
                {
                    world.BeginSelectionDrag(new Vector2(ms.X, ms.Y));
                }
                else
                {
                    world.UpdateSelectionDrag(new Vector2(ms.X, ms.Y));
                }
            }

            if (!ms.IsButtonDown(MouseButton.Button1) && world.IsSelectionDragging)
            {
                world.EndSelectionDrag();
                if (world.HasSelectionRectangle())
                {
                    world.SelectActorsInRectangle(world.SelectionRectangle);
                    return;
                }

                var actor = world.QueryActorAtCursor();
                Order newOrder = new SelectUnitOrder(cam, ks, ms);
                newOrder.OrderExecuted += NewOrder_OrderExecuted;
                newOrder.Execute(actor);
                historyOrders.Push(newOrder);
                return;
            }

            if (world.ConsumeRightClick())
            {
                Vector3 groundPosition = world.QueryCommandTargetAtCursor();
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
