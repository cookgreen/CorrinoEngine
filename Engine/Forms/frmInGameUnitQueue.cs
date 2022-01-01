using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CorrinoEngine.Forms
{
    public partial class frmInGameUnitQueue : Form
    {
        private static frmInGameUnitQueue instance;
        public static frmInGameUnitQueue Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new frmInGameUnitQueue();
                }
                return instance;
            }
        }

        public frmInGameUnitQueue()
        {
            InitializeComponent();
        }

        public void UpdateData()
        {

        }
    }
}
