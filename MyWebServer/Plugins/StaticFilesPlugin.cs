using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BIF.SWE1.Interfaces;

namespace MyWebServer
{
    class StaticFilesPlugin : IPlugin
    {
        public float CanHandle(IRequest req)
        {
            return 0.999f;
        }

        public IResponse Handle(IRequest req)
        {
            return new Response(req);
        }
    }
}
