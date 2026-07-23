using CorrinoEngine;
using CorrinoEngine.FileSystem;
using CorrinoEngine.Graphics.Shaders;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

namespace CorrinoEngine.Assets
{

	public class XbfShader : Shader<XbfShader.XbfShaderParameters>
	{
		public class XbfShaderParameters : ShaderParameters<XbfShader>
		{
			public int Texture;
            public int GroundColorTexture;
            public int GroundLightTexture;
            public Vector3 LightDirection = new Vector3(-0.25f, 1f, 0.35f);
            public Vector3 AmbientTint = Vector3.One;
            public bool UseGroundColor;
            public bool UseGroundLight;

			public XbfShaderParameters(XbfShader shader)
				: base(shader)
			{
			}
		}

		// language=GLSL
		private const string VertexShader = @"
			#version 330 core

			uniform mat4 uModel;
			uniform mat4 uView;
			uniform mat4 uProjection;
			uniform mat4 uNormal;

			layout(location = 0) in vec3 aPosition;
			layout(location = 1) in vec3 aNormal;
			layout(location = 2) in vec2 aUv;

			out vec3 vNormal;
			out vec2 vUv;
            out vec3 vWorldPos;

			void main()
			{
				vNormal = normalize((uNormal * vec4(aNormal, 1.0)).xyz);
				vUv = aUv;
                vec4 worldPos = uModel * vec4(aPosition, 1.0);
                vWorldPos = worldPos.xyz;
				gl_Position = uProjection * uView * worldPos;
			}
		";

		// language=GLSL
		private const string FragmentShader = @"
			#version 330 core

			uniform vec3 uLight;
			uniform sampler2D uTexture;
            uniform sampler2D uGroundColor;
            uniform sampler2D uGroundLight;
            uniform bool uUseGroundColor;
            uniform bool uUseGroundLight;
            uniform vec3 uAmbientTint;

			in vec3 vNormal;
			in vec2 vUv;
            in vec3 vWorldPos;

			out vec4 fColor;
			
			void main()
			{
				vec4 diffuseColor = texture(uTexture, vUv);
                vec3 finalColor = diffuseColor.rgb;
                if (uUseGroundColor)
                {
                    vec2 worldUv = vec2(vWorldPos.x / 8192.0, vWorldPos.z / 8192.0);
                    vec3 tone = texture(uGroundColor, worldUv).rgb;
                    finalColor *= mix(vec3(1.0), tone, 0.45);
                }
                if (uUseGroundLight)
                {
                    vec2 worldUv = vec2(vWorldPos.x / 8192.0, vWorldPos.z / 8192.0);
                    float relief = texture(uGroundLight, worldUv).r;
                    finalColor *= mix(vec3(1.0), vec3(relief), 0.5);
                }
				vec4 lightColor = vec4((uAmbientTint * 0.35) + vec3(0.65, 0.65, 0.65) * max(dot(normalize(vNormal), normalize(uLight)), 0.0), 1.0);
				
				if (diffuseColor.w == 0.0)
					discard;
				else
					fColor = vec4(finalColor, diffuseColor.a) * lightColor;
			}
		";

		private readonly int normal;
		private readonly int light;
        private readonly int groundColorTexture;
        private readonly int groundLightTexture;
        private readonly int useGroundColor;
        private readonly int useGroundLight;
        private readonly int ambientTint;
		private readonly int vertexPosition;
		private readonly int vertexNormal;
		private readonly int vertexUv;

		public XbfShader(AssetManager assetManager, IReadableFileSystem fileSystem, string path)
			: base(XbfShader.VertexShader, XbfShader.FragmentShader)
		{
			this.normal = GL.GetUniformLocation(this.Program, "uNormal");
			this.light = GL.GetUniformLocation(this.Program, "uLight");
            this.groundColorTexture = GL.GetUniformLocation(this.Program, "uGroundColor");
            this.groundLightTexture = GL.GetUniformLocation(this.Program, "uGroundLight");
            this.useGroundColor = GL.GetUniformLocation(this.Program, "uUseGroundColor");
            this.useGroundLight = GL.GetUniformLocation(this.Program, "uUseGroundLight");
            this.ambientTint = GL.GetUniformLocation(this.Program, "uAmbientTint");
			this.vertexPosition = GL.GetAttribLocation(this.Program, "aPosition");
			this.vertexNormal = GL.GetAttribLocation(this.Program, "aNormal");
			this.vertexUv = GL.GetAttribLocation(this.Program, "aUv");
		}

		protected override void Bind(Matrix4 model, Matrix4 view, Matrix4 projection, XbfShaderParameters parameters)
		{
			base.Bind(model, view, projection, parameters);

			Matrix4 normal;
			Matrix4 mat = view * model;
			try
			{
				normal = Matrix4.Transpose(Matrix4.Invert(mat));
			}
			catch (InvalidOperationException ex) 
			when (ex.Message == "Matrix is singular and cannot be inverted.")
			{
				normal = Matrix4.Transpose(mat);
			}

			GL.UniformMatrix4(this.normal, false, ref normal);
			GL.Uniform3(this.light, Vector3.TransformVector(parameters.LightDirection, view));
            GL.Uniform3(this.ambientTint, parameters.AmbientTint);
            GL.Uniform1(this.useGroundColor, parameters.UseGroundColor ? 1 : 0);
            GL.Uniform1(this.useGroundLight, parameters.UseGroundLight ? 1 : 0);

            GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2D, parameters.Texture);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, parameters.GroundColorTexture);
            GL.Uniform1(this.groundColorTexture, 1);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, parameters.GroundLightTexture);
            GL.Uniform1(this.groundLightTexture, 2);
		}

		public override int CreateVertexArrayObject()
		{
			var vertexArrayObject = base.CreateVertexArrayObject();

			GL.VertexAttribPointer(this.vertexPosition, 3, VertexAttribPointerType.Float, false, 8 * 4, 0 * 4);
			GL.EnableVertexAttribArray(this.vertexPosition);

			GL.VertexAttribPointer(this.vertexNormal, 3, VertexAttribPointerType.Float, false, 8 * 4, 3 * 4);
			GL.EnableVertexAttribArray(this.vertexNormal);

			GL.VertexAttribPointer(this.vertexUv, 2, VertexAttribPointerType.Float, false, 8 * 4, 6 * 4);
			GL.EnableVertexAttribArray(this.vertexUv);

			return vertexArrayObject;
		}
	}
}
