using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Text.Json;
using LibData;
using System.Text;

namespace LibClient
{
    // Note: Do not change this class
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

    // Note: Do not change this class
    public class Output
    {
        public string Client_id { get; set; } // the id of the client that requests the book
        public string BookName { get; set; } // the name of the book to be reqyested
        public string Status { get; set; } // final status received from the server
        public string BorrowerName { get; set; } // the name of the borrower in case the status is borrowed, otherwise null
        public string BorrowerEmail { get; set; } // the email of the borrower in case the status is borrowed, otherwise null
    }

    // Note: Complete the implementation of this class. You can adjust the structure of this class.
    public class SimpleClient
    {
        // some of the fields are defined.
        public Output result;
        public Socket clientSocket;
        public IPEndPoint serverEndPoint;
        public IPAddress ipAddress;
        public Setting settings;
        public string client_id;
        private string bookName;
        // all the required settings are provided in this file
        public string configFile = @"./ClientServerConfig.json";
        //public string configFile = @"../../../../ClientServerConfig.json"; // for debugging

        // todo: add extra fields here in case needed
        Message message;
        byte[] buffer;
        /// <summary>
        /// Initializes the client based on the given parameters and seeting file.
        /// </summary>
        /// <param name="id">id of the clients provided by the simulator</param>
        /// <param name="bookName">name of the book to be requested from the server, provided by the simulator</param>
        public SimpleClient(int id, string bookName)
        {
            System.Console.WriteLine("Test");
            //todo: extend the body if needed.
            this.bookName = bookName;
            this.client_id = "client-" + id.ToString();
            this.result = new Output();
            result.BookName = bookName;
            result.Client_id = this.client_id;
            // read JSON directly from a file
            string configContent = File.ReadAllText(configFile);
            this.settings = JsonSerializer.Deserialize<Setting>(configContent);
            this.ipAddress = IPAddress.Parse(settings.ServerIPAddress);
            this.serverEndPoint = new IPEndPoint(ipAddress, settings.ServerPortNumber);
            this.clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);          
        }
        public Message sendMessage(Message messageARG)
        {
            string json = JsonSerializer.Serialize(messageARG);
            System.Console.WriteLine(json); //DEBUG SEND
            byte[] data = Encoding.ASCII.GetBytes(json);
            this.clientSocket.Send(data);
            int recv = this.clientSocket.Receive(buffer);
            System.Console.WriteLine(recv); //DEBUG RECEIVE
            string response = Encoding.ASCII.GetString(buffer, 0, recv);
            Message responseMessage = JsonSerializer.Deserialize<Message>(response);
            return responseMessage;
        }

        /// <summary>
        /// Establishes the connection with the server and requests the book according to the specified protocol.
        /// Note: The signature of this method must not change.
        /// </summary>
        /// <returns>The result of the request</returns>
        public Output start()
        {
            
            // todo: implement the body to communicate with the server and requests the book. Return the result as an Output object.
            // Adding extra methods to the class is permitted. The signature of this method must not change.
            buffer = new byte[1024];
            message = new Message();
            message.Type = MessageType.Hello;
            message.Content = client_id;
            clientSocket.Connect(serverEndPoint);  
            Message responseMessage = sendMessage(message);
            if (responseMessage.Type == MessageType.Welcome)
            {
                message.Type = MessageType.BookInquiry;
                message.Content = bookName;
                responseMessage = sendMessage(message);
                if (responseMessage.Type == MessageType.BookInquiryReply)
                {
                    System.Console.WriteLine(responseMessage.Content + " 106"); //DEBUG RECEIVE
                    BookData book = JsonSerializer.Deserialize<BookData>(responseMessage.Content);
                    result.Client_id = client_id;
                    result.BookName = book.Title;
                    result.Status = book.Status;
                    System.Console.WriteLine(book.Status); //DEBUG RECEIVE);                    
                    if (book.Status == "Borrowed")
                    {
                        message.Type = MessageType.UserInquiry;
                        message.Content = book.BorrowedBy;
                        responseMessage = sendMessage(message);
                        if (responseMessage.Type == MessageType.UserInquiryReply)
                        {
                            System.Console.WriteLine(responseMessage.Content); //DEBUG RECEIVE
                            UserData user = JsonSerializer.Deserialize<UserData>(responseMessage.Content);
                            result.BorrowerName = user.Name;
                            result.BorrowerEmail = user.Email;
                        }
                    }
                    else
                    {
                        result.BorrowerName = null;
                        result.BorrowerEmail = null;
                    }                    
                }
            }
            //DEBUG
            System.Console.WriteLine(JsonSerializer.Serialize(result));
            clientSocket.Disconnect(false);        
            return result;
        }
    }
}