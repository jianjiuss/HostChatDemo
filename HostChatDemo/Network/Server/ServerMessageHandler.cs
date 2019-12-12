using HostChatDemo.Network.Server.DataModel;
using HostChatDemo.Network.Server.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HostChatDemo.Network.Server
{
    public class ServerMessageHandler
    {
        private static ServerMessageHandler ins = new ServerMessageHandler();

        public static ServerMessageHandler Ins
        {
            get
            {
                return ins;
            }
        }

        internal void Init()
        {
            chatRoom = new ChatRoom();
        }

        private ChatRoom chatRoom;

        public ServerMessageHandler()
        {
            NetworkServer.Register(MessageType.RoomClose, RoomCloseHandler);
            NetworkServer.Register(MessageType.SendMessage, SendMessageHandler);
            NetworkServer.Register(MessageType.UserExit, UserExitHandler);
            NetworkServer.Register(MessageType.UserJoin, UserJoinHandler);
        }

        private void UserJoinHandler(Socket socket, byte[] data)
        {
            UserJoin userJoin = NetworkUtils.Deserialize<UserJoin>(data);
            chatRoom.UserJoin(new ChatUser() {
                socket = socket,
                userName = userJoin.UserName
            });
        }

        private void UserExitHandler(Socket socket, byte[] data)
        {
            UserExit userExit = NetworkUtils.Deserialize<UserExit>(data);
            chatRoom.UserExit(new ChatUser()
            {
                socket = socket,
                userName = userExit.UserName
            });
        }

        private void SendMessageHandler(Socket socket, byte[] data)
        {
            SendMessage sendMessage = NetworkUtils.Deserialize<SendMessage>(data);

            var user =  chatRoom.GetChatUser(socket);

            if(user == null)
            {
                Console.WriteLine("服务器处理发送消息失败，当前房间中不存在该用户");
                return;
            }

            chatRoom.SendContent(new ChatRecord() {
                chatUser = user,
                content = sendMessage.Content,
                sendDateTime = sendMessage.SendDateTime
            });
        }

        private void RoomCloseHandler(Socket socket, byte[] data)
        {
            chatRoom.Close(socket);
        }

        public void LoseUserConnection(Socket socket)
        {
            chatRoom.LoseUserConnection(socket);
        }
    }
}
