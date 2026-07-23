using CorrinoEngine.Graphics.Mesh;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine.Topography
{
    public class TerrainTile
    {
        private MeshInstance meshInstance;
        private int x;
        private int y;
        private int z;
        private bool isBuildable;

        public MeshInstance MeshInstance
        {
            get { return meshInstance; }
        }

        public int X
        {
            get { return x; }
        }

        public int Y
        {
            get { return y; }
        }

        public int Z
        {
            get { return z; }
        }

        public bool IsBuildable
        {
            get { return isBuildable; }
        }

        public TerrainTile(MeshInstance meshInstance, int x, int y, int z, bool isBuildable = true)
        {
            this.meshInstance = meshInstance;
            this.x = x;
            this.y = y;
            this.z = z;
            this.isBuildable = isBuildable;
        }
    }
}
