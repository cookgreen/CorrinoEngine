using CorrinoEngine.Forms;
using CorrinoEngine.Game;
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
        public InnerGameScene(World world): base(world)
        {
        }

        public override void Start()
        {
            frmFactionSelection factionSelectionWin = new frmFactionSelection(world.FactionInfos);
            if(factionSelectionWin.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var startUnit = world.CreateActor(factionSelectionWin.SelectedFactionInfo.StartActor);
                world.SpawnActor(startUnit);
            };
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}
