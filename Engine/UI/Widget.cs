using System.Collections.Generic;
using System.Drawing;
using OpenTK.Mathematics;

namespace CorrinoEngine.UI
{
    public abstract class Widget
    {
        private readonly List<Widget> children = new();

        public RectangleF Bounds { get; set; }
        public bool Visible { get; set; } = true;
        public bool Enabled { get; set; } = true;
        public bool ConsumeInput { get; set; } = false;
        public Widget Parent { get; private set; }
        public IReadOnlyList<Widget> Children => children;

        public virtual void AddChild(Widget widget)
        {
            if (widget != null)
            {
                widget.Parent = this;
                children.Add(widget);
            }
        }

        public virtual bool HitTest(Vector2 point)
        {
            return Visible && Bounds.Contains(point.X, point.Y);
        }

        public virtual void Layout(RectangleF availableBounds)
        {
            Bounds = availableBounds;

            foreach (Widget child in children)
                child.Layout(child.Bounds);
        }

        public virtual bool HandleInput(UiInputState input)
        {
            if (!Visible || !Enabled)
                return false;

            for (int i = children.Count - 1; i >= 0; i--)
            {
                if (children[i].HandleInput(input))
                    return true;
            }

            return ConsumeInput && HitTest(input.MousePosition);
        }

        public virtual void Render(UiRenderContext context)
        {
            if (!Visible)
                return;

            foreach (Widget child in children)
                child.Render(context);
        }
    }
}
