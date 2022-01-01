using CorrinoEngine.Game;
using CorrinoEngine.Scenes.Customs;
using OpenTK.Windowing.Desktop;
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

        public SceneManager(World world)
        {
            this.world = world;
            scenes = new Stack<GameScene>();
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
    }
}
