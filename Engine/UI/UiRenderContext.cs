using System.Drawing;
using OpenTK.Mathematics;
using CorrinoEngine.Renderer;

namespace CorrinoEngine.UI
{
    public class UiRenderContext
    {
        public Vector2 ViewportSize { get; set; }
        public TextRenderer TextRenderer { get; set; }
        public Brush WhiteBrush { get; set; }
        public Brush AccentBrush { get; set; }
        public Brush DimBrush { get; set; }
        public Brush WarningBrush { get; set; }
        public Font TitleFont { get; set; }
        public Font BodyFont { get; set; }
        public Font SmallFont { get; set; }
        public System.Action<float, float, float, float, Color> DrawRect { get; set; }
        public System.Action<float, float, float, float, int, Color, float> DrawTexture { get; set; }
        public System.Action<string, Font, Brush, PointF> DrawText { get; set; }
    }
}
