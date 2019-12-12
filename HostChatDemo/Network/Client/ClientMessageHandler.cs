using HostChatDemo.Network.Server;
using HostChatDemo.Network.Server.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace HostChatDemo.Network.Client
{
    public class ClientMessageHandler
    {
        private static ClientMessageHandler instance = new ClientMessageHandler();

        public static ClientMessageHandler Ins
        {
            get
            {
                return instance;
            }
        }

        public event Action<UserJoin> UserJoinEvent;
        public event Action<UserExit> UserExitEvent;
        public event Action<SendMessage> SendMessageEvent;
        public event Action RoomCloseEvent;
        public event Action ConnectionLostEvent;

        private ClientMessageHandler()
        {
            NetworkClient.Register(MessageType.RoomClose, RoomCloseHandler);
            NetworkClient.Register(MessageType.SendMessage, SendMessageHandler);
            NetworkClient.Register(MessageType.UserExit, UserExitHandler);
            NetworkClient.Register(MessageType.UserJoin, UserJoinHandler);
        }

        private void UserJoinHandler(byte[] data)
        {
            UserJoin userJoin = NetworkUtils.Deserialize<UserJoin>(data);
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                UserJoinEvent?.Invoke(userJoin);
            }));
        }

        private void UserExitHandler(byte[] data)
        {
            UserExit userExit = NetworkUtils.Deserialize<UserExit>(data);
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                UserExitEvent?.Invoke(userExit);
            }));
        }

        private void SendMessageHandler(byte[] data)
        {
            SendMessage sendMessage = NetworkUtils.Deserialize<SendMessage>(data);
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                SendMessageEvent?.Invoke(sendMessage);
            }));
        }

        private void RoomCloseHandler(byte[] data)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                RoomCloseEvent?.Invoke();
            }));
        }

        public void ConnectionLost()
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                ConnectionLostEvent?.Invoke();
            }));
        }
    }
}
