namespace CorrinoEngine.Graphics.Shaders
{
	public readonly struct ShaderIndex
	{
		private readonly int i1;
		private readonly int i2;
		private readonly int i3;

		public ShaderIndex(int i1, int i2, int i3)
		{
			this.i1 = i1;
			this.i2 = i2;
			this.i3 = i3;
		}

		public int[] Pack()
		{
			return new[] {this.i1, this.i2, this.i3};
		}
	}
}
