using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BIF.SWE1.Interfaces;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Xml.Linq;

namespace MyWebServer
{
    class NavigationPlugin : IPlugin
    {
        #region Properties
        private Dictionary<string, List<string>> _WholeMap = new Dictionary<string, List<string>>();

        private MyMutex _ObjMutex = new MyMutex();

        public const string _Url = "/navigation";

        private static string _OsmName = "wien.osm";
        private static string _OsmPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), _OsmName);
        #endregion Properties

        #region Methods
        public float CanHandle(IRequest req)
        {
            if (req.Url.RawUrl.StartsWith(_Url))
            {
                return 1.0f;
            }
            return 0.1f;
        }

        public IResponse Handle(IRequest req)
        {
            var rsp = new Response(req);
            rsp.StatusCode = 200;

            string searchStreet = string.Empty;
            if (req.Headers.ContainsKey("content-length"))
            {
                if (req.ContentLength <= 7)
                {
                    rsp.SetContent("Bitte geben Sie eine Anfrage ein");
                    return rsp;
                }

                searchStreet = req.ContentString.Substring(7);
            }

            //Check if able to Lock!
            if (_ObjMutex.TryWait())
            {
                //Requested an update
                if (req.Url.ParameterCount == 1 && req.Url.Parameter.ContainsKey("Update")
                && req.Url.Parameter["Update"] == "true")
                {
                    ReadWholeFile(saveAll: true);
                    _ObjMutex.Release();
                    XElement xmlEles = new XElement("div", "Erfolgreiches Update");
                    rsp.SetContent(xmlEles.ToString());
                    return rsp;
                }

                // Requested a Read
                List<string> result = new List<string>();

                if (_WholeMap != null)
                {
                    if (_WholeMap.ContainsKey(searchStreet))
                    {
                        result = _WholeMap[searchStreet];
                    }
                }
                else
                {
                    result = ReadWholeFile();
                }

                XElement xmlElements = new XElement("div", result.Count + " Orte gefunden");
                if (result.Count > 0)
                {
                    xmlElements.AddAfterSelf(new XElement("ul", result.Select(i => new XElement("li", i))));
                }
                rsp.SetContent(xmlElements.ToString());

                _ObjMutex.Release();

                return rsp;
            }

            // Couldnt lock
            else
            {
                rsp.SetContent("Das NavigationPlugin wird zurzeit verwendet. Bitte versuchen Sie es später noch einmal.");
                return rsp;
            }
        }
        #endregion Methods

        #region XML
        private List<string> ReadWholeFile(bool saveAll = false)
        {
            if (saveAll)
            {
                _WholeMap.Clear();
            }

            try
            {
                using (var fs = File.OpenRead(_OsmPath))
                using (var xml = new System.Xml.XmlTextReader(fs))
                {
                    while (xml.Read())
                    {
                        if (xml.NodeType == System.Xml.XmlNodeType.Element && xml.Name == "osm")
                        {
                            return ReadOsm(xml, saveAll);
                        }
                    }
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("The OpenStreetMap has not been found (" + _OsmPath + ")");
            }
            return null;
        }

        private List<string> ReadOsm(System.Xml.XmlTextReader xml, bool saveAll)
        {
            List<string> cities = new List<string>();

            using (var osm = xml.ReadSubtree())
            {
                while (osm.Read())
                {
                    if (osm.NodeType == System.Xml.XmlNodeType.Element
                    && (osm.Name == "node" || osm.Name == "way"))
                    {
                        ReadAnyOsmElement(osm, saveAll, ref cities);
                    }
                }
            }

            return cities;
        }

        private List<string> ReadAnyOsmElement(System.Xml.XmlReader osm, bool saveAll, ref List<string> cities)
        {
            using (var element = osm.ReadSubtree())
            {
                string street = null, city = null, postcode = null;
                while (element.Read())
                {
                    if (element.NodeType == System.Xml.XmlNodeType.Element
                    && element.Name == "tag")
                    {
                        switch (element.GetAttribute("k"))
                        {
                            case "addr:city":
                                city = element.GetAttribute("v");
                                break;
                            case "addr:postcode":
                                postcode = element.GetAttribute("v");
                                break;
                            case "addr:street":
                                street = element.GetAttribute("v");
                                break;
                        }
                    }
                }

                //if (postcode != null && city != null)
                //    city = city + ", " + postcode;

                if (saveAll)
                {
                    if (!string.IsNullOrEmpty(street) && !string.IsNullOrEmpty(city))
                    {
                        if (_WholeMap.ContainsKey(street) && !_WholeMap[street].Contains(city))
                        {
                            _WholeMap[street].Add(city);
                        }
                        else if (!_WholeMap.ContainsKey(street))
                        {
                            _WholeMap.Add(street, new List<string>(new[] { city }));
                        }
                    }
                }
                else
                {
                    if (!cities.Contains(city))
                    {
                        cities.Add(city);
                    }
                }
            }

            return cities;
        }
        #endregion XML
    }
}
