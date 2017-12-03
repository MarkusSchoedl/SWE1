using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BIF.SWE1.Interfaces;
using System.Threading;

namespace MyWebServer
{
    class TempMeasurementPlugin : IPlugin
    {
        #region Properties
        public const string _Url = "/temperature";
        #endregion Properties

        #region Constructor
        public TempMeasurementPlugin()
        {
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                ReadSensor();
            }).Start();
        }
        #endregion Constructor

        #region Methods
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

        private void ReadSensor()
        {
            float oldTemp = 20f;
            while (true)
            {
                Random ran = new Random();
                float x = oldTemp + (float)ran.NextDouble() * 2;

                Console.WriteLine("Adding Temperature " + x + "°C");

                Thread.Sleep(5000);
            }
        }
        #endregion Methodes
    }
}
