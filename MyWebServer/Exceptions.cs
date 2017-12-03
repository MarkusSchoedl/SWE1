using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyWebServer
{
    class HTTPStatusCodeNotSetException : Exception
    {
        public HTTPStatusCodeNotSetException()
        {
        }

        public HTTPStatusCodeNotSetException(string message)
        : base(message)
        {
        }

        public HTTPStatusCodeNotSetException(string message, Exception inner)
        : base(message, inner)
        {
        }
    }

    class RequestStreamNullOrEmptyException : Exception
    {
        public RequestStreamNullOrEmptyException()
        {
        }

        public RequestStreamNullOrEmptyException(string message)
        : base(message)
        {
        }

        public RequestStreamNullOrEmptyException(string message, Exception inner)
        : base(message, inner)
        {
        }
    }

    class NetworkNotWriteableException : Exception
    {
        public NetworkNotWriteableException()
        {
        }

        public NetworkNotWriteableException(string message)
        : base(message)
        {
        }

        public NetworkNotWriteableException(string message, Exception inner)
        : base(message, inner)
        {
        }
    }
    class ContentNotSetException : Exception
    {
        public ContentNotSetException()
        {
        }

        public ContentNotSetException(string message)
        : base(message)
        {
        }

        public ContentNotSetException(string message, Exception inner)
        : base(message, inner)
        {
        }
    }
    class RequestNotSetException : Exception
    {
        public RequestNotSetException()
        {
        }

        public RequestNotSetException(string message)
        : base(message)
        {
        }

        public RequestNotSetException(string message, Exception inner)
        : base(message, inner)
        {
        }
    }
    class NoContentSetException : Exception
    {
        public NoContentSetException()
        {
        }

        public NoContentSetException(string message)
        : base(message)
        {
        }

        public NoContentSetException(string message, Exception inner)
        : base(message, inner)
        {
        }
    }
    class CouldntConvertContentLengthException : Exception
    {
        public CouldntConvertContentLengthException()
        {
        }

        public CouldntConvertContentLengthException(string message)
        : base(message)
        {
        }

        public CouldntConvertContentLengthException(string message, Exception inner)
        : base(message, inner)
        {
        }
    }
    class CouldntFindPluginNameException : Exception
    {
        public CouldntFindPluginNameException()
        {
        }

        public CouldntFindPluginNameException(string message)
        : base(message)
        {
        }

        public CouldntFindPluginNameException(string message, Exception inner)
        : base(message, inner)
        {
        }
    }
    class SqlServerNotConnectedException : Exception
    {
        public SqlServerNotConnectedException()
        {
        }

        public SqlServerNotConnectedException(string message)
        : base(message)
        {
        }

        public SqlServerNotConnectedException(string message, Exception inner)
        : base(message, inner)
        {
        }
    }
}
