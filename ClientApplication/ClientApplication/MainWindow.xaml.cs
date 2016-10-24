using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Net.Sockets;
using System.ComponentModel;
using System.IO;
using Newtonsoft.Json.Linq;

namespace ClientApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public IPAddress ipAddress { get; set; }
        public IPEndPoint remoteEP { get; set; }
        public NetworkStream networkStream { get; set; }
        public StreamWriter streamWriter { get; set; }
        public StreamReader streamReader { get; set; }
        public Socket socket { get; set; }
        public double secret { get; set; }
        public string encryption { get; set; }

        private string userName { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }


        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            userName = NameTextBox.Text;
            if ((bool)XorRadioButton.IsChecked)
            {
                encryption = "xor";
            }
            else if ((bool)CeasarRadioButton.IsChecked)
            {
                encryption = "cezar";
            }
            else
            {
                encryption = "none";
            }

            string ip = IPTextBox.Text;
            int port = Int32.Parse(PortTextBox.Text);
            ipAddress = IPAddress.Parse(ip);
            remoteEP = new IPEndPoint(ipAddress, port);

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(remoteEP);

            networkStream = new NetworkStream(socket);

            streamWriter = new StreamWriter(networkStream);
            streamReader = new StreamReader(networkStream);

            ConversationListBox.Items.Add(String.Format("Socket connected to {0}", socket.RemoteEndPoint.ToString()));

            bool validation = DiffieHellman();

            if (validation == true)
            {
                ConnectButton.IsEnabled = false;
                DisconnectButton.Visibility = Visibility.Visible;
                SendButton.IsEnabled = true;
            }
        }

        private bool DiffieHellman()
        {
            string data = "";
            JObject json = new JObject();
            Random r = new Random();
            int sa = r.Next(1, 15);

            string request = "{ \"request\": \"keys\" }";
            streamWriter.WriteLine(request);
            streamWriter.Flush();
            ConversationListBox.Items.Add(String.Format("Client: {0}", request));

            data = streamReader.ReadLine();
            ConversationListBox.Items.Add(String.Format("Server: {0}", data));
            json = JObject.Parse(data);
            string p = (string)json["p"];
            string g = (string)json["g"];
            if (p == null || g == null)
            {
                return false;
            }

            double a = Math.Pow(Double.Parse(g), (double)sa) % Double.Parse(p);

            request = String.Format("{{ \"a\": {0} }}", a);
            streamWriter.WriteLine(request);
            streamWriter.Flush();
            ConversationListBox.Items.Add(String.Format("Client: {0}", request));

            data = streamReader.ReadLine();
            ConversationListBox.Items.Add(String.Format("Server: {0}", data));
            json = JObject.Parse(data);
            string b = (string)json["b"];
            if (b == null)
            {
                return false;
            }

            secret = Math.Pow(Double.Parse(b), sa) % Double.Parse(p);
            ConversationListBox.Items.Add(secret);

            request = String.Format("{{ \"encryption\": \"{0}\" }}", encryption);
            streamWriter.WriteLine(request);
            streamWriter.Flush();
            ConversationListBox.Items.Add(String.Format("Client: {0}", request));

            return true;
        }

        public string Encrypt(string message)
        {
            List<char> charList = message.ToCharArray().ToList();

            if (encryption == "xor")
            {
                for (int i = 0; i < charList.Count; i++)
                {
                    charList[i] = (char)(charList[i] ^ ((char)secret & 255));
                }
            }
            else if (encryption == "cezar")
            {
                charList = message.ToLower().ToCharArray().ToList();
                for (int i = 0; i < charList.Count; i++)
                {
                    if (charList[i] != 32)
                    {
                        charList[i] = (char)(charList[i] + secret);
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

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string message = MessageTextBox.Text;
            message = Encrypt(message);
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(message);
            message = System.Convert.ToBase64String(plainTextBytes);
            string json = String.Format("{{ \"msg\": \"{0}\", \"from\": \"{1}\" }}", message, userName);

            streamWriter.WriteLine(json);
            streamWriter.Flush();

            ConversationListBox.Items.Add(message);
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            DisconnectButton.Visibility = Visibility.Hidden;
            SendButton.IsEnabled = false;
            ConnectButton.IsEnabled = true;
        }
    }
}
