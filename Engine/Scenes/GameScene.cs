using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine.Scenes
{
    public class GameScene
    {
        protected GameWindow wnd;

        public GameScene(GameWindow wnd)
        {
            this.wnd = wnd;
        }

        public virtual void Start()
        {

        }

        public virtual void Exit()
        {

        }

        public virtual void EventHandler()
        {

        }
    }
}
