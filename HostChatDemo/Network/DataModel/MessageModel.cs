using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostChatDemo.Network.Server.DataModel
{
    [Serializable]
    public class UserJoin
    {
        public string UserName;

        public bool Suc;
    }

    [Serializable]
    public class UserExit
    {
        public string UserName;

        public bool Suc;
    }

    [Serializable]
    public class SendMessage
    {
        public string UserName;

        public DateTime SendDateTime;

        public string Content;

        public bool Suc;
    }
}
