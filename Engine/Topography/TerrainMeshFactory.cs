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
        public static Mesh CreateTileMesh(float tileSize, Vector3 color)
        {
            float half = tileSize * 0.5f;
            VertexPositionNormalUv[] vertices = new VertexPositionNormalUv[]
            {
                new VertexPositionNormalUv(new Vector3(-half, 0, -half), Vector3.UnitY, new Vector2(0, 0)),
                new VertexPositionNormalUv(new Vector3(half, 0, -half), Vector3.UnitY, new Vector2(1, 0)),
                new VertexPositionNormalUv(new Vector3(half, 0, half), Vector3.UnitY, new Vector2(1, 1)),
                new VertexPositionNormalUv(new Vector3(-half, 0, half), Vector3.UnitY, new Vector2(0, 1))
            };

            int[] indices = new int[] { 0, 1, 2, 0, 2, 3 };

            return new Mesh(
                new TerrainShader.TerrainShaderParameters(new TerrainShader()) { Color = color },
                vertices.SelectMany(o => o.Pack()).ToArray(),
                indices);
        }
    }

    public class TerrainShader : Shader<TerrainShader.TerrainShaderParameters>
    {
        public class TerrainShaderParameters : ShaderParameters<TerrainShader>
        {
            public Vector3 Color { get; set; }

            public TerrainShaderParameters(TerrainShader shader)
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

        public TerrainShader()
            : base(VertexShader, FragmentShader)
        {
            color = GL.GetUniformLocation(Program, "uColor");
            light = GL.GetUniformLocation(Program, "uLight");
            vertexPosition = GL.GetAttribLocation(Program, "aPosition");
            vertexNormal = GL.GetAttribLocation(Program, "aNormal");
            vertexUv = GL.GetAttribLocation(Program, "aUv");
        }

        protected override void Bind(Matrix4 model, Matrix4 view, Matrix4 projection, TerrainShaderParameters parameters)
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
}
