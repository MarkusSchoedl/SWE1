using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime;

namespace MyWebServer.MyExceptions
{
    /// <summary>
    /// This Exception is thrown wenn no Status Code was set.
    /// </summary>
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

    /// <summary>
    /// This Exception is thrown if we received an request with an NullOrEmpty stream.
    /// </summary>
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

    /// <summary>
    /// This Exception is thrown if the network is not writeable (Usually in Response).
    /// </summary>
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

    /// <summary>
    /// This Exception is thrown if the content of the Response couldnt be set.
    /// </summary>
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

    /// <summary>
    /// This Exeption is thrown if the Request was not set at all.
    /// </summary>
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

    /// <summary>
    /// This Exception is thrown if no Content was set in the Request.
    /// </summary>
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

    /// <summary>
    /// This Exception is thrown if the received Content-Length couldnt be converted to Int.
    /// </summary>
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

    /// <summary>
    /// This Exception is thrown if someone wanted to set Add a plugin in the <seealso cref="PluginManager"/> which is not available.
    /// </summary>
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

    /// <summary>
    /// This Exception is thrown if we are not connected to the MSSQL Server but want to set off commands.
    /// </summary>
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
