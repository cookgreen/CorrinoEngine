using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine.Cameras
{
    public class CameraController
    {
        protected Camera camera;
        protected MouseState ms;
        protected KeyboardState ks;

        public CameraController(Camera camera)
        {
            this.camera = camera;
        }

        public void InjectMouseState(MouseState  ms)
        {
            this.ms = ms;
        }

        public void InjectKeyborardState(KeyboardState ks)
        {
            this.ks = ks;
        }

        public virtual void Update()
        {

        }
    }
}
