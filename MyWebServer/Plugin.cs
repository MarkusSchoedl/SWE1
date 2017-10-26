﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BIF.SWE1.Interfaces;

namespace MyWebServer
{
    public class Plugin : IPlugin
    {
        /// <summary>
        /// Returns a score between 0 and 1 to indicate that the plugin is willing to handle the request. The plugin with the highest score will execute the request.
        /// </summary>
        /// <param name="req"></param>
        /// <returns>A score between 0 and 1</returns>
        public float CanHandle(IRequest req)
        {
            Response resp = new Response(req);

            if(resp.StatusCode == 200 || (req.Url.Parameter.ContainsKey("test_plugin") &&  req.Url.Parameter["test_plugin"] == "true"))
            {
                return 0.5f;
            }
            else
            {
                return 0f;
            }
        }

        /// <summary>
        /// Called by the server when the plugin should handle the request.
        /// </summary>
        /// <param name="req"></param>
        /// <returns>A new response object.</returns>
        public IResponse Handle(IRequest req)
        {
            return new Response(req);
        }
    } 
}