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
        public Socket socket { get; set; }

        private string userName { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }


        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            userName = NameTextBox.Text;

            string ip = IPTextBox.Text;
            int port = Int32.Parse(PortTextBox.Text);
            ipAddress = IPAddress.Parse(ip);
            remoteEP = new IPEndPoint(ipAddress, port);

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(remoteEP);

            networkStream = new NetworkStream(socket);

            ConversationListBox.Items.Add(String.Format("Socket connected to {0}", socket.RemoteEndPoint.ToString()));
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string message = MessageTextBox.Text;

            StreamWriter streamWriter = new StreamWriter(networkStream);
            streamWriter.WriteLine(message);
            streamWriter.Flush();

            ConversationListBox.Items.Add(message);
        }
    }
}
