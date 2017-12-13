using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BIF.SWE1.Interfaces;
using System.Reflection;
using MyWebServer.MyExceptions;

namespace MyWebServer
{

    /// <summary>
    /// Represents a Response to the client.
    /// </summary>
    public class Response : IResponse
    {
        #region Fields
        private Dictionary<int, string> _HTTP_Statuscodes;

        private Dictionary<string, string> _Headers;
        private String _ContentType;
        private int _StatusCode = 0;
        private String _Response;
        private Byte[] _Content;
        private String _DefaultServer = "BIF-SWE1-Server";

        private Encoding _Encoder = Encoding.UTF8;
        #endregion Fields
        
        #region Constructor
        /// <summary>
        /// Creates a new Instance of the <see cref="Response"/> Class and sets the default values.
        /// </summary>
        public Response()
        {
            _Headers = new Dictionary<string, string>();
            SetHttpStatuscodes();

            AddHeader("Server", _DefaultServer);
        }

        /// <summary>
        /// Creates a new Instance of the <see cref="Response"/> Class and parses all values from the stream which could be relevant later.
        /// </summary>
        /// <param name="req">The request we got from the Client</param>
        public Response(IRequest req)
        {
            if (req.Url == null)
            {
                return;
            }

            _Headers = new Dictionary<string, string>();
            SetHttpStatuscodes();

            if (req.IsValid)
            {
                _StatusCode = _HTTP_Statuscodes.FirstOrDefault(x => x.Value == "OK").Key;
            }
            else
            {
                _StatusCode = _HTTP_Statuscodes.FirstOrDefault(x => x.Value == "Bad Request").Key;
            }

            AddHeader("Server", _DefaultServer);

            _Response = "HTTP/1.1 " + Status;
        }
        #endregion

        #region Properties
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
            get
            {
                if (_Content == null)
                {
                    return 0;
                }
                else
                {
                    return _Content.Length;
                }
            }
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
        /// <exception cref="HTTPStatusCodeNotSetException">No Status Code was set</exception>
        public int StatusCode
        {
            get
            {
                if (_StatusCode != 0)
                {
                    return _StatusCode;
                }
                else
                {
                    throw new HTTPStatusCodeNotSetException();
                }
            }
            set
            {
                _StatusCode = value;
                _Response = "HTTP/1.1 " + Status;
            }
        }
        /// <summary>
        /// Returns the status code as string. (200 OK)
        /// </summary>
        /// <exception cref="HTTPStatusCodeNotSetException">No Status Code was set</exception>
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
                    throw new HTTPStatusCodeNotSetException();
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
                return _Headers["Server"];
            }
            set
            {
                AddHeader("Server", value);
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
            _Content = _Encoder.GetBytes(content);
        }
        /// <summary>
        /// Sets a byte[] as content.
        /// </summary>
        /// <param name="content"></param>
        public void SetContent(byte[] content)
        {
            _Content = content;
        }
        /// <summary>
        /// Sets the stream as content.
        /// </summary>
        /// <param name="stream"></param>
        public void SetContent(Stream stream)
        {
            //TODO stream.length may be unset because its a network
            _Content = new Byte[stream.Length];
            stream.Read(_Content, 0, (int)stream.Length);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Sends the response to the network stream.
        /// </summary>
        /// <param name="network"></param>
        /// <exception cref="RequestNotSetException">The Request was Null. <seealso cref="ArgumentNullException"/></exception>
        /// <exception cref="NoContentSetException">If no content was set for the response</exception>
        /// <exception cref="NetworkNotWriteableException">The network was suddenly not writeable anymore</exception>
        public void Send(Stream network)
        {
            if (_Response == null)
            {
                throw new RequestNotSetException();
            }

            if (network.CanWrite)
            {
                // throw exception if status code is set to OK but no Content(-Type) is set
                if (_StatusCode == 200 && _ContentType != null && _Content == null)
                {
                    throw new NoContentSetException();
                }

                Byte[] response = _Encoder.GetBytes(_Response + "\n");
                network.Write(response, 0, response.Length);

                //write headers
                String headerString = "";
                foreach (var header in _Headers)
                {
                    headerString += header.Key + ": " + header.Value + "\n";
                }

                network.Write(_Encoder.GetBytes(headerString), 0, _Encoder.GetBytes(headerString).Length);

                network.Write(_Encoder.GetBytes("\n"), 0, 1);

                if (_Content != null)
                {
                    //write content
                    network.Write(_Content, 0, _Content.Length);
                }

                network.Flush();
            }
            else
            {
                throw new NetworkNotWriteableException();
            }
        }

        protected void SetHttpStatuscodes()
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
        #endregion Properties
    }
}