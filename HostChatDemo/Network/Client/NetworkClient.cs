using HostChatDemo.Network.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HostChatDemo.Network.Client
{
    public static class NetworkClient
    {
        private static Socket clientSocket;
        private static Dictionary<MessageType, Action<byte[]>> receiveMessageHandler = new Dictionary<MessageType, Action<byte[]>>();

        private static bool isServerOnline;
        private static bool isReceiveHeartBeatCallBack;

        public static void Run(EndPoint endPoint)
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.Connect(endPoint);
            isServerOnline = true;

            //接收服务端消息线程
            ParameterizedThreadStart receiveMethod = 
                new ParameterizedThreadStart(Receive);
            Thread listener = new Thread(receiveMethod) { IsBackground = true };
            listener.Start(clientSocket);

            //定期发送心跳包线程
            ParameterizedThreadStart sendHeartBeatMethod =
                new ParameterizedThreadStart(SendHeartBeat);
            Thread heartbeater = new Thread(sendHeartBeatMethod);
            heartbeater.Start(clientSocket);
        }

        public static void Stop()
        {
            if(isServerOnline)
            {
                isServerOnline = false;
                clientSocket.Close();
            }
        }

        private static void Receive(object obj)
        {
            Socket socket = (Socket)obj;

            Console.WriteLine($"服务端已连接.IP:{((IPEndPoint)socket.RemoteEndPoint).Address.ToString()},Port:{((IPEndPoint)socket.RemoteEndPoint).Port.ToString()}.");

            while (isServerOnline)
            {
                byte[] data = new byte[4];

                try
                {
                    int receive = socket.Receive(data);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("获取消息头时，服务端已经断开连接");
                    ConnectionLost();
                    isServerOnline = false;
                    break;
                }

                //解析协议
                int messageLength = BitConverter.ToUInt16(data.Take(2).ToArray(), 0);
                int messageType = BitConverter.ToUInt16(data.Skip(2).Take(2).ToArray(), 0);

                MessageType type = (MessageType)messageType;

                if(type == MessageType.HeartBeat)
                {
                    ReceiveHeartBeatCallBack();
                    continue;
                }

                //解析消息体
                data = new byte[messageLength - 4];
                try
                {
                    int receive = socket.Receive(data);
                    if(receive != data.Length)
                    {
                        Console.WriteLine("消息体长度与消息头中标记的长度不一致，断开服务端连接");
                        ConnectionLost();
                        isServerOnline = false;
                        break;
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine("获取消息体时，服务端断开连接");
                    ConnectionLost();
                    isServerOnline = false;
                    break;
                }

                //调用注册了的响应消息处理方法
                Action<byte[]> handler;
                if(receiveMessageHandler.TryGetValue(type, out handler))
                {
                    handler(data);
                }
            }
        }


        private static void SendHeartBeat(object obj)
        {
            Socket socket = (Socket)obj;

            //每2秒发送一次心跳包,并判断服务端是否有回调心跳包消息，如果没有回调的话断开连接
            while(isServerOnline)
            {
                isReceiveHeartBeatCallBack = false;
                Send(MessageType.HeartBeat);
                Thread.Sleep(2000);
                if(!isReceiveHeartBeatCallBack && isServerOnline)
                {
                    isServerOnline = false;
                    socket.Close();
                    ConnectionLost();
                    Console.WriteLine("服务端下线");
                }
            }
        }

        private static void ConnectionLost()
        {
            if(isServerOnline)
            {
                ClientMessageHandler.Ins.ConnectionLost();
            }
        }

        private static void ReceiveHeartBeatCallBack()
        {
            isReceiveHeartBeatCallBack = true;
        }
        
        public static void Register(MessageType type, Action<byte[]> messageHandler)
        {
            receiveMessageHandler[type] = messageHandler;
        }


        public static void Send(MessageType type, object data = null)
        {
            try
            {
                byte[] b = NetworkUtils.Serialize(data);
                byte[] pb = NetworkUtils.Pack(type, b);
                clientSocket.Send(pb);
            }
            catch(Exception ex)
            {
                Console.WriteLine("客户端发送消息异常.ex:" + ex.Message);
            }
        }
    }
}
