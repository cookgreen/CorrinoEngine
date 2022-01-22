using CorrinoEngine.Core;
using CorrinoEngine.Renderer;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine
{
    public class MouseRay
    {
        private Vector3 _start;
        private Vector3 _end;

        public Vector3 Start { get { return _start; } }

        public Vector3 End { get { return _end; } }

        public MouseRay(Point mouse)
            : this(mouse.X, mouse.Y)
        {
        }

        public MouseRay(int x, int y)
        {
            int[] viewport = new int[4];
            Matrix4 modelMatrix, projMatrix;

            // get matrix and viewport:
            GL.GetFloat(GetPName.TransposeModelviewMatrix, out modelMatrix);
            GL.GetFloat(GetPName.TransposeProjectionMatrix, out projMatrix);
            GL.GetInteger(GetPName.Viewport, viewport);

            _start = new Vector3(0, 0, 0);
            _end = new Vector3(0, 0, 0);

            OpenTKHelper.UnProject(ref projMatrix, modelMatrix, new Size(viewport[0], viewport[1]), new Vector3(x, y, 0.0f));
            OpenTKHelper.UnProject(ref projMatrix, modelMatrix, new Size(viewport[0], viewport[1]), new Vector3(x, y, 1.0f));
        }

        internal List<IRenderable> RayCastQuery()
        {
            List<IRenderable> queryResult = new List<IRenderable>();
            foreach(var renderableObject in WorldRenderer.Instance.RenderableObjects)
            {
                if(HitSphere(renderableObject))
                {
                    queryResult.Add(renderableObject);
                }
            }
            return queryResult;
        }

        public bool HitSphere(IRenderable drawable)
        {
            return HitSphereAt(drawable) != Vector3.Zero;
        }

        public Vector3 HitSphereAt(IRenderable drawable)
        {
            Vector3 pos = drawable.Position;
            float radius = drawable.BoundingRadius;

            Vector3 d = _end - _start;
            float a = Vector3.Dot(d, d);
            float b = 2.0f * Vector3.Dot(d, _start - pos);
            float c = 2.0f * Vector3.Dot(pos, pos) + Vector3.Dot(_start, _start) - 2.0f * Vector3.Dot(pos, _start) - radius * radius;

            float test = b * b - 4.0f * a * c;

            if (test >= 0.0)
            {
                // Hit (according to Treebeard, "a fine hit").
                float u = (-b - (float)Math.Sqrt(test)) / (2.0f * a);
                return _start + u * (_end - _start);
            }

            return Vector3.Zero;
        }
    }
}
