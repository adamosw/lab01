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
    class Session
    {
        public Socket clientSocket { get; set; }
        public Program Server { get; set; }
        public NetworkStream networkStream { get; set; }
        public StreamReader streamReader { get; set; }
        public StreamWriter streamWriter { get; set; }
        public string Encryption { get; set; }
        public double Secret { get; set; }
        public int RandomNumber { get; set; }

        public Session(Socket clientSocket, Program server)
        {
            this.clientSocket = clientSocket;
            this.Server = server;
        }

        public void ManageClientSession()
        {
            IPEndPoint newclient = (IPEndPoint)clientSocket.RemoteEndPoint;
            Console.WriteLine("Connected with {0} at port {1}", newclient.Address, newclient.Port);

            networkStream = new NetworkStream(clientSocket);
            streamReader = new StreamReader(networkStream);
            streamWriter = new StreamWriter(networkStream);

            Random r = new Random();
            RandomNumber = r.Next(1, 15);

            bool validation = DiffieHellman();

            string data = String.Empty;
            JObject json = new JObject();
            string message = "";
            string author = "";

            while ((data = streamReader.ReadLine()) != null)
            {
                Console.WriteLine(data);
                json = JObject.Parse(data);
                message = (string)json["msg"];
                author = (string)json["from"];
                var base64EncodedBytes = System.Convert.FromBase64String(message);
                message = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
                data = Decrypt(message);
                Server.Broadcast(data, author);
                Console.WriteLine(data);
            }

            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();

            Server.deleteSocket(clientSocket);
            Server.deleteSession(this);
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
            Encryption = (string)json["encryption"];
            if (Encryption != null)
            {
                //TO DO: Encryption
                if (Encryption == "xor")
                {
                    this.Encryption = Encryption;
                }
                else if (Encryption == "cezar")
                {
                    this.Encryption = Encryption;
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
                    charList[i] = (char)(charList[i] ^ ((char)Secret & 0xFF));
                }
            }
            else if (Encryption == "cezar")
            {
                charList = message.ToLower().ToCharArray().ToList();
                for (int i = 0; i < charList.Count; i++)
                {
                    if (charList[i] != 32)
                    {
                        charList[i] = (char)(charList[i] - Secret);
                        if (charList[i] > 'z')
                        {
                            charList[i] = (char)(charList[i] - 26);
                        }
                        else if (charList[i] < 'a')
                        {
                            charList[i] = (char)(charList[i] + 26);
                        }
                    }
                }
            }
            return new string(charList.ToArray());
        }

        public string Encrypt(string message)
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
                charList = message.ToLower().ToCharArray().ToList();
                for (int i = 0; i < charList.Count; i++)
                {
                    if (charList[i] != 32)
                    {
                        charList[i] = (char)(charList[i] + Secret);
                        if (charList[i] > 'z')
                        {
                            charList[i] = (char)(charList[i] - 26);
                        }
                        else if (charList[i] < 'a')
                        {
                            charList[i] = (char)(charList[i] + 26);
                        }
                    }
                }
            }
            return new string(charList.ToArray());
        }

        public void SendToClient(string message, string author)
        {
            message = Encrypt(message);
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(message);
            message = System.Convert.ToBase64String(plainTextBytes);
            string json = String.Format("{{ \"msg\": \"{0}\", \"from\": \"{1}\" }}", message, author);

            streamWriter.WriteLine(json);
            streamWriter.Flush();    
        }
    }
}
