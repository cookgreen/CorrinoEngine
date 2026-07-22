using CorrinoEngine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine.UI
{
    public class UIManager
    {
        private static UIManager instance;
        private World world;
        private bool isBuildQueueVisible;

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
            get { return isBuildQueueVisible; }
        }

        public void CreateButton(string name)
        {
        } 

        public void BindWorld(World world)
        {
            this.world = world;
        }

        public void StartUI(string internalUIName)
        {
            if (internalUIName == "MainMenuUI")
            {
                return;
            }

            if (internalUIName == "BuildQueueUI" && world != null)
            {
                isBuildQueueVisible = true;
            }
        }

        public void RefreshBuildQueueUI()
        {
            if (world != null && world.SelectedActor != null && world.SelectedActor.HasField("ProvideBuildings"))
            {
                isBuildQueueVisible = true;
            }
        }

        public void CloseBuildQueueUI()
        {
            isBuildQueueVisible = false;
        }
    }
}
