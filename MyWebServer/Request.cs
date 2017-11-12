using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BIF.SWE1.Interfaces;

namespace MyWebServer
{
    public class Request : IRequest
    {
        #region Parameters
        private Dictionary<string, string> _Headers;
        private string _Method;
        private bool _IsValid;
        private Url _Url;
        private int _HeaderCount;
        //private int _ContentLength;
        private string _ContentType;
        private Stream _ContentStream;
        private Byte[] _ContentBytes;
        private string _ContentString;

        private static readonly string[] _ValidOnes = { "GET", "POST", "HEAD", "PUT", "PATCH", "DELETE", "TRACE", "OPTIONS", "CONNECT" };
        #endregion Parameters

        public Request(System.IO.Stream stream)
        {
            _Headers = new Dictionary<string, string>();
            _IsValid = false;
            _ContentType = "";

            if (stream.CanRead)
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    ParseStream(reader);
                }
            }
        }

        protected void ParseStream(System.IO.StreamReader reader)
        {
            if (reader.EndOfStream)
            {
                return;
            }

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

            if (!reader.EndOfStream)
            {
                _ContentString = reader.ReadToEnd();
                _ContentStream = GenerateStreamFromString(_ContentString);
                _ContentBytes = Encoding.UTF8.GetBytes(_ContentString);
            }

            if (_Headers.Count() > 0 && isMethodValid())
            {
                _IsValid = true;
            }
        }

        protected bool isMethodValid()
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
                int ret;
                if(Int32.TryParse(_Headers["content-length"], out ret))
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
        public Stream ContentStream {
            get
            {
                return _ContentStream;
            }
        }
        public static Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        /// <summary>
        /// Returns the request content (body) as string or null if there is no content.
        /// </summary>
        public string ContentString
        {
            get
            {
                return _ContentString;
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
    }
}