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
using System.Windows.Shapes;

namespace WpfClient
{
    /// <summary>
    /// Interaction logic for SelectionWindow.xaml
    /// </summary>
    public partial class SelectionWindow : Window
    {
        public SelectionWindow()
        {
            InitializeComponent();
            nameInput.Visibility = Visibility.Hidden;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (UserName.Text == string.Empty || UserName.Text == "Enter your name")
            {
                nameInput.Visibility = Visibility.Visible;
                return;
            }

            MainWindow main = new MainWindow(UserName.Text);
            main.Show();
            this.Close();
        }
    }
}
