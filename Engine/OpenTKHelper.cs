using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine
{
    internal class OpenTKHelper
    {
		public static Vector3 UnProject(
			ref Matrix4 projection,
			Matrix4 view,
			System.Drawing.Size viewport,
			Vector3 mouse)
		{
			Vector4 vec;

			vec.X = 2.0f * mouse.X / (float)viewport.Width - 1;
			vec.Y = -(2.0f * mouse.Y / (float)viewport.Height - 1);
			vec.Z = mouse.Z;
			vec.W = 1.0f;

			Matrix4 viewInv = Matrix4.Invert(view);
			Matrix4 projInv = Matrix4.Invert(projection);

			Vector4.Transform(vec, matrixToQuaternion(projInv), out vec);
			Vector4.Transform(vec, matrixToQuaternion(viewInv), out vec);

			if (vec.W > float.Epsilon || vec.W < -float.Epsilon)
			{
				vec.X /= vec.W;
				vec.Y /= vec.W;
				vec.Z /= vec.W;
			}

			return new Vector3(vec.X, vec.Y, vec.Z);
		}

		private static Quaternion matrixToQuaternion(Matrix4 m)
        {
            Quaternion q = new Quaternion();
            q.W = MathF.Sqrt(MathF.Max(0, 1 + m[0, 0] + m[1, 1] + m[2, 2])) / 2;
            q.X = MathF.Sqrt(MathF.Max(0, 1 + m[0, 0] - m[1, 1] - m[2, 2])) / 2;
            q.Y = MathF.Sqrt(MathF.Max(0, 1 - m[0, 0] + m[1, 1] - m[2, 2])) / 2;
            q.Z = MathF.Sqrt(MathF.Max(0, 1 - m[0, 0] - m[1, 1] + m[2, 2])) / 2;
            q.X *= MathF.Sign(q.X * (m[2, 1] - m[1, 2]));
            q.Y *= MathF.Sign(q.Y * (m[0, 2] - m[2, 0]));
            q.Z *= MathF.Sign(q.Z * (m[1, 0] - m[0, 1]));
            return q;
        }
    }
}
