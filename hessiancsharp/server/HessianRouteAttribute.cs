using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HessianCSharp.server
{
    public class HessianRouteAttribute : Attribute
    {
        public HessianRouteAttribute()
        {
        }
        public HessianRouteAttribute(string uri)
        {
            this.Uri = uri;
        }
        public string Uri { get; set; }
    }
}
