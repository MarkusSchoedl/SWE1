using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using BIF.SWE1.Interfaces;

using static System.Web.MimeMapping;

namespace MyWebServer.Plugins
{
    /// <summary>
    /// <para> Reads files from the given URL and generates an appropiate response.
    /// If the file doesnt exist, 404 is returned. </para>
    /// <para>If no other Plugin wants to handle a request, this plugin is chosen.</para>
    /// </summary>
    [AttributePlugins]
    class StaticFilesPlugin : IPlugin
    {
        #region Fields
        private static String _SiteFolder = "Sites";
        #endregion Fields

        #region Methods
        /// <summary>
        /// Returns how much the plugin wants to handle the request.
        /// </summary>
        /// <param name="req">The request the Browser/Client sent to us.</param>
        /// <returns>0.8 if the file from the URL exists; 0.15 otherwise.</returns>
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

        /// <summary>
        /// Reads a file if it exists and generates an appropiate response.
        /// If the file doesnt exist, 404 is set.
        /// </summary>
        /// <param name="req">The request the Browser/Client sent to us.</param>
        /// <returns>
        /// A response which just needs to be sent. The content of the response is the content of the file.
        /// </returns>
        public IResponse Handle(IRequest req)
        {
            Response rsp = new Response(req); // Status code 200 
            rsp.SetContent(LoadContentFromFile(rsp, req)); // Also sets the mime type!
            return rsp;
        }

        /// <summary>
        /// Loads all the content from a file and returns it.<para />
        /// Sets 404 if no file was not found.<para/>
        /// Also sets an appropiate MIME Type! 
        /// </summary>
        /// <param name="rsp">The response to set</param>
        /// <param name="req">The request we received</param>
        /// <returns>An byte array containing the content of a file; null if file didnt exist.</returns>
        protected byte[] LoadContentFromFile(IResponse rsp, IRequest req)
        {
            // open filestream with req.Url.Path
            try
            {
                if (req.Url.Path.Length > 0)
                {
                    byte[] fileBytes;
                    string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                    string file = req.Url.Path;
                    if (file[0] == '/')
                    {
                        file = file.Remove(0, 1);
                    }

                    string full;
                    if (Path.IsPathRooted(file))
                    {
                        full = file;
                    }
                    else
                    {
                        full = Path.Combine(dir, _SiteFolder, file);
                    }

                    //for jenkins...
                    full = full.Replace("Sites\\./deploy\\static-files", @"static-files");

                    fileBytes = File.ReadAllBytes(full);

                    rsp.ContentType = GetMimeMapping(full);
                    rsp.AddHeader("content-type", rsp.ContentType);

                    return fileBytes;
                }
            }
            catch (Exception ex)
            {
                if (ex is FileNotFoundException)
                {
                    Console.Write("A requested File was not found: {0}\n", req.Url.Path);
                    rsp.StatusCode = 404;
                    return null;
                }

                throw;
            }

            return null;
        }
        #endregion Methods
    }
}
