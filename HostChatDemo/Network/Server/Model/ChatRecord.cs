using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostChatDemo.Network.Server.Model
{
    public class ChatRecord
    {
        public ChatUser chatUser;

        public DateTime sendDateTime;

        public string content;
    }
}
