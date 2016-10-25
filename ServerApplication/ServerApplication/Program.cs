using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerApplication
{
    class Program
    {
        public IPHostEntry ipHostInfo { get; set; }
        public IPAddress ipAddress { get; set; }
        public IPEndPoint localEndPoint { get; set; }
        public int serverPort { get; set; }
        public Socket socket { get; set; }
        public List<Socket> clientSockets { get; set; }
        public List<Session> clientSessions { get; set; }

        public void Initialize()
        {
            clientSockets = new List<Socket>();
            clientSessions = new List<Session>();

            try
            {
                Console.Write("Enter the servers IP: ");
                string ip = Console.ReadLine();
                ipAddress = IPAddress.Parse(ip);
                Console.Write("Enter the servers port: ");
                string port = Console.ReadLine();
                serverPort = Int32.Parse(port);
            }
            catch (FormatException ex)
            {
                Console.WriteLine("Invalid data.");
                Console.ReadLine();
                return;
            }

            localEndPoint = new IPEndPoint(ipAddress, serverPort);

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(localEndPoint);
            socket.Listen(10);

            Thread listeningThread = new Thread(new ThreadStart(StartListening));
            listeningThread.Start();
        }

        public void StartListening()
        {
            Session session;
            Thread sessionThread;
            Socket client;

            Console.WriteLine("Listening...");

            while (true)
            {
                client = socket.Accept();
                clientSockets.Add(client);
                session = new Session(client, this);
                clientSessions.Add(session);
                sessionThread = new Thread(new ThreadStart(session.ManageClientSession));
                sessionThread.Start();
            }
        }

        public void Broadcast(string message, string author)
        {
            foreach (Session session in clientSessions)
            {
                session.SendToClient(message, author);
            }
        }

        public void deleteSocket(Socket socket)
        {
            clientSockets.Remove(socket);   
        }

        public void deleteSession(Session session)
        {
            clientSessions.Remove(session);
        }

        static void Main(string[] args)
        {
            Program Server = new Program();
            Server.Initialize();
        }
    }
}
