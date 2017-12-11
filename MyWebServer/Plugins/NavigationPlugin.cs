using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BIF.SWE1.Interfaces;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Xml.Linq;
using System.Web;

namespace MyWebServer
{
    /// <summary>
    /// <para>Reads one or more OSM Maps and can give you all cities for a certain street.</para>
    /// </summary>
    [AttributePlugins]
    class NavigationPlugin : IPlugin
    {
        #region Fields
        private Dictionary<string, List<string>> _WholeMap;

        private MyMutex _ObjMutex = new MyMutex();
        
        /// <summary>
        /// The exact url which you can call the navigation plugin on.
        /// You might add "?test=1" after the end of it so you get a testresult without parsing an OSM Map.
        /// </summary>
        public const string _Url = "/navigation";

        private static string _OsmSubDir = "Maps";
        private static string _OsmPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), _OsmSubDir);
        #endregion Fields

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationPlugin"/> class
        /// </summary>
        public NavigationPlugin()
        {
            Directory.CreateDirectory(_OsmPath);
        }
        #endregion Constructor

        #region Methods
        /// <summary>
        /// Returns how much the plugin wants to handle the request.
        /// </summary>
        /// <param name="req">The request the Browser/Client sent to us.</param>
        /// <returns>A floating point number greater than 0 and smaller or equal to 1.</returns>
        public float CanHandle(IRequest req)
        {
            if (req.Url.Path == _Url)
            {
                return 1.0f;
            }
            return 0.1f;
        }

        /// <summary>
        /// Handles a request and generates an appropiate response. <para/>
        /// Important: The street to search for has to be set in the content using: <code>"street=" + [TOSEARCH]</code>
        /// </summary>
        /// <param name="req">The request the Browser/Client sent to us.</param>
        /// <returns>A response which just needs to be sent.</returns>
        public IResponse Handle(IRequest req)
        {
            var rsp = new Response(req);
            rsp.StatusCode = 200;

            string searchStreet = string.Empty;
            if (req.Headers.ContainsKey("content-length"))
            {
                if (req.ContentLength > 0)
                {
                    if (req.ContentLength <= 7)
                    {
                        rsp.SetContent("Bitte geben Sie eine Anfrage ein");
                        return rsp;
                    }

                    searchStreet = HttpUtility.UrlDecode(req.ContentString.Substring(7));
                }
            }

            // For testing the plugin without database connection
            if (req.Url.ParameterCount > 0 && req.Url.Parameter.Contains(new KeyValuePair<string, string>("test", "1")))
            {
                rsp.StatusCode = 200;
                rsp.SetContent("<div><ul><li>This is Test-Data</li><li>3 Orte gefunden</li><li>Wien</li><li>Klosterneuburg</li><li>Wiener Neustadt</li></ul></div>");
                rsp.ContentType = "text/xml";
                return rsp;
            }

            //Check if able to Lock!
            if (_ObjMutex.TryWait())
            {
                //Requested an update
                if (req.Url.ParameterCount == 1 && req.Url.Parameter.ContainsKey("Update")
                && req.Url.Parameter["Update"] == "true")
                {
                    List<string> res = ReadWholeFile(saveAll: true);
                    _ObjMutex.Release();

                    XElement xmlEles = new XElement("div", "Erfolgreiches Update");
                    if (res != null && res.Count > 0)
                    {
                        xmlEles = new XElement("div", res[0]);
                    }
                    rsp.SetContent(xmlEles.ToString());
                    return rsp;
                }

                // Requested a Read
                List<string> result = new List<string>();

                if (_WholeMap != null)
                {
                    if (_WholeMap.ContainsKey(searchStreet.ToLower()))
                    {
                        result = _WholeMap[searchStreet.ToLower()];
                    }
                }
                else
                {
                    result = ReadWholeFile(searchStreet: searchStreet);
                }

                XElement xmlElements = new XElement("div", result.Count + " Orte gefunden");
                if (result.Count > 0)
                {
                    xmlElements.Add(new XElement("ul", result.Select(i => new XElement("li", i))));
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
        private List<string> ReadWholeFile(bool saveAll = false, string searchStreet = null)
        {
            if (saveAll)
            {
                if (_WholeMap == null)
                {
                    _WholeMap = new Dictionary<string, List<string>>();
                }
                _WholeMap.Clear();
            }

            List<string> result = new List<string>();

            try
            {
                foreach (string file in Directory.GetFiles(_OsmPath).Where(x => x.EndsWith(".osm")))
                {
                    using (var fs = File.OpenRead(file))
                    using (var xml = new System.Xml.XmlTextReader(fs))
                    {
                        while (xml.Read())
                        {
                            if (xml.NodeType == System.Xml.XmlNodeType.Element && xml.Name == "osm")
                            {
                                ReadOsm(xml, saveAll, searchStreet).ForEach(x => result.Add(x));
                            }
                        }
                    }
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("The OpenStreetMap has not been found (" + _OsmPath + ")");
            }

            if (Directory.GetFiles(_OsmPath).Count() == 0)
            {
                return new List<string>(new[] { "No OSM Files found." });
            }

            return result;
        }

        private List<string> ReadOsm(System.Xml.XmlTextReader xml, bool saveAll, string searchStreet)
        {
            List<string> cities = new List<string>();

            using (var osm = xml.ReadSubtree())
            {
                while (osm.Read())
                {
                    if (osm.NodeType == System.Xml.XmlNodeType.Element
                    && (osm.Name == "node" || osm.Name == "way"))
                    {
                        ReadAnyOsmElement(osm, saveAll, ref cities, searchStreet);
                    }
                }
            }

            return cities;
        }

        private List<string> ReadAnyOsmElement(System.Xml.XmlReader osm, bool saveAll, ref List<string> cities, string searchStreet)
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
                if (!string.IsNullOrEmpty(street) && !string.IsNullOrEmpty(city))
                {
                    if (saveAll)
                    {

                        if (_WholeMap.ContainsKey(street.ToLower()) && !_WholeMap[street.ToLower()].Contains(city))
                        {
                            _WholeMap[street.ToLower()].Add(city);
                        }
                        else if (!_WholeMap.ContainsKey(street.ToLower()))
                        {
                            _WholeMap.Add(street.ToLower(), new List<string>(new[] { city }));
                        }
                    }
                    else
                    {
                        if (!cities.Contains(city) && street.ToLower() == searchStreet.ToLower())
                        {
                            cities.Add(city);
                        }
                    }
                }
            }

            return cities;
        }
        #endregion XML
    }
}
