using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine.Scenes.Customs
{
    public class InnerGameScene : GameScene
    {
        private GameApp gameApp;

        public InnerGameScene(GameWindow wnd): base(wnd)
        {
            gameApp = (GameApp)wnd;
        }

        public override void Start()
        {
            //GameMap => Terrain

            loadMap();

            createTerrain();
        }

        private void loadMap()
        {

        }

        private void createTerrain()
        {

        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}
