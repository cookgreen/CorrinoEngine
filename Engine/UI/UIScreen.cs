namespace CorrinoEngine.UI
{
    public abstract class UIScreen : UI
    {
        public string Name { get; }
        public bool IsVisible { get; set; } = true;
        public bool BlocksWorldInput { get; protected set; }

        protected UIScreen(string name)
        {
            Name = name;
        }

        public virtual void OnShown()
        {
        }

        public virtual void OnHidden()
        {
        }
    }
}
