using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;

namespace CorrinoEngine.UI
{
    public class UIManager
    {
        private static UIManager instance;
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

        public void StartUI(string internalUIName)
        {

        }
    }
}
