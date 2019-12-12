using HostChatDemo.Network.Client;
using HostChatDemo.Network.Server;
using HostChatDemo.Service;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace HostChatDemo
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnExit(ExitEventArgs e)
        {
            //关闭所有网络相关服务
            BroadcastClient.Stop();
            BroadcastServer.Stop();

            NetworkClient.Stop();
            NetworkServer.Stop();
        }
    }
}
