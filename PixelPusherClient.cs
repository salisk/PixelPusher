using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace rpi_rgb_led_matrix_sharp
{
    public class PixelPusherClient
    {

        private const int listenPort = 11000;
        
        static void Main()
        {
            StartListener();   
        }

        private static void StartListener()
        {
            UdpClient client = new UdpClient(listenPort);
            IPEndPoint localEP = new IPEndPoint(IPAddress.Any, listenPort);

            try
            {
                while (true)
                {
                    Console.WriteLine("Waiting for broadcast");
                    byte[] bytes = client.Receive(ref localEP);
                
                    Console.WriteLine($"Received broadcast from {localEP} :");
                    Console.WriteLine($" {Encoding.ASCII.GetString(bytes, 0, bytes.Length)}");

                    Bitmap bmp;
                    using (var ms = new MemoryStream(bytes))
                    {
                        bmp = new Bitmap(ms);
                    }
                    Console.WriteLine($"{bmp.Height}, {bmp.Width}");
                }
            } 
            catch (SocketException e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                client.Close();
            }
        }
    }
}