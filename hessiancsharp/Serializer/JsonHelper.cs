using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Serializer
{
    public class JsonHelper
    {
        public static T DeserializeObject<T>(string szJson)
        {
            return JsonConvert.DeserializeObject<T>(szJson);
        }
        public static object DeserializeObject(Type type, string szJson)
        {
            return JsonConvert.DeserializeObject(szJson, type);
        }

        public static string SerializeObject(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}