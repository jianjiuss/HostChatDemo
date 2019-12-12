using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace HostChatDemo.Network.Server
{
    public static class NetworkUtils
    {
        //序列化：obj -> byte[]
        public static byte[] Serialize(object obj)
        {
            //对象必须被标记为Serializable
            if (obj == null || !obj.GetType().IsSerializable)
                return null;
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream())
            {
                formatter.Serialize(stream, obj);
                byte[] data = stream.ToArray();
                return data;
            }
        }

        //反序列化：byte[] -> obj
        public static T Deserialize<T>(byte[] data) where T : class
        {
            //T必须是可序列化的类型
            if (data == null || !typeof(T).IsSerializable)
                return null;
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream(data))
            {
                object obj = formatter.Deserialize(stream);
                return obj as T;
            }
        }

        /// <summary>
        /// 获取本机IPv4,获取失败则返回null
        /// </summary>
        public static string GetLocalIPv4()
        {
            string hostName = Dns.GetHostName(); //得到主机名
            IPHostEntry iPEntry = Dns.GetHostEntry(hostName);
            for (int i = 0; i < iPEntry.AddressList.Length; i++)
            {
                //从IP地址列表中筛选出IPv4类型的IP地址
                if (iPEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                    return iPEntry.AddressList[i].ToString();
            }
            return null;
        }

        public static byte[] Pack(MessageType type, byte[] data = null)
        {
            List<byte> list = new List<byte>();
            if (data != null)
            {
                list.AddRange(BitConverter.GetBytes((ushort)(4 + data.Length)));//消息长度2字节
                list.AddRange(BitConverter.GetBytes((ushort)type));             //消息类型2字节
                list.AddRange(data);                                            //消息内容n字节
            }
            else
            {
                list.AddRange(BitConverter.GetBytes((ushort)4));                //消息长度2字节
                list.AddRange(BitConverter.GetBytes((ushort)type));             //消息类型2字节
            }
            return list.ToArray();
        }
    }
}
