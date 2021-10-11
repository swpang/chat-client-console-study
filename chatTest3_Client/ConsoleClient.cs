using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace chatTest3_Client
{
    class ConsoleClient
    {
        TcpClient client = null;
        Thread receiveMessageThread = null;
        ConcurrentBag<string> sendMessageListToView = null;
        ConcurrentBag<string> receiveMessageListToView = null;

        private string name = null;
        private string id = null;
        private string parsedName = null;
        private string chatroom = null;
        private char usertype = '\0';
        public string parser = "$$#$$";
        public string connect_header = "UserInfo";
        public string getclientlist_header = "GetClientList";
        public string disconnect_header = "Disconnect";
        public string message_header = "Message";

        public void Run()
        {
            sendMessageListToView = new ConcurrentBag<string>();
            receiveMessageListToView = new ConcurrentBag<string>();
            receiveMessageThread = new Thread(receiveMessage);

            while (true)
            {
                Console.WriteLine("=============Client=============");
                Console.WriteLine("1. 서버 연결");
                Console.WriteLine("2. Message 보내기");
                Console.WriteLine("3. 보낸 Message 확인");
                Console.WriteLine("4. 받은 Message 확인");
                Console.WriteLine("0. 종료");
                Console.WriteLine("================================");

                string key = Console.ReadLine();
                int order = 0;

                if (int.TryParse(key, out order))
                {
                    switch (order)
                    {
                        case StaticDefine.CONNECT:
                            if (client != null)
                            {
                                Console.WriteLine("Already Connected");
                                Console.ReadKey();
                            }
                            else
                                Connect();
                            break;
                        case StaticDefine.SEND_MESSAGE:
                            if (client == null)
                            {
                                Console.WriteLine("Connect with the Server first");
                                Console.ReadKey();
                            }
                            else
                                SendMessage();
                            break;
                        case StaticDefine.SEND_MSG_VIEW:
                            SendMessageView();
                            break;
                        case StaticDefine.RECEIVE_MSG_VIEW:
                            ReceiveMessageView();
                            break;
                        case StaticDefine.EXIT:
                            receiveMessageThread.Abort();
                            if (client != null)
                                client.Close();
                            return;
                    }
                }

                else
                {
                    Console.WriteLine("Wrong Input");
                    Console.ReadKey();
                }
                Console.Clear();
                Thread.Sleep(50);
            }
        }
        private void ReceiveMessageView()
        {
            if (receiveMessageListToView.Count == 0)
            {
                Console.WriteLine("No Messages Received");
                Console.ReadKey();
                return;
            }
            foreach (var item in receiveMessageListToView)
                Console.WriteLine(item);
            Console.ReadKey();
        }
        private void SendMessageView()
        {
            if (sendMessageListToView.Count == 0)
            {
                Console.WriteLine("No Messages Sent");
                Console.ReadKey();
                return;
            }
            foreach (var item in sendMessageListToView)
                Console.WriteLine(item);
            Console.ReadKey();
        }
        private void receiveMessage()
        {
            string receiveMessage = "";
            while (true)
            {
                byte[] receiveByte = new byte[1024];
                client.GetStream().Read(receiveByte, 0, receiveByte.Length);

                receiveMessage = Encoding.Default.GetString(receiveByte);
                ParsingReceiveMessage(receiveMessage);
            }
            Thread.Sleep(500);
        }
        private void ParsingReceiveMessage(string receiveMessage)
        {
            List<string> receiveMessageList = new List<string>();
            string[] receiveMessageArray = receiveMessage.Split(parser);
            for (int i = 0; i < receiveMessageArray.Length; i++)
            {
                if (String.IsNullOrEmpty(receiveMessageArray[i]))
                    continue;
                if (receiveMessageArray[i] == "Dummy")
                    if (receiveMessageArray[i + 2] == "Dummy")
                        break;
                receiveMessageList.Add(receiveMessageArray[i]);
            }

            string type = receiveMessageList[0];

            switch (type)
            {
                case "Message":
                    type_Message(receiveMessageList);
                    break;
                case "ClientList":
                    type_ClientList(receiveMessageList);
                    break;
            }
        }
        private void type_Message(List<string> messageList)
        {
            string sender = messageList[1];
            string message = messageList[2];

            Console.WriteLine(string.Format("[Message has arrived] {0} : {1}", sender, message));
            receiveMessageListToView.Add(string.Format("[{0}] Sender : {1}, Message : {2}",
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), sender, message));
        }

        private void type_ClientList(List<string> messageList)
        {
            string userList = "";
            string[] splittedUser = messageList[1].Split("%%");
            foreach (var el in splittedUser)
            {
                if (string.IsNullOrEmpty(el))
                    continue;
                userList += el + " ";
            }
            Console.WriteLine(string.Format("[Connected Clients] {0}", userList));
            messageList.Clear();
            return;
        }

        private void SendMessage()
        {

            string getUserList = string.Format(parser 
                + getclientlist_header
                + parser
                + chatroom
                + parser
                + parsedName);
            byte[] getUserByte = Encoding.Default.GetBytes(getUserList);
            client.GetStream().Write(getUserByte, 0, getUserByte.Length);

            try
            {
                Console.WriteLine("Who do you want to send a message to? (Choose from client list)");
                string receiver = Console.ReadLine();

                Console.WriteLine("\nWrite the message : ");
                string message = Console.ReadLine();

                if (string.IsNullOrEmpty(receiver) || string.IsNullOrEmpty(message))
                {
                    Console.WriteLine("Check the receiptant and message");
                    Console.ReadKey();
                    return;
                }

                string parsedMessage = string.Format(
                    parser
                    + message_header
                    + parser
                    + chatroom
                    + parser
                    + receiver
                    + parser
                    + parsedName
                    + parser
                    + message);

                byte[] byteData = new byte[parsedMessage.Length];
                byteData = Encoding.Default.GetBytes(parsedMessage);

                client.GetStream().Write(byteData, 0, byteData.Length);
                sendMessageListToView.Add(string.Format("[{0}] Receiver : {1}, Message : {2}",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), receiver, message));
                Console.WriteLine("Successfully Sent");
                Console.ReadKey();
            }
            
            catch (Exception e) { }
        }

        private void Connect()
        {
            Console.Write("Your username : ");
            name = Console.ReadLine();

            Console.Write("\nYour student ID : ");
            id = Console.ReadLine();

            parsedName = name + '_' + id;
            if (String.IsNullOrEmpty(parsedName))
            {
                Console.WriteLine("Invalid Name");
                Console.ReadKey();
                return;
            }

            Console.Write("\nStudent [1], Supervisor [2] : ");
            usertype = Console.ReadKey().KeyChar;

            if (!((usertype == '1') || (usertype == '2')))
            {
                Console.WriteLine("Wrong Choice");
                Console.ReadKey();
                return;
            }
            Console.ReadKey();

            Console.Write("\n\nEnter Chatroom Key : ");
            chatroom = Console.ReadLine();

            if (String.IsNullOrEmpty(chatroom))
            {
                Console.WriteLine("Invalid Chatroom Key");
                Console.ReadKey();
                return;
            }

            client = new TcpClient();
            client.Connect(StaticDefine.HOST_ADDRESS, 9999);

            string parsedMessage = string.Format(
                parser
                + connect_header
                + parser
                + parsedName
                + parser
                + usertype
                + parser
                + chatroom);

            Console.WriteLine(parsedMessage);

            byte[] byteData = new byte[parsedMessage.Length];
            byteData = Encoding.UTF8.GetBytes(parsedMessage);
            client.GetStream().Write(byteData, 0, byteData.Length);

            receiveMessageThread.Start();

            Console.WriteLine("Successfully connected to server. You can now send messages");
            Console.ReadKey();
        }
    }
}
