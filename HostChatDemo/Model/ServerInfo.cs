using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HostChatDemo.Model
{
    public class ServerInfo
    {
        public EndPoint ServerEndPoint { get; set; }
        public IPEndPoint TcpEndPoint { get; set; }
        public string TcpIp
        {
            get
            {
                return TcpEndPoint.Address.ToString();
            }
        }
        public string RoomName { get; set; }
        public float ExpirationTime { get; set; }
    }
}
