using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostChatDemo.Network.Server
{
    public enum MessageType
    {
        None,
        HeartBeat,
        RoomClose,
        UserJoin,
        UserExit,
        SendMessage
    }
}
