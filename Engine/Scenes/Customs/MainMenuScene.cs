using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorrinoEngine.UI;
using ImGuiNET;
using ImGuiNETWidget;
using ImGuiOpenTK;
using OpenTK.Windowing.Desktop;

namespace CorrinoEngine.Scenes.Customs
{
    public class MainMenuScene : GameScene
    {
        public MainMenuScene(GameWindow wnd) : base(wnd)
        {

        }

        public override void Start()
        {
            buildMainMenuUI();
        }

        private void buildMainMenuUI()
        {
            UIManager.Instance.CreateButton("Start New Game");
        }
    }
}
