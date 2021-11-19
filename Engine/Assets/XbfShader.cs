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

			public XbfShaderParameters(XbfShader shader)
				: base(shader)
			{
			}
		}

		// language=GLSL
		private const string VertexShader = @"
			#version 300 es
			precision highp float;

			uniform mat4 uModel;
			uniform mat4 uView;
			uniform mat4 uProjection;
			uniform mat4 uNormal;

			in vec3 aPosition;
			in vec3 aNormal;
			in vec2 aUv;

			out vec3 vNormal;
			out vec2 vUv;

			void main()
			{
				vNormal = normalize((uNormal * vec4(aNormal, 1.0)).xyz);
				vUv = aUv;
				gl_Position = uProjection * uView * uModel * vec4(aPosition, 1.0);
			}
		";

		// language=GLSL
		private const string FragmentShader = @"
			#version 300 es
			precision highp float;

			uniform vec3 uLight;
			uniform sampler2D uTexture;

			in vec3 vNormal;
			in vec2 vUv;

			out vec4 fColor;
			
			// TODO make it a float to multiply below
			vec4 ambientLight = vec4(0.5, 0.5, 0.5, 1.0);

			void main()
			{
				vec4 diffuseColor = texture(uTexture, vUv);
				vec4 lightColor = vec4(vec3(1.0, 1.0, 1.0) * max(dot(normalize(vNormal), normalize(uLight)), 0.0), 1.0);
				
				if (diffuseColor.w == 0.0)
					discard;
				else
					fColor = diffuseColor * (lightColor + ambientLight);
			}
		";

		private readonly int normal;
		private readonly int light;
		private readonly int vertexPosition;
		private readonly int vertexNormal;
		private readonly int vertexUv;

		public XbfShader(AssetManager assetManager, IReadableFileSystem fileSystem, string path)
			: base(XbfShader.VertexShader, XbfShader.FragmentShader)
		{
			this.normal = GL.GetUniformLocation(this.Program, "uNormal");
			this.light = GL.GetUniformLocation(this.Program, "uLight");
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
			GL.Uniform3(this.light, Vector3.TransformVector(-Vector3.UnitZ, view));
			GL.BindTexture(TextureTarget.Texture2D, parameters.Texture);
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
