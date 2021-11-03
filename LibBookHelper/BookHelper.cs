using System;
using System.Text.Json;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;
using LibData;
using System.Linq; // Needed for .Where()
using System.Text;

namespace BookHelper
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
        public Setting setting; 
        public SequentialHelper()
        {
            //todo: implement the body. Add extra fields and methods to the class if needed
            string settings = File.ReadAllText($"./ClientServerConfig.json");
            setting = JsonSerializer.Deserialize<Setting>(settings);
        }

        public void start()
        {
            //todo: implement the body. Add extra fields and methods to the class if needed 
            #region  Yoinked from https://github.com/afshinamighi/Courses/blob/main/Networking/SimpleCS/Client/Program.cs the sample code who got from school

            #endregion

            IPAddress ipAddress = IPAddress.Parse(setting.BookHelperIPAddress); //changed IP and Port
            IPEndPoint localEndpoint = new IPEndPoint(ipAddress, setting.BookHelperPortNumber);

            Socket sock = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);
            sock.Bind(localEndpoint);
            sock.Listen(setting.ServerListeningQueue);

            //accept socket from Server
            Socket Recsock = sock.Accept();

            while (true)
            {
                int maxBuffSize = 1000;

                byte[] buffer = new byte[maxBuffSize];
                byte[] msg = new byte[maxBuffSize];
                string data = null;
                System.Console.WriteLine("Ready to accept");
                int receivingBytes = Recsock.Receive(buffer);
                data += Encoding.ASCII.GetString(buffer, 0, receivingBytes);
                Console.WriteLine("Received: {0}", data);
                Message message = JsonSerializer.Deserialize<Message>(data);
                //we krijgen sws nooit een message dat NIET bookInquiry is
                Recsock.Send(Encoding.ASCII.GetBytes(JsonSerializer.Serialize(FindMessage(message))));
            }
        }

        public Message FindMessage(Message message)
        {
            string jsonText = File.ReadAllText(@"./Books.json");
            BookData[] books = JsonSerializer.Deserialize<BookData[]>(jsonText); // Je had de verkeerde JsonSerializer   
            Message newMessage = new Message();
            System.Console.WriteLine(message.Content + " DEBUG BOOKHELPER 86.60"); 
            
            try {
                BookData ret = books.Single(x => x.Title == message.Content);
                System.Console.WriteLine(ret.Title + " Debug");
                newMessage.Type = MessageType.BookInquiryReply;
                newMessage.Content = JsonSerializer.Serialize(ret);
                System.Console.WriteLine(newMessage.Content);                
            }
            catch (System.InvalidOperationException) {
                    newMessage.Type = MessageType.NotFound;
                    newMessage.Content = "";
            }
            return newMessage;
        }
    }
}
