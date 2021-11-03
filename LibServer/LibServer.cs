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
        public Socket client_book;
        public Socket sock;
        public Setting setting;
        public Socket Recsock;
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
            #endregion
            #region Socket_Client
            IPAddress ipAddress = IPAddress.Parse(this.setting.UserHelperIPAddress);
            IPEndPoint localEndpoint = new IPEndPoint(ipAddress, this.setting.UserHelperPortNumber);
            sock = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);
            sock.Bind(localEndpoint);
            sock.Listen(this.setting.ServerListeningQueue);
            //accept socket from Client
            Recsock = sock.Accept();
            #endregion
            #region Socket_BookHelper
            try
            {
                if(false){
                   #region do nothing
                     //do nothing   
                    #endregion
                }
                IPAddress IPBookHelper = IPAddress.Parse(this.setting.BookHelperIPAddress);
                IPEndPoint BookserverEndPoint = new IPEndPoint(IPBookHelper, this.setting.BookHelperPortNumber);
                client_book = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client_book.Connect(BookserverEndPoint);
                System.Console.WriteLine("Connected to BookHelper"); 
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            #endregion


            while (true)
            {
                System.Console.WriteLine("Starting Receiving Data from Client");
                
                int receivingBytes = Recsock.Receive(buffer);
                string data = Encoding.ASCII.GetString(buffer, 0, receivingBytes);
                System.Console.WriteLine(data + " DEBUG 89");
                Console.WriteLine("Received: {0} 90", data);
                Message message = JsonSerializer.Deserialize<Message>(data);
                switch (message.Type)
                {
                    case MessageType.Hello:
                        {
                            message.Type = MessageType.Welcome;
                            message.Content = "";
                            string send = JsonSerializer.Serialize(message);
                            byte[] sendBytes = Encoding.ASCII.GetBytes(send);
                            Recsock.Send(sendBytes);                                 
                            break;
                        }
                    case MessageType.BookInquiry:
                        {
                            string send = JsonSerializer.Serialize(message);
                            byte[] sendBytes = Encoding.ASCII.GetBytes(send);
                            client_book.Send(sendBytes);
                            int receivingBookBytes = client_book.Receive(buffer);

                            string data_book = Encoding.ASCII.GetString(buffer, 0, receivingBookBytes);
                            Console.WriteLine("Received: {0} 110", data_book);
                            Message message_book = JsonSerializer.Deserialize<Message>(data_book);
                            if (message_book.Type == MessageType.BookInquiryReply){
                                send = JsonSerializer.Serialize(message_book);
                                sendBytes = Encoding.ASCII.GetBytes(send);
                                Recsock.Send(sendBytes);                                
                            }
                            else if (message_book.Type == MessageType.NotFound){
                                //PepeSadge
                            }
                            break;
                        }
                    case MessageType.BookInquiryReply:
                        {
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }


                //TODO: Here something todo what data we got
                // if (data.IndexOf("<EOF>") > -1)
                // {
                //     data = data.TrimEnd("<EOF>".ToCharArray());
                //     System.Console.WriteLine(data);
                //     data = null;
                // }
                // hugo added this to see what we got from client
            }
        }
        public Message ReceiveMessage(Message message)
        {
            return message;
        }
    }
}
