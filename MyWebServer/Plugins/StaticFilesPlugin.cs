using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using BIF.SWE1.Interfaces;

namespace MyWebServer
{
    class StaticFilesPlugin : IPlugin
    {
        public float CanHandle(IRequest req)
        {
            if (File.Exists(req?.Url?.Path))
            {
                return 0.8f;
            }
            else
            {
                return 0.15f;
            }
        }

        public IResponse Handle(IRequest req)
        {
            return new Response(req);
        }
    }
}
