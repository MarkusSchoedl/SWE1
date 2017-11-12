using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BIF.SWE1.Interfaces;

namespace MyWebServer
{
    public class Url : IUrl
    {
        #region Parameters
        private string _Path;
        private string _RawUrl;
        private string _Extension;
        private string _FileName;
        private string _Fragment;
        private string[] _Segments;
        private string _DefaultPage = "/index.html";

        private Dictionary<string, string> _Parameter;
        #endregion

        #region Constructor
        public Url()
        {
            // everything empty
            _Path = null;
            _RawUrl = null;
            _Extension = "";
            _FileName = "";
            _Fragment = "";
            _Segments = new String[] { };

            _Parameter = new Dictionary<string, string>();
        }

        public Url(string raw)
        {
            _Parameter = new Dictionary<string, string>();

            if (raw == null)
            {
                return;
            }

            _RawUrl = raw;
            _Segments = _RawUrl?.Split('/').Skip(1).ToArray() ?? new String[] { };

            // get both indizes of ? and #; If not found: set to URL Length 
            int qIndex = _RawUrl.IndexOf('?') == -1 ? _RawUrl.Length : _RawUrl.IndexOf('?');
            int hIndex = _RawUrl.IndexOf('#') == -1 ? _RawUrl.Length : _RawUrl.IndexOf('#');
            // Geth the lower one of both (or the URL Length if none is present)
            int lowerIndex = qIndex < hIndex ? qIndex : hIndex;

            // No ? and # found
            if (lowerIndex == _RawUrl.Length)
            {
                _Path = raw;
                _Fragment = "";
            }
            // One of those have been found
            else
            {
                // Path is from the beginning to the first # or ?
                _Path = raw.Substring(0, lowerIndex);

                // Save everything except the path
                string ending = raw.Substring(lowerIndex + 1, raw.Length - lowerIndex - 1);

                // if the # is before the ?  
                // extract fragment and continue with all after the fragment
                if (lowerIndex == hIndex)
                {
                    string[] temp = ending.Split('#');
                    temp = ending.Split('?');
                    _Fragment = temp[0];

                    ending = ending.Substring(ending.IndexOf('?')+1, ending.Length - ending.IndexOf('?') - 1);
                }

                // Split everything by & (more parameters)
                string[] parameters = ending.Split('&');

                foreach (string parameter in parameters)
                {
                    string currentVar = parameter; // because i cant override "parameter"

                    // if its the last parameter AND the question mark was found first:
                    // Split by # and save fragment if present
                    if (lowerIndex == qIndex && parameter == parameters.Last())
                    {
                        string[] t = parameter.Split('#');

                        if (t.Count() > 1) // # found
                        {
                            currentVar = t[0];
                            _Fragment = t[1];
                        }
                    }

                    // if the questionmark was found first:
                    // split the parameter again and save it 
                    if (lowerIndex == qIndex)
                    {
                        string[] keynvalue = currentVar.Split('=');
                        _Parameter.Add(keynvalue[0], keynvalue[1]);
                    }
                }
            }
            
            if (_Path == "/") // If "/" was requested, switch to the default page
            {
                _Path = _DefaultPage;
            }
        }
        #endregion

        #region SettersGetters
        public IDictionary<string, string> Parameter
        {
            get { return _Parameter; }
        }

        public int ParameterCount
        {
            get { return _Parameter.Count; }
        }

        public string Path
        {
            get { return _Path; }
        }

        public string RawUrl
        {
            get { return _RawUrl; }
        }

        public string Extension
        {
            get { return _Extension; }
        }

        public string FileName
        {
            get { return _FileName; }
        }

        public string Fragment
        {
            get { return _Fragment; }
        }

        public string[] Segments
        {
            get { return _Segments; }
        }
        #endregion
    }
}
