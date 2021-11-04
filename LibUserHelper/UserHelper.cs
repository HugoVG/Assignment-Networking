using System;
using System.Text.Json;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;
using LibData;
using System.Linq; 
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
        private Setting setting; // geen static zetten 
        public SequentialHelper()
        {
            string settings = (File.ReadAllText("./ClientServerConfig.json"));
            this.setting = JsonSerializer.Deserialize<Setting>(settings);

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

            IPAddress ipAddress = IPAddress.Parse(setting.UserHelperIPAddress);
            IPEndPoint localEndpoint = new IPEndPoint(ipAddress, setting.UserHelperPortNumber);

            Socket sock = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);
            sock.Bind(localEndpoint);
            sock.Listen(setting.ServerListeningQueue);

            //accept socket from Client
            Socket Recsock = sock.Accept();

            try{
                while (Recsock.Connected)
                {
                    System.Console.WriteLine("Ready to accept");
                    int receivingBytes = Recsock.Receive(buffer);
                    data += Encoding.ASCII.GetString(buffer, 0, receivingBytes);
                    Console.WriteLine("Received: {0}", data);

                    Message message = JsonSerializer.Deserialize<Message>(data);
                    Recsock.Send(Encoding.ASCII.GetBytes(JsonSerializer.Serialize(ExtractInfo(message))));
                    // Thanks for the one-line. i hate to admit it mr dit maakt mijn level zeer simpel 
                    
                    //TODO: Here something todo what data we got
                }
            }
            catch (System.Net.Sockets.SocketException e)
            {
                e.ToString();
                //kinda wrong but also very right
            }
        }

        public Message ExtractInfo(Message message)
        {
            string userInfo = File.ReadAllText(@"./Users.json");
            UserData[] users = JsonSerializer.Deserialize<UserData[]>(userInfo);
            Message newMessage = new Message();
            newMessage.Type = MessageType.UserInquiryReply;

            System.Console.WriteLine(message.Content + " DEBUGING USERHELPER...");
            UserData user;
            try{
                 user = users.Single(x => x.User_id == message.Content);
            }
            catch (Exception e)
            {
                e.ToString(); // no errors ;)
                newMessage.Type = MessageType.NotFound;
                newMessage.Content = "User not found";
                return newMessage;
            }
            System.Console.WriteLine(user.Name + " Debug");
            newMessage.Content = JsonSerializer.Serialize(user);
            System.Console.WriteLine(newMessage.Content);                
            return newMessage;

            // ik heb hier geen try catch gedaan want volgens de stappen heb je geen situatie waar er een boek/ user niet gevonden is
        }
    }
}