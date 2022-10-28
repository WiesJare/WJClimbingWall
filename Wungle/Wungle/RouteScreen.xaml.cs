using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Wungle
{
    /// <summary>
    /// Interaction logic for RouteScreen.xaml
    /// </summary>
    public partial class RouteScreen : Page
    {
        public RouteScreen()
        {
            InitializeComponent();
            Loaded += RouteScreen_Load;
        }

        private void RouteScreen_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < 15; i++)
            {
                for (int j = 0; j < 30; j++)
                {
                    Button button = new Button();
                    int size;
                    size = (1080 / 31);
                    button.Width = size;
                    button.Height = size;
                    int locx, locy;
                    locx = (i + 1) * (1080 / 31);
                    locy = (j + 1) * (1080 / 31);
                    button.Margin = new Thickness(locx, locy,0,0);
                    button.Padding = new Thickness(0);
                    //button.f = Style.
                    //button.FlatAppearance.BorderSize = 1;
                    button.MouseEnter += MouseOverButton;
                    button.Name = "btn_" + i + "_" + j;
                    button.IsEnabled = false;
                    customGrid.Children.Add(button);
                }
            }
            for (int i = 1; i <= 15; i++)//Labels on top
            {
                char line = (char)(64 + i);
                Label label = new Label();
                int size;
                size = (1080 / 31);
                label.Width = size;
                label.Height = size;
                int locx, locy;
                locx = i * (1080 / 31);
                locy = 0;
                label.Margin = new Thickness(locx, locy,0,0);
                label.Content = line.ToString();
                label.Foreground = Brushes.Black;
                //label. = ContentAlignment.MiddleCenter;
                //label.AutoSize = false;
                label.Name = "lbl_" + i + "_0";
                customGrid.Children.Add(label);
            }
            for (int i = 1; i <= 30; i++)//Labels on side
            {
                string line = i.ToString();
                Label label = new Label();
                int size;
                size = (1080 / 31);
                label.Width = size;
                label.Height = size;
                int locx, locy;
                locx = 0;
                locy = i * (1080 / 31);
                label.Margin = new Thickness(locx, locy, 0, 0);
                label.Content = line.ToString();
                label.Foreground = Brushes.Black;
                //label. = ContentAlignment.MiddleCenter;
                //label.AutoSize = false;
                label.Name = "lbl_" + i + "_0";
                customGrid.Children.Add(label);
            }
        }
        private void MouseOverButton(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            SolidColorBrush mySolidColorBrush = new SolidColorBrush(Colors.Pink);
            mySolidColorBrush.Opacity = 0;
            button.Background = mySolidColorBrush;
        }
    }
}
