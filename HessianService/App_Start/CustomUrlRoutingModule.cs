using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace HessianService
{
    public class CustomUrlRoutingModule : UrlRoutingModule
    {
        public override void PostResolveRequestCache(HttpContextBase context)
        {
            if (context.Request.Path.ToLower().EndsWith(".do"))
            {
                return;
            }
            base.PostResolveRequestCache(context);
        }
    }
}