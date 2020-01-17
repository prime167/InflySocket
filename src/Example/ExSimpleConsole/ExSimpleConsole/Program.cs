using System;
using System.Threading;
using InflySocket;

namespace ExSimpleConsole
{
    class Program
    {
        private static readonly InflyServerSimple Server = new InflyServerSimple();
        private static readonly InflyClientSimple Client = new InflyClientSimple();

        static void Main(string[] args)
        {
            Server.OnNewConnectedEvent += Server_OnNewConnectedEvent;
            Server.OnReceiveMessageEvent += Server_OnReceiveMessageEvent;
            Server.OnCloseEvent += Server_OnCloseEvent;

            Client.OnReceiveMessageEvent += Client_OnReceiveMessageEvent;
            Client.OnConnectedEvent += Client_OnConnectedEvent;
            Client.OnCloseEvent += Client_OnCloseEvent;

            Server.Listen(9999);

            Client.Connect("127.0.0.1", 9999);
            Thread.Sleep(1000);
            for (int i = 0; i < 5; i++)
            {
                Client.Send(i + "" + '\n');
                Thread.Sleep(1000);
            }

            Console.ReadLine();
        }

        private static void Server_OnCloseEvent(SessionBase newClient)
        {
            Console.WriteLine($"已断开：{newClient.EndPoint}{'\n'}");
        }

        private static void Server_OnReceiveMessageEvent(string msg)
        {
            Console.WriteLine($"服务器收到消息：{msg}");
            Server.Send($"{msg}-a{'\n'}");
        }

        private static void Server_OnNewConnectedEvent(SessionBase newClient)
        {
            Console.WriteLine($"新连接：{newClient.EndPoint}{'\n'}");
        }

        private static void Client_OnReceiveMessageEvent(string msg)
        {
            Console.WriteLine($"客户端收到消息：{msg}");
        }

        private static void Client_OnConnectedEvent()
        {
            Console.WriteLine($"已连接到服务器{'\n'}");
        }

        private static void Client_OnCloseEvent()
        {
            Console.WriteLine($"已断开{'\n'}");
        }
    }
}
