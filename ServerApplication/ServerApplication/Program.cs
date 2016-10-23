using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        public NetworkStream networkStream { get; set; }
        public StreamReader streamReader { get; set; }
        public StreamWriter streamWriter { get; set; }
        public int RandomNumber { get; set; }
        public string Encryption { get; set; }
        public double Secret { get; set; }

        public void Initialize()
        {
            ipAddress = IPAddress.Parse("127.0.0.1");
            Console.WriteLine("Enter the servers port: ");
            string port = Console.ReadLine();
            serverPort = Int32.Parse(port);

            localEndPoint = new IPEndPoint(ipAddress, serverPort);

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            Random r = new Random();
            RandomNumber = r.Next(1, 15);
        }

        public void StartListening()
        {
            socket.Bind(localEndPoint);
            socket.Listen(10);

            Socket client = socket.Accept();
            IPEndPoint newclient = (IPEndPoint)client.RemoteEndPoint;
            Console.WriteLine("Connected with {0} at port {1}", newclient.Address, newclient.Port);

            networkStream = new NetworkStream(client);
            streamReader = new StreamReader(networkStream);
            streamWriter = new StreamWriter(networkStream);

            bool validation = DiffieHellman();

            string data = String.Empty;

            while (true)
            {
                data = streamReader.ReadLine();
                Console.WriteLine(data);
                data = Decrypt(data);
                Console.WriteLine(data);
            }
        }

        public bool DiffieHellman()
        {
            string serverRequest = "";
            string data = "";
            JObject json = new JObject();
            int sb = RandomNumber;

            data = streamReader.ReadLine();
            Console.WriteLine(String.Format("Client: {0}", data));
            json = JObject.Parse(data);
            string request = (string)json["request"];
            if (request != "keys")
            {
                return false;
            }

            double p = 23;
            double g = 5;
            serverRequest = String.Format("{{ \"p\": {0}, \"g\": {1} }}", p, g);
            streamWriter.WriteLine(serverRequest);
            streamWriter.Flush();
            Console.WriteLine(String.Format("Server: {0}", serverRequest));


            double b = Math.Pow(g, (double)sb) % p;
            serverRequest = String.Format("{{ \"b\": {0} }}", b);
            streamWriter.WriteLine(serverRequest);
            streamWriter.Flush();
            Console.WriteLine(String.Format("Server: {0}", serverRequest));

            data = streamReader.ReadLine();
            Console.WriteLine(String.Format("Client: {0}", data));
            json = JObject.Parse(data);
            string a = (string)json["a"];
            if (a == null)
            {
                return false;
            }

            Secret = Math.Pow(Double.Parse(a), sb) % p;
            Console.WriteLine(Secret);

            data = streamReader.ReadLine();
            Console.WriteLine(String.Format("Client: {0}", data));
            json = JObject.Parse(data);
            string encryption = (string)json["encryption"];
            if (encryption != null)
            {
                //TO DO: Encryption
                if (encryption == "xor")
                {
                    this.Encryption = encryption;
                }
                else if (encryption == "cezar")
                {
                    this.Encryption = encryption;
                }
            }

            return true;
        }

        public string Decrypt(string message)
        {
            List<char> charList = message.ToCharArray().ToList();

            if (Encryption == "xor")
            {
                for (int i = 0; i < charList.Count; i++)
                {
                    charList[i] = (char)(charList[i] ^ ((char)Secret & 255));
                }
            }
            else if (Encryption == "cezar")
            {
                /*for (int i = 0; i < charList.Count; i++)
                {
                    charList[i] = (char)(charList[i] ^ ((char)Secret & 255));
                }*/
            }
            return new string(charList.ToArray());
        }

        static void Main(string[] args)
        {
            Program Server = new Program();
            Server.Initialize();
            Server.StartListening();
        }
    }
}
