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
            //Add all plugins
            foreach (Type type in typeof(PluginManager).Assembly.GetTypes().Where(myType => myType.IsClass && !myType.IsAbstract 
                    && myType.GetInterfaces().Any(i => i == typeof(IPlugin))))
            {
                _Plugins.Add((IPlugin)Activator.CreateInstance(type));
            }
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
            IPlugin pluginObj = (IPlugin)Activator.CreateInstance(Type.GetType(plugin));

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
