using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;
using BIF.SWE1.Interfaces;

namespace MyWebServer.Plugins
{
    /// <summary>
    /// This plugin receives a text in the Request Content and makes it lowercase.
    /// This text is sent back to the client, in the Response content of course.
    /// </summary>
    [AttributePlugins]
    class ToLowerPlugin : IPlugin
    {
        #region Fields
        public const string _Url = "/to-lower";
        private static string _EmptyMessage = "Bitte geben Sie einen Text ein";
        #endregion Fields

        #region Properties
        /// <summary>
        /// The url you have to enter to communicate with this plugin.
        /// </summary>
        public string Url
        {
            get { return _Url; }
        }
        #endregion Properties

        #region Methods
        /// <summary>
        /// Returns how much the plugin wants to handle the request.
        /// </summary>
        /// <param name="req">The request the Browser/Client sent to us.</param>
        /// <returns>A floating point number greater than 0 and smaller or equal to 1.</returns>
        public float CanHandle(IRequest req)
        {
            if(req.Url.Path == _Url)
            {
                return 1.0f;
            }
            return 0.1f;
        }

        /// <summary>
        /// Handles a request and generates an appropiate response. <para/>
        /// Important: The text to lower for has to be set in the content using: <code>"text=" + [TOLOWERTEXT]</code>
        /// </summary>
        /// <param name="req">The request the Browser/Client sent to us.</param>
        /// <returns>A response which just needs to be sent.</returns>
        public IResponse Handle(IRequest req)
        {
            var rsp = new Response(req);
            rsp.StatusCode = 200;

            if (string.IsNullOrEmpty(req.ContentString))
            {
                rsp.SetContent(_EmptyMessage);
                return rsp;
            }
            
            rsp.SetContent(HttpUtility.UrlDecode(req.ContentString).ToLower());

            //received only "text="
            if (req.ContentString.Length <= 5)
            {
                rsp.SetContent(req.ContentString + _EmptyMessage);
            }

            rsp.ContentType = "text/html";

            return rsp;
        }
        #endregion Methods
    }
}
