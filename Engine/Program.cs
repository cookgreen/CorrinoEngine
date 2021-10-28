using CorrinoEngine.Forms;
using CorrinoEngine.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CorrinoEngine
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
			Argument argument = new Argument(args);

            ModManager.Instance.LoadMods();

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (argument.Contains("Mod") && 
                ModManager.Instance.Mods.ContainsKey(argument.GetArgumentParameter("Mod")))
            {
                GameApp app = new GameApp(argument);
                app.Run();
            }
            else
            {
                var modSelectorWin = new frmModSelector();
                if (modSelectorWin.ShowDialog() == DialogResult.OK)
                {
                    GameApp app = new GameApp(modSelectorWin.Argument);
                    app.Run();
                }
            }
        }
    }
}
