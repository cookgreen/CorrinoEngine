using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorrinoEngine.Core;
using CorrinoEngine.UI;
using OpenTK.Windowing.Desktop;

namespace CorrinoEngine.Scenes.Customs
{
    public class MainMenuScene : GameScene
    {
        public MainMenuScene(World world) : base(world)
        {

        }

        public override void Start()
        {
            buildMainMenuUI();
        }

        private void buildMainMenuUI()
        {
            UIManager.Instance.StartUI("MainMenuUI");
        }
    }
}
