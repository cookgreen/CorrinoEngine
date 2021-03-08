namespace CorrinoEngine.Graphics.Shaders
{
	using OpenTK.Graphics.OpenGL4;
	using OpenTK.Mathematics;
	using System;

	public abstract class Shader<T> : IShader where T : class, IShaderParameters
	{
		protected readonly int Program;

		private readonly int model;
		private readonly int view;
		private readonly int projection;

		protected Shader(string vertexShaderCode, string fragmentShaderCode)
		{
			var vertexShader = Shader<T>.CompileShader(ShaderType.VertexShader, vertexShaderCode);
			var fragmentShader = Shader<T>.CompileShader(ShaderType.FragmentShader, fragmentShaderCode);
			this.Program = Shader<T>.CompileProgram(vertexShader, fragmentShader);

			GL.DeleteShader(vertexShader);
			GL.DeleteShader(fragmentShader);

			this.model = GL.GetUniformLocation(this.Program, "uModel");
			this.view = GL.GetUniformLocation(this.Program, "uView");
			this.projection = GL.GetUniformLocation(this.Program, "uProjection");
		}

		private static int CompileShader(ShaderType type, string code)
		{
			var shader = GL.CreateShader(type);
			GL.ShaderSource(shader, code.TrimStart());
			GL.CompileShader(shader);

			var error = GL.GetShaderInfoLog(shader);

			if (error != "")
				throw new Exception(error);

			return shader;
		}

		private static int CompileProgram(int vertexShader, int fragmentShader)
		{
			var program = GL.CreateProgram();
			GL.AttachShader(program, vertexShader);
			GL.AttachShader(program, fragmentShader);
			GL.LinkProgram(program);

			var error = GL.GetProgramInfoLog(program);

			if (error != "")
				throw new Exception(error);

			return program;
		}

		protected virtual void Bind(Matrix4 model, Matrix4 view, Matrix4 projection, T parameters)
		{
			GL.UseProgram(this.Program);

			GL.UniformMatrix4(this.model, false, ref model);
			GL.UniformMatrix4(this.view, false, ref view);
			GL.UniformMatrix4(this.projection, false, ref projection);
		}

		public void Bind(Matrix4 model, Matrix4 view, Matrix4 projection, IShaderParameters parameters)
		{
			this.Bind(model, view, projection, (parameters as T)!);
		}

		public virtual int CreateVertexArrayObject()
		{
			var vertexArrayObject = GL.GenVertexArray();
			GL.BindVertexArray(vertexArrayObject);

			return vertexArrayObject;
		}
	}
}
