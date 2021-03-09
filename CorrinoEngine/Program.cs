namespace CorrinoEngine
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			Argument argument = new Argument(args);

			using var app = new Application(argument);
			app.Run();
		}
	}
}
