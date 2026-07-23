using CorrinoEngine.Assets;
using CorrinoEngine.Graphics.Mesh;
using CorrinoEngine.Graphics.Shaders;
using CorrinoEngine.Graphics.Vertices;
using CorrinoEngine.Maps;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
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

        public static Mesh CreateOriginalTerrainMesh(MapHeightField heightField, MapTerrainMaterialData materialData, Vector2 mapWorldSize, int gridResolution = 128)
        {
            int resolution = Math.Max(8, gridResolution);
            int vertexCount = (resolution + 1) * (resolution + 1);
            VertexPositionNormalUv[] packedVertices = new VertexPositionNormalUv[vertexCount];

            for (int y = 0; y <= resolution; y++)
            {
                for (int x = 0; x <= resolution; x++)
                {
                    int i = y * (resolution + 1) + x;
                    float u = x / (float)resolution;
                    float v = y / (float)resolution;
                    float worldX = u * mapWorldSize.X;
                    float worldZ = v * mapWorldSize.Y;
                    float worldY = heightField?.SampleHeight01(u, v) ?? 0f;

                    float hL = heightField?.SampleHeight01(Math.Max(0f, u - 1f / resolution), v) ?? worldY;
                    float hR = heightField?.SampleHeight01(Math.Min(1f, u + 1f / resolution), v) ?? worldY;
                    float hD = heightField?.SampleHeight01(u, Math.Max(0f, v - 1f / resolution)) ?? worldY;
                    float hU = heightField?.SampleHeight01(u, Math.Min(1f, v + 1f / resolution)) ?? worldY;
                    Vector3 normal = Vector3.Normalize(new Vector3(hL - hR, 2f, hD - hU));

                    packedVertices[i] = new VertexPositionNormalUv(
                        new Vector3(worldX, worldY, worldZ),
                        normal,
                        new Vector2(u * 16f, v * 16f));
                }
            }

            int[] indices = new int[resolution * resolution * 6];
            int cursor = 0;
            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    int i0 = y * (resolution + 1) + x;
                    int i1 = i0 + 1;
                    int i2 = i0 + resolution + 1;
                    int i3 = i2 + 1;
                    indices[cursor++] = i0;
                    indices[cursor++] = i1;
                    indices[cursor++] = i3;
                    indices[cursor++] = i0;
                    indices[cursor++] = i3;
                    indices[cursor++] = i2;
                }
            }

            return new Mesh(
                new OriginalTerrainShader.OriginalTerrainShaderParameters(new OriginalTerrainShader())
                {
                    BaseTexture = materialData?.BaseTexture ?? 0,
                    GroundColorTexture = materialData?.GroundColorTexture ?? 0,
                    GroundLightTexture = materialData?.GroundLightTexture ?? 0,
                    LightDirection = materialData?.LightDirection ?? new Vector3(-0.25f, 1f, 0.35f),
                    AmbientTint = materialData?.AmbientTint ?? Vector3.One,
                    UseGroundColor = materialData?.HasGroundColor == true,
                    UseGroundLight = materialData?.HasGroundLight == true
                },
                packedVertices.SelectMany(o => o.Pack()).ToArray(),
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
