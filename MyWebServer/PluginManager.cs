using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Security.Permissions;

using BIF.SWE1.Interfaces;


namespace MyWebServer
{
    class PluginManager : IPluginManager
    {
        #region Parameters
        List<IPlugin> _Plugins = new List<IPlugin>();
        FileSystemWatcher _DirWatcher = new FileSystemWatcher(_ExecutionLocation, "*.dll");

        static string _ExecutionLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        #endregion Parameters

        #region Constructor
        public PluginManager()
        {
            // create directory Plugins
            var lst = Directory.GetFiles(_ExecutionLocation)
                .Where(i => new[] { ".dll", ".exe" }.Contains(Path.GetExtension(i)))
                .SelectMany(i => Assembly.LoadFrom(i).GetTypes())
                .Where(myType => myType.IsClass
                              && !myType.IsAbstract
                              && myType.GetCustomAttributes(true).Any(x => x.GetType() == typeof(AttributePlugins))
                              && myType.GetInterfaces().Any(i => i == typeof(IPlugin)));

            foreach (Type type in lst)
            {
                Add((IPlugin)Activator.CreateInstance(type));
            }

            //FileSystemWatcher
            _DirWatcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                | NotifyFilters.FileName | NotifyFilters.DirectoryName;

            // Add event handlers
            _DirWatcher.Changed += new FileSystemEventHandler(OnChanged);
            _DirWatcher.Created += new FileSystemEventHandler(OnChanged);
            _DirWatcher.Deleted += new FileSystemEventHandler(OnChanged);

            // Begin watching
            _DirWatcher.EnableRaisingEvents = true;
        }
        #endregion Constructor

        #region Events
        // Define the event handlers.
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            // create directory Plugins e.FullPath
            var lst = Assembly.LoadFrom(e.FullPath).GetTypes()
                .Where(myType => myType.IsClass
                              && !myType.IsAbstract
                              && myType.GetInterfaces().Any(i => i == typeof(IPlugin)));

            // A dll was added
            if (e.ChangeType == WatcherChangeTypes.Created)
            {
                Console.WriteLine("\nLoading plugins from: " + e.FullPath);

                foreach (Type type in lst)
                {
                    Add((IPlugin)Activator.CreateInstance(type));
                }
            }
            
            // A dll was deleted
            if (e.ChangeType == WatcherChangeTypes.Deleted)
            {
                Console.WriteLine("\nRemoving plugins from: " + e.FullPath);

                foreach (Type type in lst)
                {
                    _Plugins.Remove(_Plugins.Where(plugin => plugin.GetType().FullName == type.FullName).First());
                    Console.WriteLine("Plugin now removed: " + type.Name);
                }
            }
        }
        #endregion

        #region Methods
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
                Console.WriteLine("Plugin now available: " + plugin.GetType().Name);   
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

        public IPlugin GetHighestPlugin(Request req)
        {
            return _Plugins.Select(i => new
            {
                Value = i.CanHandle(req),
                Plugin = i
            }).OrderBy(i => i.Value).Last().Plugin;
        }
        #endregion Methods
    }
}
