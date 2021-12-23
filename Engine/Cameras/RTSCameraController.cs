using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace CorrinoEngine.Cameras
{
    public class RTSCameraController : CameraController
    {
        private const float CAM_MOVEMENT = 3;

        public RTSCameraController(Camera camera) : base(camera)
        {
        }

        public override void Update()
        {
            if (ks == null || ms == null)
                return;

            if(ks.IsKeyDown(Keys.W))
            {
                camera.Position.Z += CAM_MOVEMENT;
            }
            else if(ks.IsKeyDown(Keys.A))
            {
                camera.Position.X += CAM_MOVEMENT;
            }
            else if (ks.IsKeyDown(Keys.S))
            {
                camera.Position.Z -= CAM_MOVEMENT;
            }
            else if (ks.IsKeyDown(Keys.D))
            {
                camera.Position.X -= CAM_MOVEMENT;
            }
        }
    }
}
