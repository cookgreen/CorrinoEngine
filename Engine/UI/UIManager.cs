using CorrinoEngine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace CorrinoEngine.UI
{
    public class UIManager
    {
        private static UIManager instance;
        private World world;
        private readonly Dictionary<string, UIScreen> screens = new();
        private readonly List<UIScreen> activeScreens = new();
        private UiInputState lastInputState;

        public static UIManager Instance
        {
            get 
            {
                if (instance == null)
                {
                    instance = new UIManager();
                }
                return instance;
            }
        }
        public bool IsBuildQueueVisible
        {
            get { return IsScreenVisible("WorldHud"); }
        }
        public IEnumerable<UIScreen> ActiveScreens => activeScreens.Where(screen => screen.IsVisible);

        public void CreateButton(string name)
        {
        } 

        public void BindWorld(World world)
        {
            this.world = world;
        }

        public void RegisterScreen(UIScreen screen)
        {
            if (screen == null || string.IsNullOrWhiteSpace(screen.Name))
                return;

            screens[screen.Name] = screen;
        }

        public bool IsScreenVisible(string name)
        {
            return activeScreens.Any(screen => screen.Name == name && screen.IsVisible);
        }

        public void StartUI(string internalUIName)
        {
            if (!screens.ContainsKey(internalUIName))
            {
                return;
            }

            UIScreen screen = screens[internalUIName];
            screen.IsVisible = true;
            if (!activeScreens.Contains(screen))
            {
                activeScreens.Add(screen);
                screen.OnShown();
            }
        }

        public void RefreshBuildQueueUI()
        {
            if (world != null && world.SelectedActor != null && world.SelectedActor.HasField("ProvideBuildings"))
            {
                StartUI("WorldHud");
            }
        }

        public void CloseBuildQueueUI()
        {
            CloseScreen("WorldHud");
        }

        public void CloseScreen(string name)
        {
            UIScreen screen = activeScreens.FirstOrDefault(o => o.Name == name);
            if (screen != null)
            {
                screen.IsVisible = false;
                activeScreens.Remove(screen);
                screen.OnHidden();
            }
        }

        public bool Update(UiInputState input)
        {
            lastInputState = input;
            for (int i = activeScreens.Count - 1; i >= 0; i--)
            {
                UIScreen screen = activeScreens[i];
                if (screen.IsVisible)
                {
                    screen.Update(input);
                }
            }

            return activeScreens.Any(screen => screen.IsVisible);
        }

        public void Layout(RectangleF viewport)
        {
            foreach (UIScreen screen in activeScreens)
            {
                if (screen.IsVisible)
                {
                    screen.Layout(viewport);
                }
            }
        }

        public void Render(UiRenderContext context)
        {
            foreach (UIScreen screen in activeScreens)
            {
                if (screen.IsVisible)
                {
                    screen.Render(context);
                }
            }
        }

        public bool IsBlockingWorldInput()
        {
            return activeScreens.Any(screen => screen.IsVisible && screen.BlocksWorldInput);
        }
    }
}
