﻿using CorrinoEngine.Scenes.Customs;
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

        public void StartNewScene(string sceneName, GameWindow wnd)
        {
            GameScene newScene = null;
            switch(sceneName)
            {
                case "MainMenu":
                    newScene = new MainMenuScene(wnd);
                    break;
                case "InnerGame":
                    newScene = new InnerGameScene(wnd);
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
