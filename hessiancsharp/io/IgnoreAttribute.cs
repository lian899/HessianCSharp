using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HessianCSharp.io
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class IgnoreAttribute : Attribute
    {
    }
}
