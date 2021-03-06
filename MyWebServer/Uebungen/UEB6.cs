﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BIF.SWE1.Interfaces;
using MyWebServer;
using MyWebServer.Plugins;

namespace Uebungen
{
    public class UEB6 : IUEB6
    {
        private const string _ToLowerUrl = ToLowerPlugin._Url;
        private const string _TemeratureUrl = TempMeasurementPlugin._Url;
        private const string _TemeratureRestUrl = TempMeasurementPlugin._RestUrl;
        private const string _NaviUrl = NavigationPlugin._Url;

        public void HelloWorld()
        {
        }

        public IPluginManager GetPluginManager()
        {
            return new PluginManager();
        }

        public IRequest GetRequest(System.IO.Stream network)
        {
            return new Request(network);
        }

        public string GetNaviUrl()
        {
            return _NaviUrl + "?test=1";
        }

        public IPlugin GetNavigationPlugin()
        {
            return new NavigationPlugin();
        }

        public IPlugin GetTemperaturePlugin()
        {
            return new TempMeasurementPlugin();
        }

        public string GetTemperatureRestUrl(DateTime from, DateTime until)
        {
            return _TemeratureRestUrl + until.ToShortDateString().Replace(".", "-") + "?test=1";
        }

        public string GetTemperatureUrl(DateTime from, DateTime until)
        {
            return _TemeratureUrl + from.ToShortDateString().Replace(".", "-") + "/" + until.ToShortDateString().Replace(".", "-") + "?test=1";
        }

        public IPlugin GetToLowerPlugin()
        {
            return new ToLowerPlugin();
        }

        public string GetToLowerUrl()
        {
            return _ToLowerUrl;
        }
    }
}
