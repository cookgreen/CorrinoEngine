using CorrinoEngine.Scenes.Customs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine.Scenes
{
    public class SceneManager
    {
        private Stack<GameScene> scenes;

        private static SceneManager instance;
        public static SceneManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SceneManager();
                }
                return instance;
            }
        }

        public SceneManager()
        {
            scenes = new Stack<GameScene>();
        }

        public void StartNewScene(string sceneName)
        {
            GameScene newScene = null;
            switch(sceneName)
            {
                case "MainMenu":
                    newScene = new MainMenuScene();
                    break;
                case "InnerGame":
                    newScene = new InnerGameScene();
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
