using System;

public class Session
{
    public Socket socket { get; set; }
    public NetworkStream networkStream { get; set; }
    public StreamReader streamReader { get; set; }
    public StreamWriter streamWriter { get; set; }
    public string Encryption { get; set; }
    public double Secret { get; set; }
}
