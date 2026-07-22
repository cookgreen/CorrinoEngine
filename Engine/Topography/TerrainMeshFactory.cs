using CorrinoEngine.Assets;
using CorrinoEngine.Graphics.Mesh;
using CorrinoEngine.Graphics.Shaders;
using CorrinoEngine.Graphics.Vertices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Linq;

namespace CorrinoEngine.Topography
{
    public static class TerrainMeshFactory
    {
        public static Mesh CreateColorTileMesh(float tileSize, Vector3 color)
        {
            BuildTileGeometry(tileSize, 1, out float[] vertices, out int[] indices);

            return new Mesh(
                new TerrainColorShader.TerrainColorShaderParameters(new TerrainColorShader()) { Color = color },
                vertices,
                indices);
        }

        public static Mesh CreateTextureTileMesh(AssetManager assetManager, object holder, float tileSize, string texturePath, float uvScale)
        {
            BuildTileGeometry(tileSize, uvScale, out float[] vertices, out int[] indices);

            return new Mesh(
                new TerrainTextureShader.TerrainTextureShaderParameters(new TerrainTextureShader())
                {
                    Texture = assetManager.Load<Texture>(holder, texturePath).Id
                },
                vertices,
                indices);
        }

        private static void BuildTileGeometry(float tileSize, float uvScale, out float[] vertices, out int[] indices)
        {
            float half = tileSize * 0.5f;
            float finalUvScale = uvScale <= 0 ? 1 : uvScale;
            VertexPositionNormalUv[] packedVertices = new VertexPositionNormalUv[]
            {
                new VertexPositionNormalUv(new Vector3(-half, 0, -half), Vector3.UnitY, new Vector2(0, 0)),
                new VertexPositionNormalUv(new Vector3(half, 0, -half), Vector3.UnitY, new Vector2(finalUvScale, 0)),
                new VertexPositionNormalUv(new Vector3(half, 0, half), Vector3.UnitY, new Vector2(finalUvScale, finalUvScale)),
                new VertexPositionNormalUv(new Vector3(-half, 0, half), Vector3.UnitY, new Vector2(0, finalUvScale))
            };

            vertices = packedVertices.SelectMany(o => o.Pack()).ToArray();
            indices = new int[] { 0, 1, 2, 0, 2, 3 };
        }
    }

    public class TerrainColorShader : Shader<TerrainColorShader.TerrainColorShaderParameters>
    {
        public class TerrainColorShaderParameters : ShaderParameters<TerrainColorShader>
        {
            public Vector3 Color { get; set; }

            public TerrainColorShaderParameters(TerrainColorShader shader)
                : base(shader)
            {
            }
        }

        private readonly int color;
        private readonly int light;
        private readonly int vertexPosition;
        private readonly int vertexNormal;
        private readonly int vertexUv;

        private const string VertexShader = @"
            #version 300 es
            precision highp float;

            uniform mat4 uModel;
            uniform mat4 uView;
            uniform mat4 uProjection;

            in vec3 aPosition;
            in vec3 aNormal;
            in vec2 aUv;

            out vec3 vNormal;

            void main()
            {
                vNormal = aNormal;
                gl_Position = uProjection * uView * uModel * vec4(aPosition, 1.0);
            }
        ";

        private const string FragmentShader = @"
            #version 300 es
            precision highp float;

            uniform vec3 uColor;
            uniform vec3 uLight;

            in vec3 vNormal;

            out vec4 fColor;

            void main()
            {
                float diffuse = max(dot(normalize(vNormal), normalize(uLight)), 0.0);
                vec3 finalColor = uColor * (0.45 + diffuse * 0.55);
                fColor = vec4(finalColor, 1.0);
            }
        ";

        public TerrainColorShader()
            : base(VertexShader, FragmentShader)
        {
            color = GL.GetUniformLocation(Program, "uColor");
            light = GL.GetUniformLocation(Program, "uLight");
            vertexPosition = GL.GetAttribLocation(Program, "aPosition");
            vertexNormal = GL.GetAttribLocation(Program, "aNormal");
            vertexUv = GL.GetAttribLocation(Program, "aUv");
        }

