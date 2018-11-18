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
            // Initialize Udp client
            UdpClient client = new UdpClient(listenPort);
            IPEndPoint localEP = new IPEndPoint(IPAddress.Any, listenPort);
            System.Drawing.Color px;
            
            // Initialize the matrix
            RGBLedMatrix matrix = new RGBLedMatrix(32, 1, 1);
            var canvas = matrix.CreateOffscreenCanvas();
            Color color;
            
            try
            {
                while (true)
                {
                    Console.WriteLine("Waiting for broadcast");
                    byte[] bytes = client.Receive(ref localEP);
                
                    Console.WriteLine($"Received broadcast from {localEP} :");

                    Bitmap bmp;
                    using (var ms = new MemoryStream(bytes))
                    {
                        bmp = new Bitmap(ms);
                    }
                    Console.WriteLine($"{bmp.Height}, {bmp.Width}");
                    
                    for (int i = 0; i < bmp.Height; i++)
                    {
                        for (int j = 0; j < bmp.Width; j++)
                        {
                            // Get bmp pixel
                            px = bmp.GetPixel(j, i);
                            
                            // Convert to matrix color
                            color = new Color(px.R, px.G, px.B);
                            
                            // Set pixel to canvas
                            canvas.SetPixel(j, i, color); 
                        }
                    }
                    
                    // Swap on vsync
                    canvas = matrix.SwapOnVsync(canvas);
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