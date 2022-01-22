using CorrinoEngine.Assets;
using CorrinoEngine.Core;
using CorrinoEngine.FileSystem;
using CorrinoEngine.Mods;
using LibEmperor;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine
{
    public enum GameState
    {
        stopped,
        running,
    }

    public class Game
    {
        private GameState gameState;
        private Argument argument;
        private ModData currentMod;
        private World world;
        private AssetManager assetManager;

        public GameState State
        {
            get { return gameState; }
        }

        public Game(Argument argument)
        {
            this.argument = argument;
        }

        public void StartNewGame(Vector2 size, MouseState ms, KeyboardState ks)
        {
            if(gameState == GameState.running)
            {
                return;
            }

            gameState = GameState.running;

            GL.ClearColor(0, 0, 0, 1);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            currentMod = null;
            if (argument.Length > 0 && argument.Contains("Mod"))
            {
                currentMod = ModManager.Instance.LoadSpecificMod(argument.GetArgumentParameter("Mod"));
            }
            else if (ModManager.Instance.Mods.Count > 0)
            {
                currentMod = ModManager.Instance.Mods.ElementAt(0).Value;
            }

            loadAssest(currentMod);

            world = new World(assetManager, currentMod, size, ms, ks);
            world.Start();
        }

        private void loadAssest(ModData currentMod)
        {
            if (currentMod != null)
            {
                var fileSystem = new VirtualFileSystem();

                foreach (var asset in currentMod.Manifest.Asset.Assets)
                {
                    string assetFullPath = Path.Combine(Environment.CurrentDirectory, "Mods/" + currentMod.ID, asset);
                    fileSystem.Add(new FolderFileSystem(assetFullPath));

                    foreach (var file in fileSystem.GetFiles())
                    {
                        if (file.EndsWith(".RFH", StringComparison.OrdinalIgnoreCase))
                        {
                            fileSystem.Add(new RfhFileSystem(new Rfh(fileSystem.Read(file)!, fileSystem.Read(file.Substring(0, file.Length - 1) + "D")!)));
                        }
                        else if (file.EndsWith(".BAG", StringComparison.OrdinalIgnoreCase))
                        {
                            fileSystem.Add(new BagFileSystem(new Bag(fileSystem.Read(file)!)));
                        }
                    }
                }

                assetManager = new AssetManager(fileSystem);
            }
        }

        public void RenderFrame()
        {
            world.RenderFrame();
        }

        public void Update(FrameEventArgs args)
        {
            world.Update(args);
        }
    }
}
