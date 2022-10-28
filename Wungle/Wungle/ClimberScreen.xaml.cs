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
    /// Interaction logic for ClimberScreen.xaml
    /// </summary>
    public partial class ClimberScreen : Page
    {
        public ClimberScreen()
        {
            InitializeComponent();
            Loaded += ClimberScreen_Load;
        }

        private void ClimberScreen_Load(object sender, EventArgs e)
        {
            SolidColorBrush mySolidColorBrush = new SolidColorBrush(Colors.Gray);
            mySolidColorBrush.Opacity = 0;
            lBClimbers.Background = mySolidColorBrush;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new HomeScreen());
        }

        private void BtnAddClimber_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new AddClimberScreen());
        }
    }
}
