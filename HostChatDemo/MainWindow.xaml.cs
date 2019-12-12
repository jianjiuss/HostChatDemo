using HostChatDemo.ViewModel;
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

namespace HostChatDemo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        static MainVM vm = new MainVM();

        public static MainVM VM
        {
            get
            {
                return vm;
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            DataContext = vm;

            vm.Status = "Not in room ....";
            vm.RenameClick = RenameClick;

            Application.Current.MainWindow = this;
        }

        private void OpenRoomListClick(object sender, RoutedEventArgs e)
        {
            new ServerList().Show();
        }

        private void RenameClick()
        {
            new LoginWindow().Show();
            this.Close();
        }

        private void ExitClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
