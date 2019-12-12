using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HostChatDemo.Service
{
    public static class BroadcastServer
    {
        private static Socket sock;
        private static IPEndPoint iep;
        private static byte[] data;
        private static int broadcastPort = 9051;
        private static bool isClose;

        private static string ServerName;
        private static string TcpIpAddress;
        private static string TcpPort;

        public static void Run(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("启动参数不正确");
                return;
            }
            ServerName = args[0];
            TcpIpAddress = args[1];
            TcpPort = args[2];

            isClose = false;

            Console.WriteLine(args[0]);
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
            ProtocolType.Udp);
            //255.255.255.255
            iep =
                new IPEndPoint(IPAddress.Broadcast, broadcastPort);

            data = Encoding.ASCII.GetBytes($"{ServerName},{TcpIpAddress},{TcpPort}");

            sock.SetSocketOption(SocketOptionLevel.Socket,
                SocketOptionName.Broadcast, 1);

            Thread t = new Thread(BroadcastMessage);
            t.Start();
        }

        public static void Stop()
        {
            Console.WriteLine("广播服务端关闭");
            isClose = true;
        }

        private static void BroadcastMessage()
        {
            while (!isClose)
            {
                sock.SendTo(data, iep);
                Thread.Sleep(TimeSpan.FromSeconds(2));
            }
            sock.Close();
        }
    }
}
