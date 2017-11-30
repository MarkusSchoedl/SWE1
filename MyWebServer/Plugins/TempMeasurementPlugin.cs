using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BIF.SWE1.Interfaces;

namespace MyWebServer
{
    class TempMeasurementPlugin : IPlugin
    {
        public const string _Url = "/temperature";

        public float CanHandle(IRequest req)
        {
            if (req?.Url?.RawUrl == _Url)
            {
                return 1.0f;
            }
            return 0.1f;
        }

        public IResponse Handle(IRequest req)
        {
            throw new NotImplementedException();
        }
    }
}
