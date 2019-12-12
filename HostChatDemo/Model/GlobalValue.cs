using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostChatDemo.Model
{
    public static class GlobalValue
    {
        public static string UserName { get; set; }

        private static bool isInRoom;

        public static bool IsInRoom
        {
            get { return isInRoom; }
            set
            {
                isInRoom = value;
            }
        }

        public static bool IsRoomMaster { get; set; }
    }
}
