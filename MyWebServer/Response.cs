using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BIF.SWE1.Interfaces;

namespace MyWebServer
{
    public class Response : IResponse
    {
        private Dictionary<int, string> _HTTP_Statuscodes;

        private Dictionary<string, string> _Headers;
        private int _ContentLength;
        private string _ContentType;
        private int _StatusCode;
        private string _Response = "BIF-SWE1-Server";

        //encoder!

        public Response()
        {
            _Headers = new Dictionary<string, string>();
            setHttpStatuscodes();
            _StatusCode = 0;
        }

        public Response(IRequest req)
        {
            _Headers = new Dictionary<string, string>();
            setHttpStatuscodes();
            _StatusCode = 0;

            if(req.IsValid)
            {
                _StatusCode = _HTTP_Statuscodes.FirstOrDefault(x => x.Value == "OK").Key;
            }
            else
            {
                _StatusCode = _HTTP_Statuscodes.FirstOrDefault(x => x.Value == "Bad Request").Key;
            }

       

            // open filestream with req.Url.Path
        }


        /// <summary>
        /// Returns a writable dictionary of the response headers. Never returns null.
        /// </summary>
        public IDictionary<string, string> Headers
        {
            get { return _Headers; } // Should be get only
        }// = new Dictionary<string, string>();

        /// <summary>
        /// Returns the content length or 0 if no content is set yet.
        /// </summary>
        public int ContentLength
        {
            get { return _ContentLength; }
        }

        /// <summary>
        /// Gets or sets the content type of the response.
        /// </summary>
        /// <exception cref="InvalidOperationException">A specialized implementation may throw a InvalidOperationException when the content type is set by the implementation.</exception>
        public string ContentType
        {
            get { return _ContentType; }
            set { _ContentType = value; }
        }

        /// <summary>
        /// Gets or sets the current status code. An Exceptions is thrown, if no status code was set.
        /// </summary>
        public int StatusCode
        {
            get
            {
                if (_StatusCode == 0)
                {
                    throw new Exception();
                }
                return _StatusCode;
            }
            set { _StatusCode = value; }
        }
        /// <summary>
        /// Returns the status code as string. (200 OK)
        /// </summary>
        public string Status
        {
            get
            {
                if (_HTTP_Statuscodes.ContainsKey(_StatusCode))
                {
                    return _StatusCode.ToString() + " " + _HTTP_Statuscodes[_StatusCode];
                }
                else
                {
                    throw new Exception();
                }
            }
        }

        /// <summary>
        /// Gets or sets the Server response header. Defaults to "BIF-SWE1-Server".
        /// </summary>
        public string ServerHeader
        {
            get
            {
                return _Response;
            }
            set
            {
                _Response = value;
            }
        }

        /// <summary>
        /// Adds or replaces a response header in the headers dictionary.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="value"></param>
        public void AddHeader(string header, string value)
        {
            if (_Headers.ContainsKey(header))
            {
                _Headers[header] = value;
            }
            else
            {
                _Headers.Add(header, value);
            }
        }

        /// <summary>
        /// Sets a string content. The content will be encoded in UTF-8.
        /// </summary>
        /// <param name="content"></param>
        public void SetContent(string content)
        {

        }
        /// <summary>
        /// Sets a byte[] as content.
        /// </summary>
        /// <param name="content"></param>
        public void SetContent(byte[] content)
        {

        }
        /// <summary>
        /// Sets the stream as content.
        /// </summary>
        /// <param name="stream"></param>
        public void SetContent(Stream stream)
        {

        }

        /// <summary>
        /// Sends the response to the network stream.
        /// </summary>
        /// <param name="network"></param>
        public void Send(Stream network)
        {

        }

        protected void setHttpStatuscodes()
        {
            _HTTP_Statuscodes = new Dictionary<int, string>();

            /* This is by far NOT the full list! */
            _HTTP_Statuscodes.Add(100, "Continue");
            _HTTP_Statuscodes.Add(101, "Switching Protocols");
            _HTTP_Statuscodes.Add(102, "Processing");
            _HTTP_Statuscodes.Add(200, "OK");
            _HTTP_Statuscodes.Add(201, "Created");
            _HTTP_Statuscodes.Add(202, "Accepted");
            _HTTP_Statuscodes.Add(203, "Non - Authoritative Information");
            _HTTP_Statuscodes.Add(204, "No Content");
            _HTTP_Statuscodes.Add(205, "Reset Content");
            _HTTP_Statuscodes.Add(300, "Multiple Choices");
            _HTTP_Statuscodes.Add(301, "Moved Permanently");
            _HTTP_Statuscodes.Add(400, "Bad Request");
            _HTTP_Statuscodes.Add(401, "Unauthorized");
            _HTTP_Statuscodes.Add(403, "Forbidden");
            _HTTP_Statuscodes.Add(404, "Not Found");
            _HTTP_Statuscodes.Add(405, "Method Not Allowed");
            _HTTP_Statuscodes.Add(500, "Internal Server Error");
            _HTTP_Statuscodes.Add(501, "Not Implemented");
            _HTTP_Statuscodes.Add(502, "Bad Gateway");
            _HTTP_Statuscodes.Add(503, "Service Unavailable");
        }
    }
}