        protected override void Bind(Matrix4 model, Matrix4 view, Matrix4 projection, TerrainColorShaderParameters parameters)
        {
            base.Bind(model, view, projection, parameters);

            GL.Uniform3(color, parameters.Color);
            GL.Uniform3(light, Vector3.TransformVector(new Vector3(-0.25f, 1f, 0.35f), view));
        }

        public override int CreateVertexArrayObject()
        {
            int vertexArrayObject = base.CreateVertexArrayObject();

            GL.VertexAttribPointer(vertexPosition, 3, VertexAttribPointerType.Float, false, 8 * 4, 0);
            GL.EnableVertexAttribArray(vertexPosition);

            GL.VertexAttribPointer(vertexNormal, 3, VertexAttribPointerType.Float, false, 8 * 4, 3 * 4);
            GL.EnableVertexAttribArray(vertexNormal);

            GL.VertexAttribPointer(vertexUv, 2, VertexAttribPointerType.Float, false, 8 * 4, 6 * 4);
            GL.EnableVertexAttribArray(vertexUv);

            return vertexArrayObject;
        }
    }

    public class TerrainTextureShader : Shader<TerrainTextureShader.TerrainTextureShaderParameters>
    {
        public class TerrainTextureShaderParameters : ShaderParameters<TerrainTextureShader>
        {
            public int Texture { get; set; }

            public TerrainTextureShaderParameters(TerrainTextureShader shader)
                : base(shader)
            {
            }
        }

        private readonly int texture;
        private readonly int light;
        private readonly int vertexPosition;
        private readonly int vertexNormal;
        private readonly int vertexUv;

        private const string VertexShader = @"
            #version 300 es
            precision highp float;

            uniform mat4 uModel;
            uniform mat4 uView;
            uniform mat4 uProjection;

            in vec3 aPosition;
            in vec3 aNormal;
            in vec2 aUv;

            out vec3 vNormal;
            out vec2 vUv;

            void main()
            {
                vNormal = aNormal;
                vUv = aUv;
                gl_Position = uProjection * uView * uModel * vec4(aPosition, 1.0);
            }
        ";

        private const string FragmentShader = @"
            #version 300 es
            precision highp float;

            uniform sampler2D uTexture;
            uniform vec3 uLight;

            in vec3 vNormal;
            in vec2 vUv;

            out vec4 fColor;

            void main()
            {
                vec4 diffuseColor = texture(uTexture, vUv);
                float diffuse = max(dot(normalize(vNormal), normalize(uLight)), 0.0);
                vec3 finalColor = diffuseColor.rgb * (0.45 + diffuse * 0.55);
                fColor = vec4(finalColor, diffuseColor.a);
            }
        ";

        public TerrainTextureShader()
            : base(VertexShader, FragmentShader)
        {
            texture = GL.GetUniformLocation(Program, "uTexture");
            light = GL.GetUniformLocation(Program, "uLight");
            vertexPosition = GL.GetAttribLocation(Program, "aPosition");
            vertexNormal = GL.GetAttribLocation(Program, "aNormal");
            vertexUv = GL.GetAttribLocation(Program, "aUv");
        }

        protected override void Bind(Matrix4 model, Matrix4 view, Matrix4 projection, TerrainTextureShaderParameters parameters)
        {
            base.Bind(model, view, projection, parameters);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, parameters.Texture);
            GL.Uniform1(texture, 0);
            GL.Uniform3(light, Vector3.TransformVector(new Vector3(-0.25f, 1f, 0.35f), view));
        }

        public override int CreateVertexArrayObject()
        {
            int vertexArrayObject = base.CreateVertexArrayObject();

            GL.VertexAttribPointer(vertexPosition, 3, VertexAttribPointerType.Float, false, 8 * 4, 0);
            GL.EnableVertexAttribArray(vertexPosition);

            GL.VertexAttribPointer(vertexNormal, 3, VertexAttribPointerType.Float, false, 8 * 4, 3 * 4);
            GL.EnableVertexAttribArray(vertexNormal);

            GL.VertexAttribPointer(vertexUv, 2, VertexAttribPointerType.Float, false, 8 * 4, 6 * 4);
            GL.EnableVertexAttribArray(vertexUv);

            return vertexArrayObject;
        }
    }
}
