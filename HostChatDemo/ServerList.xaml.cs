using HostChatDemo.Service;
using HostChatDemo.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace HostChatDemo
{
    /// <summary>
    /// ServerList.xaml 的交互逻辑
    /// </summary>
    public partial class ServerList : Window
    {
        static ServerListVM vm = new ServerListVM();

        public ServerList()
        {
            InitializeComponent();
            DataContext = vm;

            vm.OnCreatedRoom = OnCreatedRoom;
            vm.OnEnterRoom = OnEnterRoom;
        }

        private void OnCreatedRoom()
        {
            this.Close();
        }

        private void OnEnterRoom()
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            vm.OnLoadedCmd.Execute(null);
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            vm.OnUnloadedCmd.Execute(null);
        }
    }
}
