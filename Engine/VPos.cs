using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine
{
    public class VPos
    {
        public float X;
        public float Y;
        public float Z;

        public VPos()
        {
            X = 0;
            Y = 0;
            Z = 0;
        }

        public Vector3 Vector3
        {
            get
            {
                Vector3 vector3 = new Vector3();
                vector3.X = X;
                vector3.Y = Y;
                vector3.Z = Z;
                return vector3;
            }
        }
    }
}
