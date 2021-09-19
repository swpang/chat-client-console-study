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
                        case Static
                    }
                }
            }
        }
    }
}
