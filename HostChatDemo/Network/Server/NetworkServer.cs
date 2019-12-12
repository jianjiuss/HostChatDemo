using HostChatDemo.Network.Server.DataModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HostChatDemo.Network.Server
{
    public static class NetworkServer
    {
        private static ConcurrentDictionary<string,Socket> allClientSocket = new ConcurrentDictionary<string, Socket>();
        private static Dictionary<MessageType, Action<Socket, byte[]>> receiveHandler = new Dictionary<MessageType, Action<Socket, byte[]>>();

        public static Socket ServerSocket { get; private set; }

        private static bool isRunning;

        public static void Run()
        {
            isRunning = true; 

            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint iep = new IPEndPoint(IPAddress.Parse(NetworkUtils.GetLocalIPv4()), 0);
            ServerSocket.Bind(iep);
            ServerSocket.Listen(0);

            Thread thread = new Thread(Await) { IsBackground = true };
            thread.Start();

            ServerMessageHandler.Ins.Init();
        }

        public static void Stop()
        {
            if(isRunning)
            {
                isRunning = false;

                ServerSocket.Close();
                foreach(var socket in allClientSocket.Values)
                {
                    socket.Close();
                }
                allClientSocket.Clear();
            }
        }

        private static void Await()
        {
            Socket client = null;

            while (isRunning)
            {
                try
                {
                    //同步等待,程序会阻塞在这里
                    client = ServerSocket.Accept();

                    //获取客户端唯一键
                    string endPoint = client.RemoteEndPoint.ToString();
                    allClientSocket[endPoint] = client;

                    //创建特定类型的方法
                    ParameterizedThreadStart receiveMethod =
                       new ParameterizedThreadStart(Receive);  //Receive方法在后面实现
                    Thread listener = new Thread(receiveMethod) { IsBackground = true };
                    listener.Start(client); //开启一个线程监听该客户端发送的消息
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private static void Receive(object obj)
        {
            Socket clientSocket = (Socket)obj;

            Console.WriteLine($"客户端已连接.IP:{((IPEndPoint)clientSocket.RemoteEndPoint).Address.ToString()},Port:{((IPEndPoint)clientSocket.RemoteEndPoint).Port.ToString()}.");

            while (isRunning)
            {
                byte[] data = new byte[4];
                int receive;

                try
                {
                    receive = clientSocket.Receive(data);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("接收客户端数据出问题，断开客户端连接。");
                    ConnectionLost(clientSocket);
                    break;
                }

                if (receive < 4)
                {
                    Console.WriteLine("接收客户端数据长度小于4字节，断开客户端连接。");
                    ConnectionLost(clientSocket);
                    break;
                }

                //解析协议
                int messageLength = BitConverter.ToUInt16(data.Take(2).ToArray(),0);
                int messageType = BitConverter.ToUInt16(data.Skip(2).Take(2).ToArray(), 0);

                MessageType type = (MessageType)messageType;
                if(type == MessageType.HeartBeat)
                {
                    SendHeartBeat(MessageType.HeartBeat, clientSocket);
                    continue;
                }

                //解析消息体
                data = new byte[messageLength - 4];
                try
                {
                    receive = clientSocket.Receive(data);
                    if(receive != data.Length)
                    {
                        Console.WriteLine("消息体长度与消息头中标记的长度不一致，断开服务端连接");
                        ConnectionLost(clientSocket);
                        break;
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine("获取消息体时，客户端断开连接");
                    ConnectionLost(clientSocket);
                    break;
                }

                //调用注册了的响应消息处理方法
                Action<Socket, byte[]> handler;
                if(receiveHandler.TryGetValue(type, out handler))
                {
                    handler(clientSocket, data);
                }
            }
        }

        private static void ConnectionLost(Socket clientSocket)
        {
            try
            {
                if (allClientSocket.TryRemove(clientSocket.RemoteEndPoint.ToString(), out Socket socket))
                {
                    ServerMessageHandler.Ins.LoseUserConnection(socket);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void SendHeartBeat(MessageType type, Socket clientSocket)
        {
            clientSocket.Send(NetworkUtils.Pack(MessageType.HeartBeat));
        }

        public static void Register(MessageType type, Action<Socket, byte[]> messageHandler)
        {
            receiveHandler[type] = messageHandler;
        }
    }
}
