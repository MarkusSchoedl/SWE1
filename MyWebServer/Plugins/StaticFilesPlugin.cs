using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using BIF.SWE1.Interfaces;
using System.Reflection;

namespace MyWebServer
{
    [AttributePlugins]
    class StaticFilesPlugin : IPlugin
    {
        #region Parameters
        private static String _SiteFolder = "Sites";
        #endregion Parameters

        #region Methods
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
            Response rsp = new Response(req);
            rsp.SetContent(LoadContentFromFile(rsp, req));
            return rsp;
        }
        
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
                    fileBytes = File.ReadAllBytes(full);

                    rsp.ContentType = "text/" + Path.GetExtension(full).Replace(".", "");
                    rsp.AddHeader("content-type", rsp.ContentType);

                    return fileBytes;
                }
            }
            catch (Exception ex)
            {
                if (ex is FileNotFoundException || ex is DirectoryNotFoundException)
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
