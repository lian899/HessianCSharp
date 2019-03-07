using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HessianCSharp.server
{
    public class ServiceFactory
    {
        private static List<HessianRoute> _routeList { get; set; }
        private static readonly object objLock = new object();

        private static List<HessianRoute> InitServiceBase()
        {
            lock (objLock)
            {
                if (_routeList != null) return _routeList;
                List<HessianRoute> list = new List<HessianRoute>();
                var Assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in Assemblies)
                {
                    try
                    {
                        var types = GetLoadableTypes(assembly)
                            .Select(item => new
                            {
                                Type = item,
                                Interface = item
                                .GetInterfaces()
                                .FirstOrDefault(p => p.GetCustomAttributes(typeof(HessianRouteAttribute), false).Any())
                            });

                        var routes = types
                             .Where(item => item.Interface != null)
                             .Select(t => new HessianRoute
                             {
                                 Uri = ((HessianRouteAttribute)t.Interface.GetCustomAttributes(typeof(HessianRouteAttribute), false).First()).Uri,
                                 Type = t.Type
                             });

                        list.AddRange(routes);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }

                _routeList = list;
                return _routeList;
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
            _routeList = InitServiceBase();
            var ctxuri = serviceUrl;
            foreach (var route in _routeList)
            {
                if (ctxuri == null || route.Uri == null) continue;
                ctxuri = ctxuri.ToLower();
                if (route.Uri != null) route.Uri = route.Uri.ToLower();
                if (ctxuri == route.Uri)
                    return Activator.CreateInstance(route.Type);
            }
            return null;
        }
    }
    public class HessianRoute
    {
        public string Uri { get; set; }
        public Type Type { get; set; }
    }
}
