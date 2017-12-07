using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BIF.SWE1.Interfaces;

namespace MyWebServer
{
    /// <summary>
    /// Represents a Request by the Client.
    /// </summary>
    public class Request : IRequest
    {
        #region Fields
        private Dictionary<string, string> _Headers;
        private string _Method;
        private bool _IsValid;
        private Url _Url;
        private int _HeaderCount;
        //private int _ContentLength;
        private string _ContentType;
        private Byte[] _ContentBytes;

        private static readonly string[] _ValidOnes = { "GET", "POST", "HEAD", "PUT", "PATCH", "DELETE", "TRACE", "OPTIONS", "CONNECT" };
        #endregion Fields

        #region Constructor
        /// <summary>
        /// Initializes a new <see cref="Request"/> and parses all the data from the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The networkstream to the Client.</param>
        public Request(System.IO.Stream stream)
        {
            _Headers = new Dictionary<string, string>();
            _IsValid = false;
            _ContentType = "";

            if (stream.CanRead)
            {
                StreamReader reader = new StreamReader(stream);
                ParseStream(reader);
            }
        }
        #endregion

        #region Methods
        protected void ParseStream(System.IO.StreamReader reader)
        {
            // parse the first line
            string requestLine = reader.ReadLine();
            if (String.IsNullOrEmpty(requestLine))
            {
                return;
            }
            string[] requestFields = requestLine.Split(' ');
            if (requestFields.Count() != 3) // not enough arguments
            {
                return;
            }
            _Method = requestFields[0].ToUpper();
            _Url = new MyWebServer.Url(requestFields[1]);

            string line;
            string[] keyNvalue;
            while ((line = reader.ReadLine()) != null && !String.IsNullOrEmpty(line))
            {
                keyNvalue = line.Split(new string[] { ": " }, StringSplitOptions.RemoveEmptyEntries);
                _Headers.Add(keyNvalue[0].ToLower(), keyNvalue[1]);
                _HeaderCount++;
            }
            
            int contentLength;
            if (_Headers.ContainsKey("content-length") && Int32.TryParse(_Headers["content-length"], out contentLength) && contentLength > 0)
            {
                char[] buff = new char[contentLength];
                reader.ReadBlock(buff, 0, contentLength);

                _ContentBytes = Encoding.UTF8.GetBytes(buff);
            }

            if (_Headers.Count() > 0 && IsMethodValid())
            {
                _IsValid = true;
            }
        }
        #endregion

        #region Properties
        protected bool IsMethodValid()
        {
            return _ValidOnes.Contains(_Method.ToUpper());
        }

        /// <summary>
        /// Returns true if the request is valid. A request is valid, if method and url could be parsed. A header is not necessary.
        /// </summary>
        public bool IsValid
        {
            get { return _IsValid; }
        }

        /// <summary>
        /// Returns the request method in UPPERCASE. get -> GET.
        /// </summary>
        public string Method
        {
            get { return _Method; }
        }

        /// <summary>
        /// Returns a URL object of the request. Never returns null.
        /// </summary>
        public IUrl Url
        {
            get { return _Url; }
        }

        /// <summary>
        /// Returns the request header. Never returns null. All keys must be lower case.
        /// </summary>
        public IDictionary<string, string> Headers
        {
            get { return new Dictionary<string, string>(_Headers); }
        }

        /// <summary>
        /// Returns the user agent from the request header
        /// </summary>
        public string UserAgent
        {
            get { return _Headers["user-agent"]; }
        }

        /// <summary>
        /// Returns the number of header or 0, if no header where found.
        /// </summary>
        public int HeaderCount
        {
            get { return _HeaderCount; }
        }

        /// <summary>
        /// Returns the parsed content length request header.
        /// </summary>
        public int ContentLength
        {
            get
            {
                if (Int32.TryParse(_Headers["content-length"], out int ret))
                {
                    return ret;
                }
                else
                {
                    throw new CouldntConvertContentLengthException();
                }
            }
        }

        /// <summary>
        /// Returns the parsed content type request header. Never returns null.
        /// </summary>
        public string ContentType
        {
            get
            {
                _ContentType = _Headers["content-type"];
                return _ContentType;
            }
        }

        /// <summary>
        /// Returns the request content (body) stream or null if there is no content stream.
        /// </summary>
        public Stream ContentStream
        {
            get
            {
                return new MemoryStream(_ContentBytes);
            }
        }

        /// <summary>
        /// Returns the request content (body) as string or null if there is no content.
        /// </summary>
        public string ContentString
        {
            get
            {
                if (_ContentBytes == null) return null;
                return Encoding.UTF8.GetString(_ContentBytes);
            }
        }

        /// <summary>
        /// Returns the request content (body) as byte[] or null if there is no content.
        /// </summary>
        public byte[] ContentBytes
        {
            get
            {
                return _ContentBytes;
            }
        }
        #endregion Properties
    }
}