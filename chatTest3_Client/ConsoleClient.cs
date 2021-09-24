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
                            if (client != null)
                                client.Close();
                            receiveMessageThread.Abort();
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
            List<string> receiveMessageList = new List<string>();
            while (true)
            {
                byte[] receiveByte = new byte[1024];
                client.GetStream().Read(receiveByte, 0, receiveByte.Length);

                receiveMessage = Encoding.Default.GetString(receiveByte);

                string[] receiveMessageArray = receiveMessage.Split('>');
                foreach (var item in receiveMessageArray)
                {
                    if (!item.Contains('<'))
                        continue;
                    if (item.Contains("Admin<TEST"))
                        continue;
                    receiveMessageList.Add(item);
                }
                ParsingReceiveMessage(receiveMessageList);

                Thread.Sleep(500);
            }
        }
        private void ParsingReceiveMessage(List<string> messageList)
        {
            foreach (var item in messageList)
            {
                string sender = "";
                string message = "";

                if (item.Contains('<'))
                {
                    string[] splittedMsg = item.Split('<');

                    sender = splittedMsg[0];
                    message = splittedMsg[1];

                    if (sender == "Admin")
                    {
                        string userList = "";
                        string[] splittedUser = message.Split('$');
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
                    Console.WriteLine(string.Format("[Message has arrived] {0} : {1}", sender, message));
                    receiveMessageListToView.Add(string.Format("[{0}] Sender : {1}, Message : {2}",
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), sender, message));
                }
            }
            messageList.Clear();
        }

        private void SendMessage()
        {
            string getUserList = string.Format("{0}<GiveMeUserList>", name);
            byte[] getUserByte = Encoding.Default.GetBytes(getUserList);
            client.GetStream().Write(getUserByte, 0, getUserByte.Length);

            Console.WriteLine("Who do you want to send a message to?");
            string receiver = Console.ReadLine();

            Console.WriteLine("Write the message : ");
            string message = Console.ReadLine();

            if (string.IsNullOrEmpty(receiver) || string.IsNullOrEmpty(message))
            {
                Console.WriteLine("Check the receiptant and message");
                Console.ReadKey();
                return;
            }

            string parsedMessage = string.Format("{0}<{1}>", receiver, message);

            byte[] byteData = new byte[1024];
            byteData = Encoding.Default.GetBytes(parsedMessage);

            client.GetStream().Write(byteData, 0, byteData.Length);
            sendMessageListToView.Add(string.Format("[{0}] Receiver : {1}, Message : {2}",
                DateTime.Now.ToString("yyy-MM-dd HH:mm:ss"), receiver, message));
            Console.WriteLine("Successfully Sent");
            Console.ReadKey();
        }

        private void Connect()
        {
            Console.WriteLine("Your name : ");
            name = Console.ReadLine();

            string parsedName = "%^&" + name;
            if (parsedName == "%^&")
            {
                Console.WriteLine("Invalid Name");
                Console.ReadKey();
                return;
            }

            client = new TcpClient();
            client.Connect("127.0.0.1", 9999);

            byte[] byteData = new byte[1024];
            byteData = Encoding.Default.GetBytes(parsedName);
            client.GetStream().Write(byteData, 0, byteData.Length);

            receiveMessageThread.Start();

            Console.WriteLine("Successfully connected to server. You can now send messages");
            Console.ReadKey();
        }
    }
}
