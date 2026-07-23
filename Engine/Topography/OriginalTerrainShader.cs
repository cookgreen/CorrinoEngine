using CorrinoEngine.Graphics.Shaders;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace CorrinoEngine.Topography
{
    public class OriginalTerrainShader : Shader<OriginalTerrainShader.OriginalTerrainShaderParameters>
    {
        public class OriginalTerrainShaderParameters : ShaderParameters<OriginalTerrainShader>
        {
            public int BaseTexture { get; set; }
            public int GroundColorTexture { get; set; }
            public int GroundLightTexture { get; set; }
            public Vector3 LightDirection { get; set; } = new Vector3(-0.25f, 1f, 0.35f);
            public Vector3 AmbientTint { get; set; } = Vector3.One;
            public bool UseGroundColor { get; set; }
            public bool UseGroundLight { get; set; }

            public OriginalTerrainShaderParameters(OriginalTerrainShader shader)
                : base(shader)
            {
            }
        }

        private readonly int baseTexture;
        private readonly int groundColorTexture;
        private readonly int groundLightTexture;
        private readonly int lightDirection;
        private readonly int ambientTint;
        private readonly int useGroundColor;
        private readonly int useGroundLight;
        private readonly int vertexPosition;
        private readonly int vertexNormal;
        private readonly int vertexUv;

        private const string VertexShader = @"
            #version 330 core
            uniform mat4 uModel;
            uniform mat4 uView;
            uniform mat4 uProjection;

            layout(location = 0) in vec3 aPosition;
            layout(location = 1) in vec3 aNormal;
            layout(location = 2) in vec2 aUv;

            out vec3 vNormal;
            out vec2 vUv;
            out vec3 vWorldPos;

            void main()
            {
                vec4 worldPos = uModel * vec4(aPosition, 1.0);
                vWorldPos = worldPos.xyz;
                vNormal = mat3(uModel) * aNormal;
                vUv = aUv;
                gl_Position = uProjection * uView * worldPos;
            }
        ";

        private const string FragmentShader = @"
            #version 330 core
            uniform sampler2D uBaseTexture;
            uniform sampler2D uGroundColor;
            uniform sampler2D uGroundLight;
            uniform vec3 uLightDirection;
            uniform vec3 uAmbientTint;
            uniform bool uUseGroundColor;
            uniform bool uUseGroundLight;

            in vec3 vNormal;
            in vec2 vUv;
            in vec3 vWorldPos;

            out vec4 fColor;

            void main()
            {
                vec4 baseColor = texture(uBaseTexture, vUv);
                vec3 normal = normalize(vNormal);
                float diffuse = max(dot(normal, normalize(uLightDirection)), 0.0);
                vec3 lighting = (uAmbientTint * 0.35) + vec3(0.65 + diffuse * 0.35);

                vec3 finalColor = baseColor.rgb;
                if (uUseGroundColor)
                {
                    vec2 worldUv = vec2(vWorldPos.x / 8192.0, vWorldPos.z / 8192.0);
                    vec3 tone = texture(uGroundColor, worldUv).rgb;
                    finalColor *= mix(vec3(1.0), tone, 0.45);
                }
                if (uUseGroundLight)
                {
                    vec2 lightUv = vec2(vWorldPos.x / 8192.0, vWorldPos.z / 8192.0);
                    float lightMask = texture(uGroundLight, lightUv).r;
                    finalColor *= mix(vec3(1.0), vec3(lightMask), 0.50);
                }

                fColor = vec4(finalColor * lighting, baseColor.a);
            }
        ";

        public OriginalTerrainShader()
            : base(VertexShader, FragmentShader)
        {
            baseTexture = GL.GetUniformLocation(Program, "uBaseTexture");
            groundColorTexture = GL.GetUniformLocation(Program, "uGroundColor");
            groundLightTexture = GL.GetUniformLocation(Program, "uGroundLight");
            lightDirection = GL.GetUniformLocation(Program, "uLightDirection");
            ambientTint = GL.GetUniformLocation(Program, "uAmbientTint");
            useGroundColor = GL.GetUniformLocation(Program, "uUseGroundColor");
            useGroundLight = GL.GetUniformLocation(Program, "uUseGroundLight");
            vertexPosition = GL.GetAttribLocation(Program, "aPosition");
            vertexNormal = GL.GetAttribLocation(Program, "aNormal");
            vertexUv = GL.GetAttribLocation(Program, "aUv");
        }

        protected override void Bind(Matrix4 model, Matrix4 view, Matrix4 projection, OriginalTerrainShaderParameters parameters)
        {
            base.Bind(model, view, projection, parameters);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, parameters.BaseTexture);
            GL.Uniform1(baseTexture, 0);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, parameters.GroundColorTexture);
            GL.Uniform1(groundColorTexture, 1);

            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, parameters.GroundLightTexture);
            GL.Uniform1(groundLightTexture, 2);

            GL.Uniform3(lightDirection, parameters.LightDirection);
            GL.Uniform3(ambientTint, parameters.AmbientTint);
            GL.Uniform1(useGroundColor, parameters.UseGroundColor ? 1 : 0);
            GL.Uniform1(useGroundLight, parameters.UseGroundLight ? 1 : 0);
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
