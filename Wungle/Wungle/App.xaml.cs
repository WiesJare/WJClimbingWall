using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Wungle
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Process fadeCandyListener = new Process();
            //var filepath = Path.Combine(Path.GetDirectoryName(Application.), otherexename);
            //Process.Start(filepath);
            fadeCandyListener.StartInfo.FileName = "C:\\Program Files\\FCServer\\fcserver.exe";
            fadeCandyListener.Start();

            MainWindow wnd = new MainWindow();      
            wnd.Show();
        }
    }
}
