namespace CorrinoEngine.UI
{
    /// <summary>
    /// Collection of widgets
    /// </summary>
    public abstract class UI
    {
        public Widget Root { get; protected set; }

        public virtual void Layout(System.Drawing.RectangleF viewport)
        {
            Root?.Layout(viewport);
        }

        public virtual void Update(UiInputState input)
        {
            Root?.HandleInput(input);
        }

        public virtual void Render(UiRenderContext context)
        {
            Root?.Render(context);
        }
    }
}
