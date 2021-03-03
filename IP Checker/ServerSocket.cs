using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace IP_Checker
{
    public class SynchronousSocketListener
    {
        public static string data = null;
        public static void StartListening(string ip)
        {
            byte[] bytes = new Byte[1024];
            try
            {
                TcpListener server = null;
                Int32 port = 12442;
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");
                server = new TcpListener(localAddr, port);
                server.Start();
                TcpClient client = server.AcceptTcpClient();
                NetworkStream stream = client.GetStream();
                while (true)
                {
                    data = null;
                    bytes = Encoding.ASCII.GetBytes(ip);
                    stream.Write(bytes, 0, bytes.Length);
                    Thread.Sleep(200);
                    client.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

        }
    }
}