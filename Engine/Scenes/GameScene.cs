using CorrinoEngine.Game;
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
        protected World world;

        public GameScene(World world)
        {
            this.world = world;
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
