using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerApplication
{
    class Program
    {
        public string name;
        public IPHostEntry ipHostInfo { get; set; }
        public IPAddress ipAddress { get; set; }
        public IPEndPoint localEndPoint { get; set; }
        public int serverPort { get; set; }
        public Socket socket { get; set; }

        public void Initialize()
        {
            ipAddress = IPAddress.Parse("127.0.0.1");
            Console.WriteLine("Enter the servers port: ");
            string port = Console.ReadLine();
            serverPort = Int32.Parse(port);

            localEndPoint = new IPEndPoint(ipAddress, serverPort);

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void StartListening()
        {
            socket.Bind(localEndPoint);
            socket.Listen(10);

            Socket client = socket.Accept();
            IPEndPoint newclient = (IPEndPoint)client.RemoteEndPoint;
            Console.WriteLine("Connected with {0} at port {1}", newclient.Address, newclient.Port);

            NetworkStream ns = new NetworkStream(client);
            StreamReader sr = new StreamReader(ns);
            StreamWriter sw = new StreamWriter(ns);

            string welcome = "Welcome";
            sw.WriteLine(welcome);
            sw.Flush();

            string data = String.Empty;

            while (true)
            {
                data = sr.ReadLine();
                Console.WriteLine(data);
            }
        }

        static void Main(string[] args)
        {
            Program Server = new Program();
            Server.Initialize();
            Server.StartListening();
        }
    }
}
