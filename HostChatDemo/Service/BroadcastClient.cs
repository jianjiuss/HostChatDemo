using HostChatDemo.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HostChatDemo.Service
{
    public static class BroadcastClient
    {
        private static ConcurrentDictionary<EndPoint, ServerInfo> servers = new ConcurrentDictionary<EndPoint, ServerInfo>();
        private static int broadcastPort = 9051;
        private static bool isClose = true;

        private static Socket socket;

        public static event Action<List<ServerInfo>> ServersChanged;
        public static List<ServerInfo> Servers
        {
            get
            {
                return servers.Values.ToList();
            }
        }

        public static void Run(string[] args)
        {
            servers.Clear();
            isClose = false;

            Thread countThread = new Thread(new ThreadStart(CountServers));
            countThread.IsBackground = true;
            countThread.Start();
            
            socket = new Socket(AddressFamily.InterNetwork,
                SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint iep =
                new IPEndPoint(IPAddress.Any, broadcastPort);
            socket.Bind(iep);
            EndPoint ep = iep;

            while (!isClose)
            {
                byte[] data = new byte[1024];
                int recv = 0;
                try
                {
                    recv = socket.ReceiveFrom(data, ref ep);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    break;
                }
                string stringData = Encoding.ASCII.GetString(data, 0, recv);

                RefreshServerList(ep, stringData);

                Thread.Sleep(100);
            }
        }

        private static void RefreshServerList(EndPoint ep, string data)
        {
            string[] splitDatas = data.Split(',');
            string roomName = splitDatas[0];
            string tcpIp = splitDatas[1];
            string tcpPort = splitDatas[2];

            Console.WriteLine($"接收到服务器广播.房间名字:{roomName}.Tcp连接IP:{tcpIp}.Tcp连接Port:{tcpPort}.");

            ServerInfo serverInfo;
            if (servers.TryGetValue(ep, out serverInfo))
            {
                serverInfo.ExpirationTime = 2.5f;
            }
            else
            {
                var isAdd = servers.TryAdd(ep, new ServerInfo()
                {
                    ServerEndPoint = ep,
                    TcpEndPoint = new IPEndPoint(IPAddress.Parse(tcpIp), int.Parse(tcpPort)),
                    RoomName = roomName,
                    ExpirationTime = 2.5f
                });
                if(isAdd && ServersChanged != null)
                {
                    ServersChanged(Servers);
                }
            }
        }

        public static void Stop()
        {
            if(!isClose)
            {
                isClose = true;
                socket.Close();
                Console.WriteLine("广播客户端关闭");
            }
        }

        private static void CountServers()
        {
            while (!isClose)
            {
                List<EndPoint> expirationEndPoints = new List<EndPoint>();
                foreach (var item in servers.Values)
                {
                    if (item.ExpirationTime <= 0)
                    {
                        expirationEndPoints.Add(item.ServerEndPoint);
                        continue;
                    }

                    item.ExpirationTime -= 0.1f;
                }

                foreach (var item in expirationEndPoints)
                {
                    ServerInfo serverInfo;
                    servers.TryRemove(item, out serverInfo);
                }

                if (expirationEndPoints.Count != 0 && ServersChanged != null)
                {
                    ServersChanged(Servers);
                }


                Thread.Sleep(100);
            }
        }
    }
}
