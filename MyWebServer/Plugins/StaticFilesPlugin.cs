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
    public class StaticFilesPlugin : IPlugin
    {
        #region Fields
        private static String _SiteFolder = "Sites";

        private const float _CanHandleReturn = 0.8f;
        private const float _CannotHandleReturn = 0.15f;

        private const string _DefaultNotFoundSite = "404.html";
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
                return _CanHandleReturn;
            }
            else
            {
                return _CannotHandleReturn;
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
            SetResponseContentFromFile(rsp, req); // Also sets the mime type!
            return rsp;
        }

        /// <summary>
        /// Loads all the content from a file and returns it.<para />
        /// Sets 404 if no file was not found.<para/>
        /// Also sets an appropiate MIME Type! 
        /// </summary>
        /// <param name="rsp">The response to set</param>
        /// <param name="req">The request we received</param>
        protected void SetResponseContentFromFile(IResponse rsp, IRequest req)
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

                    rsp.SetContent(fileBytes);
                    rsp.ContentType = GetMimeMapping(full);
                    rsp.AddHeader("content-type", rsp.ContentType);

                    return;
                }
            }

            //File Not Found
            catch (Exception ex)
            {
                rsp.ContentType= "text/html";
                rsp.AddHeader("content-type", rsp.ContentType);

                if (ex is FileNotFoundException || ex is DirectoryNotFoundException)
                {
                    try
                    {
                        Console.Write("A requested File was not found: {0}\n", req.Url.Path);
                        rsp.StatusCode = 404;
                        string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                        byte[] fileBytes = File.ReadAllBytes(Path.Combine(dir, _SiteFolder, _DefaultNotFoundSite));
                        rsp.SetContent(fileBytes);
                        return;
                    }
                    catch (Exception ex404)
                    {
                        if (ex404 is FileNotFoundException || ex404 is DirectoryNotFoundException)
                        {
                            Console.WriteLine("ERROR: The 404-Page could not be opened. Message: {0}", ex404.Message);
                            rsp.SetContent("<html><body>FILE NOT FOUND<body></html>");
                            return;
                        }
                        throw;
                    }
                }

                else if(ex is UnauthorizedAccessException)
                {
                    Console.Write("A request was rejected due to Unauthorized Access: {0}\n", req.Url.Path);
                    rsp.StatusCode = 401;
                    rsp.SetContent("<html><body>UNAUTHORIZED ACCESS<body></html>");
                    return;
                }
                throw;
            }
        }
        #endregion Methods
    }
}
