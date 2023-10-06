using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine.Core
{
	public class InputManager
	{
		private KeyboardState keyboard;
		private MouseState mouse;

		public event Action<Keys> KeyUp;
		public event Action<Keys> KeyDown;
		public event Action<Keys> KeyReleased;

		public event Action MouseDown;
		public event Action MouseUp;

		public void Update()
		{
		}
	}
}
