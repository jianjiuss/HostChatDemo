using HostChatDemo.Model;
using HostChatDemo.Network.Client;
using HostChatDemo.Network.Server;
using HostChatDemo.Network.Server.DataModel;
using HostChatDemo.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HostChatDemo.ViewModel
{
    public class MainVM : BaseVM
    {
        public Action RenameClick;

        private bool isSelfExitRoom;

        private ObservableCollection<ChatRecord> chatRecords;

        public ObservableCollection<ChatRecord> ChatRecords
        {
            get { return chatRecords; }
            set
            {
                chatRecords = value;
                OnPropertyChanged(() => ChatRecords);
            }
        }

        private ObservableCollection<ChatUserInfo> chatUserInfos;

        public ObservableCollection<ChatUserInfo> ChatUserInfos
        {
            get { return chatUserInfos; }
            set
            {
                chatUserInfos = value;
                OnPropertyChanged(() => ChatUserInfos);
            }
        }

        private string status;

        public string Status
        {
            get { return status; }
            set
            {
                status = value;
                OnPropertyChanged(() => Status);
            }
        }

        private string content;

        public string Content
        {
            get { return content; }
            set
            {
                content = value;
                OnPropertyChanged(() => Content);
            }
        }

        private ICommand sendContentCmd;

        public ICommand SendContentCmd
        {
            get
            {
                return sendContentCmd = sendContentCmd ?? new RelayCommand(SendContentExecute, SendContentCanExecute);
            }
        }

        private void SendContentExecute(object obj)
        {
            NetworkClient.Send(MessageType.SendMessage, new SendMessage()
            {
                UserName = GlobalValue.UserName,
                Content = this.Content,
                SendDateTime = DateTime.Now
            });

            Content = string.Empty;
        }

        private bool SendContentCanExecute(object arg)
        {
            return GlobalValue.IsInRoom;
        }

        private ICommand exitCurRoomCmd;

        public ICommand ExitCurRoomCmd
        {
            get
            {
                return exitCurRoomCmd = exitCurRoomCmd ?? new RelayCommand(ExitCurRoomExecute, ExitCurRoomCanExecute);
            }
        }

        private void ExitCurRoomExecute(object obj)
        {
            Console.WriteLine("退出当前房间");

            if(GlobalValue.IsRoomMaster)
            {
                NetworkClient.Send(MessageType.RoomClose);
            }
            else
            {
                isSelfExitRoom = true;
                NetworkClient.Send(MessageType.UserExit, new UserExit() {
                    UserName = GlobalValue.UserName
                });
            }

        }

        private bool ExitCurRoomCanExecute(object arg)
        {
            return GlobalValue.IsInRoom;
        }

        private ICommand renameCmd;

        public ICommand RenameCmd
        {
            get
            {
                return renameCmd = renameCmd ?? new RelayCommand(RenameExecute, RenameCanExecute);
            }
        }

        private void RenameExecute(object obj)
        {
            RenameClick?.Invoke();
        }

        private bool RenameCanExecute(object arg)
        {
            return !GlobalValue.IsInRoom;
        }

        public MainVM()
        {
            chatRecords = new ObservableCollection<ChatRecord>();
            chatUserInfos = new ObservableCollection<ChatUserInfo>();

            ClientMessageHandler.Ins.SendMessageEvent += SendMessageHandler;
            ClientMessageHandler.Ins.UserJoinEvent += UserJoinHandler;
            ClientMessageHandler.Ins.UserExitEvent += UserExitHandler;
            ClientMessageHandler.Ins.RoomCloseEvent += RoomCloseHandler;
            ClientMessageHandler.Ins.ConnectionLostEvent += ConnectionLostHandler;
        }

        private void ConnectionLostHandler()
        {
            MessageBox.Show("与服务端的连接丢失");

            ChatUserInfos.Clear();
            ChatRecords.Clear();

            NetworkClient.Stop();

            GlobalValue.IsInRoom = false;
            MainWindow.VM.Status = "Not in room....";
        }

        private void RoomCloseHandler()
        {
            MessageBox.Show("当前房间已关闭");
            ChatUserInfos.Clear();
            ChatRecords.Clear();

            NetworkClient.Stop();

            if(GlobalValue.IsRoomMaster)
            {
                GlobalValue.IsRoomMaster = false;
                BroadcastServer.Stop();
                NetworkServer.Stop();
            }

            GlobalValue.IsInRoom = false;
            MainWindow.VM.Status = "Not in room....";
        }

        private void UserExitHandler(UserExit userExit)
        {
            if(isSelfExitRoom)
            {
                isSelfExitRoom = false;
                MessageBox.Show("你已经离开了房间");
                ChatUserInfos.Clear();
                ChatRecords.Clear();
                NetworkClient.Stop();
                GlobalValue.IsInRoom = false;
                MainWindow.VM.Status = "Not in room....";
                return;
            }

            ChatUserInfo chatUserInfo = ChatUserInfos.FirstOrDefault(c => c.UserName.Equals(userExit.UserName));
            if(chatUserInfo != null)
            {
                ChatUserInfos.Remove(chatUserInfo);
            }
        }

        private void UserJoinHandler(UserJoin userJoin)
        {
            ChatUserInfos.Add(new ChatUserInfo() { UserName = userJoin.UserName });
        }

        private void SendMessageHandler(SendMessage sendMessage)
        {
            var record = new ChatRecord()
            {
                UserName = sendMessage.UserName,
                Content = sendMessage.Content,
                SendDateTime = sendMessage.SendDateTime
            };

            ChatRecords.Add(record);
        }
    }
}
