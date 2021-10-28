using System;
using System.Text.Json;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;
using LibData;
using System.Text;

namespace UserHelper
{
    
    // Note: Do not change this class.
    public class Setting
    {
        public int ServerPortNumber { get; set; }
        public int BookHelperPortNumber { get; set; }
        public int UserHelperPortNumber { get; set; }
        public string ServerIPAddress { get; set; }
        public string BookHelperIPAddress { get; set; }
        public string UserHelperIPAddress { get; set; }
        public int ServerListeningQueue { get; set; }
    }

    // Note: Complete the implementation of this class. You can adjust the structure of this class.
    public class SequentialHelper
    {
        private static Setting setting;
        public SequentialHelper()
        {
            setting = JsonSerializer.Deserialize<Setting>(File.ReadAllText("setting.json"));
            //todo: implement the body. Add extra fields and methods to the class if needed
        }

        public void start()
        {
            //todo: implement the body. Add extra fields and methods to the class if needed
            //My Port is in setting.ServerPortNumber
            //My IP is in setting.ServerIPAddress

            #region  Yoinked from https://github.com/afshinamighi/Courses/blob/main/Networking/SimpleCS/Client/Program.cs the sample code who got from school
            int maxBuffSize = 1000;

            byte[] buffer = new byte[maxBuffSize];
            byte[] msg = new byte[maxBuffSize];
            string data = null;
            #endregion

            IPAddress ipAddress = IPAddress.Parse(setting.ServerIPAddress);
            IPEndPoint localEndpoint = new IPEndPoint(ipAddress, setting.ServerPortNumber);

            Socket sock = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);
            sock.Bind(localEndpoint);
            sock.Listen(setting.ServerListeningQueue);

            //accept socket from Client
            Socket Recsock = sock.Accept();

            while (true)
            {
                int receivingBytes = Recsock.Receive(buffer);
                data += Encoding.ASCII.GetString(buffer, 0, receivingBytes);
                //TODO: Here something todo what data we got
                if (data.IndexOf("<EOF>") > -1)
                {
                    data = data.TrimEnd("<EOF>".ToCharArray());
                    System.Console.WriteLine(data);
                    data = null;
                }
            }
        }
    }
}
