using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace WarriorJungleClimbing
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Process fadeCandyListener = new Process();
            fadeCandyListener.StartInfo.FileName = "..\\..\\..\\..\\..\\fadecandy-package-02\\bin\\fcserver.exe";
            fadeCandyListener.Start();
            Application.Run(new WallSelection());
        }
    }
}
