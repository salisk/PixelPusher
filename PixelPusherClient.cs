using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace rpi_rgb_led_matrix_sharp
{
    public class PixelPusherClient
    {

        private const int listenPort = 11000;
        private const int matrixSize = 32;

        private static Logger logger = LogManager.GetCurrentClassLogger();
        
        static void Main()
        {
            StartListener();   
        }

        private static void StartListener()
        {
            // Initialize Udp client
            logger.Info($"Starting udp client on port: {listenPort}");
            UdpClient client = new UdpClient(listenPort);
            IPEndPoint localEP = new IPEndPoint(IPAddress.Any, listenPort);
            
            // Initialize the matrix
            logger.Info($"Creating {matrixSize}x{matrixSize} matrix");
            RGBLedMatrix matrix = new RGBLedMatrix(matrixSize, 1, 1);
            var canvas = matrix.CreateOffscreenCanvas();
            
            // Picture variables
            Bitmap bmp;
            Color color;
            System.Drawing.Color px;
            
            try
            {
                while (true)
                {
                    logger.Trace("Waiting for broadcast");
                    byte[] bytes = client.Receive(ref localEP);
                
                    logger.Trace($"Received broadcast from {localEP} :");

                    using (var ms = new MemoryStream(bytes))
                    {
                        bmp = new Bitmap(ms);
                    }
                    logger.Trace($"{bmp.Height}, {bmp.Width}");
                    
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
                logger.Fatal(e, "application encountered an error!");
            }
            finally
            {
                client.Close();
                LogManager.Shutdown();
            }
        }
    }
}