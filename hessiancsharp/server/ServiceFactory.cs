using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HessianCSharp.server
{
    public class ServiceFactory
    {
        private static Dictionary<string, Type> _routeDictionary { get; set; }
        private static readonly object objLock = new object();
        private static string _urlSuffix = ".do";
        private static List<string> _namespaces = new List<string>();

        static ServiceFactory()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, e) =>
            {
                lock (objLock)
                {
                    _routeDictionary = null;
                }
                return null;
            };

            AppDomain.CurrentDomain.AssemblyLoad += (sender, e) =>
            {
                lock (objLock)
                {
                    _routeDictionary = null;
                }
            };
        }

        public static string UrlSuffix
        {
            get { return _urlSuffix; }
            set { _urlSuffix = value; }
        }

        public static List<string> Namespaces
        {
            get { return _namespaces; }
        }

        private static Dictionary<string, Type> InitRoutes()
        {
            lock (objLock)
            {
                if (_routeDictionary != null) return _routeDictionary;
                Dictionary<string, Type> routeDictionary = new Dictionary<string, Type>();
                var Assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in Assemblies)
                {
                    if (assembly.FullName.StartsWith("System") || assembly.FullName.StartsWith("Microsoft")) continue;

                    if (assembly.FullName.Contains("Hessian"))
                        Console.Write(assembly);

                    var allInterfaceImps = GetLoadableTypes(assembly)
                        .Where(item => item.GetInterfaces().Any())
                        .Select(item => new
                        {
                            Type = item,
                            Interfaces = item.GetInterfaces()
                        });

                    foreach (var a in allInterfaceImps)
                    {
                        foreach (var face in a.Interfaces)
                        {
                            if (face.FullName == null || face.FullName.StartsWith("System") || face.FullName.StartsWith("Microsoft"))
                                continue;

                            var attr = face.GetCustomAttributes(typeof(HessianRouteAttribute), false).FirstOrDefault();
                            if (attr != null)
                            {
                                var attrUrl = ((HessianRouteAttribute)attr).Uri;
                                if (attrUrl == null)
                                    continue;
                                attrUrl = attrUrl.Trim();
                                if (!attrUrl.StartsWith("/"))
                                    attrUrl = "/" + attrUrl;
                                attrUrl = attrUrl.ToLower();
                                routeDictionary.Add(attrUrl, a.Type);

                            }

                            if (face.Namespace != null && Namespaces.Any(item => face.Namespace.StartsWith(item)))
                            {
                                var url = "/" + (face.Namespace + "." + face.Name).Replace(".", "/") + _urlSuffix;
                                url = url.ToLower();
                                if (routeDictionary.ContainsKey(url))
                                    continue;
                                routeDictionary.Add(url, a.Type);
                            }
                        }
                    }

                }

                _routeDictionary = routeDictionary;
                return _routeDictionary;
            }
        }

        public static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }

        public static object SelectService(string serviceUrl)
        {
            var routeDictionary = InitRoutes();
            var ctxuri = serviceUrl;
            if (ctxuri == null) return null;
            ctxuri = ctxuri.Trim().ToLower();
            if (routeDictionary.ContainsKey(ctxuri))
            {
                return Activator.CreateInstance(routeDictionary[ctxuri]);
            }

            return null;
        }
    }
}
