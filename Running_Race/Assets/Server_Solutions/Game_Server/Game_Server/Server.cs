using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Game_Server
{
    class Server
    {
        public static int Max_Players { get; private set; }

        public static int Port { get; private set; }
        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();
        private static TcpListener tcp_Listener;

        public static void Start(int _max_Players, int _port_Number)
        {
            Max_Players = _max_Players;
            Port = _port_Number;
            Console.WriteLine("Starting Server...");
            Initialize_Server_Data();
            tcp_Listener = new TcpListener(IPAddress.Any, Port);
            tcp_Listener.Start();
            tcp_Listener.BeginAcceptTcpClient(new AsyncCallback(TCP_Connect_Callback), null);

            Console.WriteLine($"Server started here: Port {Port}");


        }

        private static void TCP_Connect_Callback(IAsyncResult _result)
        {
            TcpClient _client = tcp_Listener.EndAcceptTcpClient(_result);
            tcp_Listener.BeginAcceptTcpClient(new AsyncCallback(TCP_Connect_Callback), null);

            Console.WriteLine($"Incoming connection from {_client.Client.RemoteEndPoint}...");
            for (int i = 1; i <= Max_Players; i++)
            {
                if (clients[i].tcp.socket == null)
                {
                    clients[i].tcp.Connect(_client);
                    return;
                }
            }

            Console.WriteLine($"{_client.Client.RemoteEndPoint} failed to connect to server: Server Full");
        }

        private static void Initialize_Server_Data()
        {
            for (int i = 1; i <= Max_Players; i++)
            {
                clients.Add(i, new Client(i));
            }
        }
    }
}
