using System;
using System.Text.Json;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using LibData;


namespace LibServer
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
    public class SequentialServer
    {
        public Setting setting;
        public SequentialServer()
        {
            //todo: implement the body. Add extra fields and methods to the class if it is needed
            string settings = File.ReadAllText($"./ClientServerConfig.json");
            setting = JsonSerializer.Deserialize<Setting>(settings);
        }


        public void start()
        {
            //todo: implement the body. Add extra fields and methods to the class if it is needed
            //My Port is in setting.ServerPortNumber
            //My IP is in setting.ServerIPAddress

            #region  Yoinked from https://github.com/afshinamighi/Courses/blob/main/Networking/SimpleCS/Client/Program.cs the sample code who got from school
            int maxBuffSize = 1000;

            byte[] buffer = new byte[maxBuffSize];
            byte[] msg = new byte[maxBuffSize];
            string data = null;
            #endregion

            IPAddress ipAddress = IPAddress.Parse(this.setting.ServerIPAddress);
            IPEndPoint localEndpoint = new IPEndPoint(ipAddress, this.setting.ServerPortNumber);

            Socket sock = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);
            sock.Bind(localEndpoint);
            sock.Listen(this.setting.ServerListeningQueue);

            //accept socket from Client
            Socket Recsock = sock.Accept();

            while (true)
            {
                int receivingBytes = Recsock.Receive(buffer);
                data += Encoding.ASCII.GetString(buffer, 0, receivingBytes);
                //TODO: Here something todo what data we got
                System.Console.WriteLine(data);


            }
            



        }
    }

}



