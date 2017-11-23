using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BIF.SWE1.Interfaces;

namespace MyWebServer
{
    class ToLowerPlugin : IPlugin
    {
        private static string _EmptyMessage = "Bitte geben Sie einen Text ein";

        public float CanHandle(IRequest req)
        {
            return 0.09f;
        }

        public IResponse Handle(IRequest req)
        {
            var rsp = new Response(req);
            
            rsp.SetContent(req.ContentString.ToLower());

            //received only "text="
            if (req.ContentString.Length <= 5)
            {
                rsp.SetContent(req.ContentString + _EmptyMessage);
            }


            return rsp;
        }
    }
}
