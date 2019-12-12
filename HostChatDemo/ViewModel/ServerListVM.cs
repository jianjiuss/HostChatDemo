using HostChatDemo.Model;
using HostChatDemo.Network.Client;
using HostChatDemo.Network.Server;
using HostChatDemo.Network.Server.DataModel;
using HostChatDemo.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace HostChatDemo.ViewModel
{
    public class ServerListVM : BaseVM
    {
        public Action OnCreatedRoom;
        public Action OnEnterRoom;

        private ObservableCollection<ServerInfo> servers;

        public ObservableCollection<ServerInfo> Servers
        {
            get { return servers; }
            set
            {
                servers = value;
                OnPropertyChanged(() => Servers);
            }
        }

        private string creatRoomName;

        public string CreateRoomName
        {
            get { return creatRoomName; }
            set
            {
                creatRoomName = value;
                OnPropertyChanged(() => CreateRoomName);
            }
        }

        private ICommand createRoomCmd;

        public ICommand CreateRoomCmd
        {
            get
            {
                return createRoomCmd = createRoomCmd ?? new RelayCommand(CreateRoomExecute, CreateRoomCanExecute);
            }
        }

        private void CreateRoomExecute(object obj)
        {
            if(string.IsNullOrEmpty(CreateRoomName))
            {
                MessageBox.Show("房间名字不能为空");
                return;
            }

            if(CreateRoomName.Contains(','))
            {
                MessageBox.Show("房间名字不能带逗号");
                return;
            }

            NetworkServer.Run();

            IPEndPoint iep = (IPEndPoint)NetworkServer.ServerSocket.LocalEndPoint;
            string tcpLocalIp = iep.Address.ToString();
            string tcpLocalPort = iep.Port.ToString();

            NetworkClient.Run(iep);

            BroadcastServer.Run(new string[] 
            {
                CreateRoomName,
                tcpLocalIp,
                tcpLocalPort
            });

            OnCreatedRoom?.Invoke();
            MainWindow.VM.Status = "Now you are room chairman...";
            GlobalValue.IsInRoom = true;
            GlobalValue.IsRoomMaster = true;

            NetworkClient.Send(MessageType.UserJoin, new UserJoin() {
                UserName = GlobalValue.UserName
            });
        }

        private bool CreateRoomCanExecute(object arg)
        {
            return !GlobalValue.IsInRoom;
        }

        private ICommand enterRoomCmd;

        public ICommand EnterRoomCmd
        {
            get
            {
                return enterRoomCmd = enterRoomCmd ?? new RelayCommand(EnterRoomExecute, EnterRoomCanExecute);
            }
        }

        private void EnterRoomExecute(object obj)
        {
            ServerInfo serverInfo = (ServerInfo)obj;

            NetworkClient.Run(serverInfo.TcpEndPoint);
            NetworkClient.Send(MessageType.UserJoin, new UserJoin() {
                UserName = GlobalValue.UserName
            });

            MainWindow.VM.Status = "Now you are in room";
            OnEnterRoom?.Invoke();
            GlobalValue.IsInRoom = true;
        }

        private bool EnterRoomCanExecute(object arg)
        {
            return !GlobalValue.IsInRoom;
        }

        protected override void OnLoadedExecute(object obj)
        {
            Servers.Clear();

            BroadcastClient.ServersChanged -= BroadcastClient_ServersChanged;
            BroadcastClient.ServersChanged += BroadcastClient_ServersChanged;

            Task.Factory.StartNew(() => {
                BroadcastClient.Run(null);
            });
        }

        private void BroadcastClient_ServersChanged(List<ServerInfo> serverInfos)
        {
            Servers = new ObservableCollection<ServerInfo>(serverInfos);
        }

        protected override void OnUnloadedExecute(object obj)
        {
            BroadcastClient.Stop();
        }


        public ServerListVM()
        {
            Servers = new ObservableCollection<ServerInfo>();
        }
    }
}
