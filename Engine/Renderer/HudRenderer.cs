using System;
using System.Drawing;
using System.Linq;
using CorrinoEngine.Core;
using CorrinoEngine.Fields;
using CorrinoEngine.UI;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace CorrinoEngine.Renderer
{
    public class HudRenderer : IDisposable
    {
        private Vector2 viewportSize;
        private TextRenderer textRenderer;
        private readonly Font titleFont;
        private readonly Font bodyFont;
        private readonly Font smallFont;
        private readonly Brush whiteBrush;
        private readonly Brush accentBrush;
        private readonly Brush dimBrush;
        private Brush warningBrush;
        private bool disposed;
        private readonly int shaderProgram;
        private readonly int vertexArrayObject;
        private readonly int vertexBufferObject;
        private readonly int indexBufferObject;
        private readonly int viewportUniform;
        private readonly int colorUniform;
        private readonly int useTextureUniform;
        private readonly int textureUniform;

        private const string VertexShaderSource = @"
            #version 330 core

            layout(location = 0) in vec2 aPosition;
            layout(location = 1) in vec2 aUv;

            uniform vec2 uViewport;

            out vec2 vUv;

            void main()
            {
                vec2 ndc = vec2(
                    (aPosition.x / uViewport.x) * 2.0 - 1.0,
                    1.0 - (aPosition.y / uViewport.y) * 2.0);
                gl_Position = vec4(ndc, 0.0, 1.0);
                vUv = aUv;
            }
        ";

        private const string FragmentShaderSource = @"
            #version 330 core

            in vec2 vUv;

            uniform vec4 uColor;
            uniform bool uUseTexture;
            uniform sampler2D uTexture;

            out vec4 fColor;

            void main()
            {
                vec4 baseColor = uUseTexture ? texture(uTexture, vUv) : vec4(1.0);
                fColor = baseColor * uColor;
            }
        ";

        public HudRenderer(Vector2 viewportSize)
        {
            this.viewportSize = viewportSize;
            textRenderer = new TextRenderer((int)viewportSize.X, (int)viewportSize.Y);
            titleFont = new Font("Segoe UI", 16, FontStyle.Bold);
            bodyFont = new Font("Segoe UI", 10, FontStyle.Regular);
            smallFont = new Font("Segoe UI", 9, FontStyle.Regular);
            whiteBrush = Brushes.White;
            accentBrush = new SolidBrush(Color.FromArgb(255, 220, 186, 109));
            dimBrush = new SolidBrush(Color.FromArgb(255, 180, 180, 180));
            warningBrush = new SolidBrush(Color.FromArgb(255, 225, 108, 108));

            shaderProgram = CreateShaderProgram(VertexShaderSource, FragmentShaderSource);
            viewportUniform = GL.GetUniformLocation(shaderProgram, "uViewport");
            colorUniform = GL.GetUniformLocation(shaderProgram, "uColor");
            useTextureUniform = GL.GetUniformLocation(shaderProgram, "uUseTexture");
            textureUniform = GL.GetUniformLocation(shaderProgram, "uTexture");

            vertexArrayObject = GL.GenVertexArray();
            vertexBufferObject = GL.GenBuffer();
            indexBufferObject = GL.GenBuffer();

            GL.BindVertexArray(vertexArrayObject);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, 16 * sizeof(float), IntPtr.Zero, BufferUsageHint.DynamicDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, 6 * sizeof(uint), new uint[] { 0, 1, 2, 0, 2, 3 }, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));

            GL.BindVertexArray(0);
        }

        public void Resize(Vector2 size)
        {
            viewportSize = size;
            textRenderer.Dispose();
            textRenderer = new TextRenderer((int)size.X, (int)size.Y);
        }

        public void Render(World world, UIManager uiManager)
        {
            GL.Viewport(0, 0, (int)viewportSize.X, (int)viewportSize.Y);
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.DepthMask(false);
            GL.UseProgram(shaderProgram);
            GL.Uniform2(viewportUniform, viewportSize);
            GL.BindVertexArray(vertexArrayObject);

            textRenderer.Clear(Color.Transparent);
            UiRenderContext context = new UiRenderContext
            {
                ViewportSize = viewportSize,
                TextRenderer = textRenderer,
                WhiteBrush = whiteBrush,
                AccentBrush = accentBrush,
                DimBrush = dimBrush,
                WarningBrush = warningBrush,
                TitleFont = titleFont,
                BodyFont = bodyFont,
                SmallFont = smallFont,
                DrawRect = DrawRect,
                DrawTexture = DrawTexture,
                DrawLine = DrawLine,
                DrawText = (text, font, brush, point) => textRenderer?.DrawString(text, font, brush, point),
                MeasureText = (text, font) => textRenderer?.MeasureString(text ?? string.Empty, font ?? bodyFont) ?? SizeF.Empty
            };
            uiManager?.Render(context);
            DrawTextOverlay();

            GL.BindVertexArray(0);
            GL.UseProgram(0);
            GL.DepthMask(true);
            GL.Enable(EnableCap.DepthTest);
        }

        private void DrawTextOverlay()
        {
            int texture = textRenderer.Texture;
            DrawQuad(0, 0, viewportSize.X, viewportSize.Y, Color.White, texture, true);
        }

        private void DrawRect(float x, float y, float width, float height, Color color)
        {
            DrawQuad(x, y, width, height, color, 0, false);
        }

        private void DrawTexture(float x, float y, float width, float height, int texture, Color color, float rotationDegrees)
        {
            DrawQuad(x, y, width, height, color, texture, true, rotationDegrees);
        }

        private void DrawLine(float x0, float y0, float x1, float y1, Color color, float thickness)
        {
            float dx = x1 - x0;
            float dy = y1 - y0;
            float length = MathF.Sqrt(dx * dx + dy * dy);
            if (length <= float.Epsilon)
            {
                return;
            }

            float angle = MathHelper.RadiansToDegrees(MathF.Atan2(dy, dx));
            DrawQuad(x0, y0 - thickness * 0.5f, length, thickness, color, 0, false, angle);
        }


        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            textRenderer.Dispose();
            titleFont.Dispose();
            bodyFont.Dispose();
            smallFont.Dispose();
            (accentBrush as IDisposable)?.Dispose();
            (dimBrush as IDisposable)?.Dispose();
            (warningBrush as IDisposable)?.Dispose();
            GL.DeleteBuffer(indexBufferObject);
            GL.DeleteBuffer(vertexBufferObject);
            GL.DeleteVertexArray(vertexArrayObject);
            GL.DeleteProgram(shaderProgram);
            disposed = true;
            GC.SuppressFinalize(this);
        }

        private void DrawQuad(float x, float y, float width, float height, Color color, int texture, bool useTexture, float rotationDegrees = 0f)
        {
            if (width <= 0 || height <= 0)
            {
                return;
            }

            float cx = x + width * 0.5f;
            float cy = y + height * 0.5f;
            float radians = MathHelper.DegreesToRadians(rotationDegrees);
            float cos = MathF.Cos(radians);
            float sin = MathF.Sin(radians);

            void Rotate(ref float px, ref float py)
            {
                float dx = px - cx;
                float dy = py - cy;
                float rx = dx * cos - dy * sin;
                float ry = dx * sin + dy * cos;
                px = cx + rx;
                py = cy + ry;
            }

            float x0 = x;
            float y0 = y;
            float x1 = x + width;
            float y1 = y;
            float x2 = x + width;
            float y2 = y + height;
            float x3 = x;
            float y3 = y + height;
            if (Math.Abs(rotationDegrees) > float.Epsilon)
            {
                Rotate(ref x0, ref y0);
                Rotate(ref x1, ref y1);
                Rotate(ref x2, ref y2);
                Rotate(ref x3, ref y3);
            }

            float[] vertices =
            {
                x0, y0, 0f, 0f,
                x1, y1, 1f, 0f,
                x2, y2, 1f, 1f,
                x3, y3, 0f, 1f
            };

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, vertices.Length * sizeof(float), vertices);

            GL.Uniform4(
                colorUniform,
                color.R / 255f,
                color.G / 255f,
                color.B / 255f,
                color.A / 255f);
            GL.Uniform1(useTextureUniform, useTexture ? 1 : 0);

            if (useTexture)
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, texture);
                GL.Uniform1(textureUniform, 0);
            }
            else
            {
                GL.BindTexture(TextureTarget.Texture2D, 0);
            }

            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
        }

        private static int CreateShaderProgram(string vertexShaderSource, string fragmentShaderSource)
        {
            int vertexShader = CompileShader(ShaderType.VertexShader, vertexShaderSource);
            int fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentShaderSource);
            int program = GL.CreateProgram();
            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragmentShader);
            GL.LinkProgram(program);
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int linkStatus);
            if (linkStatus == 0)
            {
                string error = GL.GetProgramInfoLog(program);
                GL.DeleteProgram(program);
                GL.DeleteShader(vertexShader);
                GL.DeleteShader(fragmentShader);
                throw new InvalidOperationException(error);
            }

            GL.DetachShader(program, vertexShader);
            GL.DetachShader(program, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
            return program;
        }

        private static int CompileShader(ShaderType shaderType, string shaderSource)
        {
            int shader = GL.CreateShader(shaderType);
            GL.ShaderSource(shader, shaderSource);
            GL.CompileShader(shader);
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int compileStatus);
            if (compileStatus == 0)
            {
                string error = GL.GetShaderInfoLog(shader);
                GL.DeleteShader(shader);
                throw new InvalidOperationException(error);
            }

            return shader;
        }
    }
}
