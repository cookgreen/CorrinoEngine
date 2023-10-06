using CorrinoEngine.Core;
using CorrinoEngine.Scenes.Customs;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine.Scenes
{
    public class SceneManager
    {
        private World world;
        private Stack<GameScene> scenes;
        private MouseState ms;
        private KeyboardState ks;

		public SceneManager(World world, MouseState ms, KeyboardState ks)
        {
            this.world = world;
            scenes = new Stack<GameScene>();
            this.ms = ms;
            this.ks = ks;
        }

        public void StartNewScene(string sceneName)
        {
            GameScene newScene = null;
            switch(sceneName)
            {
                case "MainMenu":
                    newScene = new MainMenuScene(world);
                    break;
                case "InnerGame":
                    newScene = new InnerGameScene(world);
                    break;
            }

            if(scenes.Count>0)
            {
                scenes.Peek().Exit();
            }

            scenes.Push(newScene);

            newScene.Start();
        }

		public void Update()
		{
		}
	}
}
