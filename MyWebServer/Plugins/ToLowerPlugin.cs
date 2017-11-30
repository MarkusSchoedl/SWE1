using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BIF.SWE1.Interfaces;

namespace MyWebServer
{
    class ToLowerPlugin : IPlugin
    {
        #region Parameters
        public const string _Url = "/to-lower";
        private static string _EmptyMessage = "Bitte geben Sie einen Text ein";
        #endregion Parameters

        #region SettersGetters
        public string Url
        {
            get { return _Url; }
        }
        #endregion SettersGetters

        #region Methods
        public float CanHandle(IRequest req)
        {
            if(req.Url.RawUrl == _Url)
            {
                return 1.0f;
            }
            return 0.1f;
        }

        public IResponse Handle(IRequest req)
        {
            var rsp = new Response(req);
            rsp.StatusCode = 200;

            if (string.IsNullOrEmpty(req.ContentString))
            {
                rsp.SetContent(_EmptyMessage);
                return rsp;
            }
            
            rsp.SetContent(req.ContentString.ToLower());

            //received only "text="
            if (req.ContentString.Length <= 5)
            {
                rsp.SetContent(req.ContentString + _EmptyMessage);
            }

            return rsp;
        }
        #endregion Methods
    }
}
