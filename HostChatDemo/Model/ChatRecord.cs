using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostChatDemo.Model
{
    public class ChatRecord
    {
        public string UserName { get; set; }
        public string Content { get; set; }
        public string SendDateTimeStr
        {
            get
            {
                return SendDateTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }
        public DateTime SendDateTime { get; set; }
    }
}
