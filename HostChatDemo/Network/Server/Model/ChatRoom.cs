using HostChatDemo.Network.Server.DataModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HostChatDemo.Network.Server.Model
{
    public class ChatRoom
    {
        List<ChatRecord> chatRecords = new List<ChatRecord>();

        ConcurrentDictionary<Socket, ChatUser> chatUsers = new ConcurrentDictionary<Socket, ChatUser>();

        public void UserJoin(ChatUser chatUser)
        {
            chatUsers[chatUser.socket] = chatUser;

            //将当前房间里的所有用户信息发送给新加入的用户
            foreach(var user in chatUsers.Values)
            {
                chatUser.Send(MessageType.UserJoin, new UserJoin() {
                    UserName = user.userName,
                    Suc = true
                });
            }

            //将当前聊天内容最后10条记录发送给新加入的用户
            int lastStartIndex = chatRecords.Count - 10;
            var lastChatRecords = chatRecords.Skip(lastStartIndex > 0 ? lastStartIndex : 0).ToList();
            foreach(var records in lastChatRecords)
            {
                chatUser.Send(MessageType.SendMessage, new SendMessage() {
                    Content = records.content,
                    SendDateTime = records.sendDateTime,
                    Suc = true,
                    UserName = records.chatUser.userName
                });
            }

            //通知所有房间里用户有新用户加入
            foreach(var user in chatUsers.Values)
            {
                if(user.socket == chatUser.socket)
                {
                    continue;
                }

                user.Send(MessageType.UserJoin, new UserJoin() {
                    UserName = chatUser.userName,
                    Suc = true
                });
            }
        }

        public void UserExit(ChatUser chatUser)
        {
            foreach(var user in chatUsers.Values)
            {
                user.Send(MessageType.UserExit, new UserExit() {
                    UserName = chatUser.userName,
                    Suc = true
                });
            }

            ChatUser r;
            chatUsers.TryRemove(chatUser.socket, out r);
        }

        public void LoseUserConnection(Socket socket)
        {
            ChatUser r;
            if(chatUsers.TryRemove(socket, out r))
            {
                foreach(var user in chatUsers.Values)
                {
                    user.Send(MessageType.UserExit, new UserExit() {
                        UserName = r.userName,
                        Suc = true
                    });
                }
            }

        }

        public void SendContent(ChatRecord chatRecord)
        {
            chatRecords.Add(chatRecord);
            foreach(var user in chatUsers.Values)
            {
                user.Send(MessageType.SendMessage, new SendMessage()
                {
                    Content = chatRecord.content,
                    UserName = chatRecord.chatUser.userName,
                    SendDateTime = chatRecord.sendDateTime,
                    Suc = true
                });
            }
        }

        public void Close(Socket masterSocket)
        {
            foreach(var user in chatUsers)
            {
                if(user.Key == masterSocket)
                {
                    continue;
                }
                user.Value.Send(MessageType.RoomClose);
            }

            chatUsers[masterSocket].Send(MessageType.RoomClose);
        }

        public ChatUser GetChatUser(Socket socket)
        {
            ChatUser c;
            if (chatUsers.TryGetValue(socket, out c))
            {
                return c;
            }
            else
            {
                return null;
            }
        }

    }
}
