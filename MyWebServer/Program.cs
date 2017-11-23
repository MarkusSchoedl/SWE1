using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace MyWebServer
{
    class Program
    {
        private static HttpListener _listener;

        static void Main(string[] args)
        {
            PluginManager pluginManager = new PluginManager();

            _listener = new HttpListener();
            _listener.Prefixes.Add("http://*:8080/");

            // listener starten
            _listener.Start();

            // auf eingehende requests warten
            // BeginGetContext benutzt dafür ein ThreadPool thread
            _listener.BeginGetContext(new
              AsyncCallback(ContextReceivedCallback), null);

            // server beenden
            Console.ReadLine();

        }

        private static void ContextReceivedCallback(IAsyncResult asyncResult)
        {
            HttpListenerContext context;

            // HttpListenerContext abholen
            context = _listener.EndGetContext(asyncResult);

            // neuen thread für eingehende requests starten
            _listener.BeginGetContext(new
              AsyncCallback(ContextReceivedCallback), null);

            Console.WriteLine("Request für: {0}", context.Request.Url.LocalPath);

            // request verarbeiten
        }
    }
}
