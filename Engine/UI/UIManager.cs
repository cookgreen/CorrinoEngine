using CorrinoEngine.Core;
using CorrinoEngine.Forms;
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
                frmInGameUnitQueue.Instance.BindWorld(world);
                frmInGameUnitQueue.Instance.UpdateData();
                if (!frmInGameUnitQueue.Instance.Visible)
                {
                    frmInGameUnitQueue.Instance.Show();
                }
                else
                {
                    frmInGameUnitQueue.Instance.BringToFront();
                }
            }
        }

        public void RefreshBuildQueueUI()
        {
            if (world == null)
            {
                return;
            }

            if (frmInGameUnitQueue.Instance.Visible)
            {
                frmInGameUnitQueue.Instance.BindWorld(world);
                frmInGameUnitQueue.Instance.UpdateData();
            }
        }

        public void CloseBuildQueueUI()
        {
            if (frmInGameUnitQueue.Instance.Visible)
            {
                frmInGameUnitQueue.Instance.Hide();
            }
        }
    }
}
