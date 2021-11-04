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
        public Socket client_User;
        public Socket sock;
        public Setting setting;
        public Socket Recsock;
        public bool running = true;
        public bool running_INNER = true;
        public SequentialServer()
        {
            //todo: implement the body. Add extra fields and methods to the class if it is needed
            string settings = File.ReadAllText($"../ClientServerConfig.json");
            setting = JsonSerializer.Deserialize<Setting>(settings);
        }


        public void start()
        {
            //todo: implement the body. Add extra fields and methods to the class if it is needed
            //My Port is in setting.ServerPortNumber
            //My IP is in setting.ServerIPAddress

            #region  Yoinked from https://github.com/afshinamighi/Courses/blob/main/Networking/SimpleCS/Client/Program.cs the sample code who got from school
            
            #endregion
            #region Socket_Client
            IPAddress ipAddress = IPAddress.Parse(this.setting.ServerIPAddress);
            IPEndPoint localEndpoint = new IPEndPoint(ipAddress, this.setting.ServerPortNumber);
            sock = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);
            sock.Bind(localEndpoint);
            sock.Listen(this.setting.ServerListeningQueue);
            //accept socket from Client
            
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
                //System.Console.WriteLine("Connected to BookHelper"); 
            }
            catch (SocketException e)
            {
                e.ToString();
                //System.Console.WriteLine("SocketException: {0}", e);
            }
            #endregion
            #region Socket_UserHelper
            try
            {
                if(false){
                   #region do nothing
                     //do nothing   
                    #endregion
                }
                IPAddress IPUserHelper = IPAddress.Parse(this.setting.UserHelperIPAddress);
                IPEndPoint UserserverEndPoint = new IPEndPoint(IPUserHelper, this.setting.UserHelperPortNumber);
                client_User = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client_User.Connect(UserserverEndPoint);
                //System.Console.WriteLine("Connected to userHelper"); 
            }
            catch (SocketException e)
            {
                e.ToString();
                //System.Console.WriteLine("SocketException: {0}", e);
            }
            #endregion
            while (running)
            {
                #region UNSAFE
begin:    
                #endregion
                Recsock = sock.Accept();
                //System.Console.WriteLine(Recsock.Connected);                
                while (running_INNER)
                {
                    #region connect
                    //System.Console.WriteLine("Ready to accept");
                    int maxBuffSize = 1000;            
                    byte[] msg = new byte[maxBuffSize];
                    byte[] buffer = new byte[maxBuffSize];
                    int receivingBytes = Recsock.Receive(buffer);
                    string data = Encoding.ASCII.GetString(buffer, 0, receivingBytes);
                    //System.Console.WriteLine(data + " DEBUG 89");
                    //System.Console.WriteLine("Received: {0} 90", data);
                    #endregion

                    //System.Console.WriteLine(data + " DEBUG 119");
                    if (data == "")
                     {
                        goto begin;
                    }
                    Message message = JsonSerializer.Deserialize<Message>(data);
                    switch (message.Type)
                    {
                        case MessageType.EndCommunication:
                            {
                                try{
                                    client_User.Shutdown(SocketShutdown.Both);
                                    //System.Console.WriteLine("Client User Shutdown");
                                    }
                                    finally{
                                        client_User.Close();
                                        //System.Console.WriteLine("Client User Closed");
                                    }
                                    try{
                                        client_book.Shutdown(SocketShutdown.Both);
                                        //System.Console.WriteLine("Client Book Shutdown");
                                    }
                                    finally{
                                        client_book.Close();
                                        //System.Console.WriteLine("Client Book Closed");                                        
                                    }
                                    try{
                                        Recsock.Shutdown(SocketShutdown.Both);
                                        //System.Console.WriteLine("Client Book Shutdown");
                                    }
                                    finally{
                                        Recsock.Close();
                                        //System.Console.WriteLine("Client Book Closed");                                        
                                    }
                                    running = false;
                                    running_INNER = false;
                                    break;
                            }
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
                                //System.Console.WriteLine("Received: {0} 110", data_book);
                                Message message_book = JsonSerializer.Deserialize<Message>(data_book);
                                if (message_book.Type == MessageType.BookInquiryReply){
                                    send = JsonSerializer.Serialize(message_book);
                                    sendBytes = Encoding.ASCII.GetBytes(send);
                                    Recsock.Send(sendBytes);                                
                                }
                                else if (message_book.Type == MessageType.NotFound){
                                    //TODO: Close everything
                                    send = JsonSerializer.Serialize(message_book);
                                    sendBytes = Encoding.ASCII.GetBytes(send);
                                    Recsock.Send(sendBytes);                                    
                                }
                                break;
                            }
                        case MessageType.BookInquiryReply:
                            {
                                //Should be handeled by Bookinqury not by a message from Client
                                break;
                            }
                        case MessageType.UserInquiry:
                            {
                                string send = JsonSerializer.Serialize(message);
                                byte[] sendBytes = Encoding.ASCII.GetBytes(send);
                                client_User.Send(sendBytes);
                                int receivingUserBytes = client_User.Receive(buffer);

                                string data_user = Encoding.ASCII.GetString(buffer, 0, receivingUserBytes);
                                //System.Console.WriteLine("Received: {0} 110", data_user);
                                Message message_user = JsonSerializer.Deserialize<Message>(data_user);
                                if (message_user.Type == MessageType.UserInquiryReply){
                                    send = JsonSerializer.Serialize(message_user);
                                    sendBytes = Encoding.ASCII.GetBytes(send);
                                    Recsock.Send(sendBytes);                                
                                }
                                else if (message_user.Type == MessageType.NotFound){
                                //TODO: Close everything
                                }
                                else if (message_user.Type == MessageType.Error){
                                    //System.Console.WriteLine("THIS IS IN MESSAGETYPE ERROR 214");
                                    sendBytes = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(message));
                                    Recsock.Send(sendBytes);
                                } 
                                break;
                                
                            }
                        case MessageType.Error:{
                                //System.Console.WriteLine("THIS IS IN MESSAGETYPE ERROR 220");
                                Byte[] sendBytes = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(message));
                                Recsock.Send(sendBytes); 
                            break;
                        }

                        default:
                            {
                                break;
                            }
                    }                
                }
                break;
            }
        }        
    }
}
