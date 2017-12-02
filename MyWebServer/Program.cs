using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Net.Sockets;
using BIF.SWE1.Interfaces;

namespace MyWebServer
{
    class Program
    {
        private static HttpListener _listener;

        static void Main(string[] args)
        {
            PluginManager pluginManager = new PluginManager();

            TcpListener server = null;
            try
            {
                // Set the TcpListener on port 13000.
                Int32 port = 8080;
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(localAddr, port);

                // Start listening for client requests.
                server.Start();

                // Enter the listening loop.
                while (true)
                {
                    Console.Write("Waiting for a connection... ");

                    // Perform a blocking call to accept requests.
                    // You could also use server.AcceptSocket() here.
                    //Socket client = server.AcceptSocket();
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("Connected!");

                    //var stream = new NetworkStream(client);
                    var stream = client.GetStream();

                    var req = new Request(stream);

                    if (req.Url != null)
                    {
                        IPlugin plugin = pluginManager.GetHighestPlugin(req);
                        var rsp = plugin.Handle(req);

                        // Send back a response.
                        rsp.Send(stream);
                    }

                    // Shutdown and end connection
                    client.Close();
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }


            Console.WriteLine("\nHit enter to continue...");
            Console.Read();
        }
    }
}
