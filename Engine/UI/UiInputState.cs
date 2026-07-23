using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace CorrinoEngine.UI
{
    public class UiInputState
    {
        public Vector2 MousePosition { get; set; }
        public float ScrollValue { get; set; }
        public float ScrollDelta { get; set; }
        public bool LeftPressed { get; set; }
        public bool LeftDown { get; set; }
        public bool LeftReleased { get; set; }
        public bool RightPressed { get; set; }
        public bool RightDown { get; set; }
        public bool RightReleased { get; set; }
        public KeyboardState KeyboardState { get; set; }
    }
}
