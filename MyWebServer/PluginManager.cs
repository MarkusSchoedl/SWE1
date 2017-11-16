using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BIF.SWE1.Interfaces;


namespace MyWebServer
{
    class PluginManager : IPluginManager
    {
        List<IPlugin> _Plugins = new List<IPlugin>();


        public PluginManager()
        {
            //foreach(Type t in typeof(PluginManager).Assembly.GetTypes())
            //{
            //    Add(t.Name);
            //}



            Add("MyWebServer.ToLowerPlugin");
            Add("MyWebServer.NavigationPlugin");
            Add("MyWebServer.TempMeasurementPlugin");
            Add("MyWebServer.StaticFilesPlugin");
        }


        /// <summary>
        /// Returns a list of all plugins. Never returns null.
        /// </summary>
        public IEnumerable<IPlugin> Plugins
        {
            get
            {
                return _Plugins;
            }
        }

        /// <summary>
        /// Adds a new plugin. If the plugin was already added, nothing will happen.
        /// </summary>
        /// <param name="plugin"></param>
        public void Add(IPlugin plugin)
        {
            //Check if the class name of the object was not loaded yet
            if (!_Plugins.Exists(x => x.GetType().Name == plugin.GetType().Name))
            {
                _Plugins.Add(plugin);
            }
        }

        /// <summary>
        /// Adds a new plugin by type name. If the plugin was already added, nothing will happen.
        /// Throws an exeption, when the type cannot be resoled or the type does not implement IPlugin.
        /// </summary>
        /// <param name="plugin"></param>
        public void Add(string plugin)
        {
            IPlugin pluginObj = (IPlugin)System.Reflection.Assembly.GetExecutingAssembly().CreateInstance(plugin);
            if (pluginObj == null)
            {
                throw new CouldntFindPluginNameException();
            }
            Add(pluginObj);
        }

        /// <summary>
        /// Clears all plugins
        /// </summary>
        public void Clear()
        {
            _Plugins.Clear();
        }
    }
}
