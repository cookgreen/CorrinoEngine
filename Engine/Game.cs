using CorrinoEngine.Assets;
using CorrinoEngine.Core;
using CorrinoEngine.FileSystem;
using CorrinoEngine.Mods;
using CorrinoEngine.Renderer;
using CorrinoEngine.UI;
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
        private HudRenderer hudRenderer;
        private Vector2 viewportSize;
        private MouseState mouseState;
        private KeyboardState keyboardState;
        private float lastScrollY;
        private bool wasLeftMouseDown;
        private bool wasRightMouseDown;

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
            viewportSize = size;
            mouseState = ms;
            keyboardState = ks;
            lastScrollY = ms.Scroll.Y;
            wasLeftMouseDown = ms.IsButtonDown(MouseButton.Button1);
            wasRightMouseDown = ms.IsButtonDown(MouseButton.Button2);

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
            UIManager.Instance.BindWorld(world);
            hudRenderer = new HudRenderer(size);
            UIManager.Instance.RegisterScreen(new WorldHudScreen(world));
            UIManager.Instance.RegisterScreen(new MainMenuScreen(world));
            world.Start();
        }

        private void loadAssest(ModData currentMod)
        {
            if (currentMod != null)
            {
                var fileSystem = new VirtualFileSystem();

                foreach (var asset in currentMod.Manifest.Asset.Assets)
                {
                    string assetFullPath = Path.Combine(currentMod.FullPath, asset);
                    if (!Directory.Exists(assetFullPath))
                    {
                        continue;
                    }

                    var folderFileSystem = new FolderFileSystem(assetFullPath);
                    fileSystem.Add(folderFileSystem);

                    foreach (var file in folderFileSystem.GetAllFiles())
                    {
                        if (file.EndsWith(".RFH", StringComparison.OrdinalIgnoreCase))
                        {
                            string dataFile = file.Substring(0, file.Length - 1) + "D";
                            Stream header = folderFileSystem.Read(file);
                            Stream data = folderFileSystem.Read(dataFile);
                            if (header != null && data != null)
                            {
                                fileSystem.Add(new RfhFileSystem(new Rfh(header, data)));
                            }
                        }
                        else if (file.EndsWith(".BAG", StringComparison.OrdinalIgnoreCase))
                        {
                            Stream bagStream = folderFileSystem.Read(file);
                            if (bagStream != null)
                            {
                                fileSystem.Add(new BagFileSystem(new Bag(bagStream)));
                            }
                        }
                    }
                }

                assetManager = new AssetManager(fileSystem);
            }
        }

        public void RenderFrame()
        {
            world.RenderFrame();
            hudRenderer?.Render(world, UIManager.Instance);
        }

        public void Update(FrameEventArgs args)
        {
            world.UpdateInput(mouseState, keyboardState);

            UiInputState input = CreateUiInputState();
            UIManager.Instance.Layout(new System.Drawing.RectangleF(0, 0, viewportSize.X, viewportSize.Y));
            bool uiVisible = UIManager.Instance.Update(input);
            bool blockWorldInput = uiVisible && UIManager.Instance.IsBlockingWorldInput();
            world.SetWorldInputBlocked(blockWorldInput);
            if (blockWorldInput && input.LeftPressed)
            {
                world.SuppressNextLeftClick();
            }

            world.Update(args);
        }

        public void UpdateInput(MouseState ms, KeyboardState ks)
        {
            mouseState = ms;
            keyboardState = ks;
        }

        public void Resize(Vector2 size)
        {
            viewportSize = size;
            world?.Resize(size);

            if (hudRenderer != null)
            {
                hudRenderer.Resize(size);
            }
        }

        private UiInputState CreateUiInputState()
        {
            bool leftDown = mouseState.IsButtonDown(MouseButton.Button1);
            bool rightDown = mouseState.IsButtonDown(MouseButton.Button2);
            float scrollDelta = mouseState.Scroll.Y - lastScrollY;
            UiInputState input = new UiInputState
            {
                MousePosition = new Vector2(mouseState.X, mouseState.Y),
                ScrollValue = mouseState.Scroll.Y,
                ScrollDelta = scrollDelta,
                LeftDown = leftDown,
                LeftPressed = leftDown && !wasLeftMouseDown,
                LeftReleased = !leftDown && wasLeftMouseDown,
                RightDown = rightDown,
                RightPressed = rightDown && !wasRightMouseDown,
                RightReleased = !rightDown && wasRightMouseDown,
                KeyboardState = keyboardState
            };

            lastScrollY = mouseState.Scroll.Y;
            wasLeftMouseDown = leftDown;
            wasRightMouseDown = rightDown;
            return input;
        }
    }
}
