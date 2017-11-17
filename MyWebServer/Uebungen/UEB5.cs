using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BIF.SWE1.Interfaces;
using System.IO;
using MyWebServer;

namespace Uebungen
{
    public class UEB5 : IUEB5
    {
        string _StaticFileFolder;

        public void HelloWorld()
        {
        }

        public IPluginManager GetPluginManager()
        {
            return new PluginManager();
        }

        public IRequest GetRequest(System.IO.Stream network)
        {
            return new Request(network);
        }

        public IPlugin GetStaticFilePlugin()
        {
            return (IPlugin) new StaticFilesPlugin();
        }

        public string GetStaticFileUrl(string fileName)
        {
            return Path.Combine(_StaticFileFolder, fileName);
        }

        public void SetStatiFileFolder(string folder)
        {
            _StaticFileFolder = folder;
        }
    }
}
