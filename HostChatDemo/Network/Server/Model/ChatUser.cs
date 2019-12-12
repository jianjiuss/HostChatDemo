using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HostChatDemo.Network.Server.Model
{
    public class ChatUser
    {
        public Socket socket;
        public string userName;

        public void Send(MessageType type, object data = null)
        {
            byte[] b = NetworkUtils.Serialize(data);
            byte[] pb = NetworkUtils.Pack(type, b);
            socket.Send(pb);
        }
    }
}
