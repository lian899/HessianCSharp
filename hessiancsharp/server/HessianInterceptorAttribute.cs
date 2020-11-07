using System;
using System.Reflection;

namespace HessianCSharp.server
{
    [AttributeUsage(AttributeTargets.Method)]
    public class HessianInterceptorAttribute : Attribute
    {
        public virtual void OnMethodExecuted(MethodExecutedContext methodExecutedContext)
        {

        }
    }

    public class MethodExecutedContext
    {
        public Type ServiceType { get; internal set; }

        public MethodInfo Method { get; internal set; }

        public ParameterInfo[] ParamInfos { get; internal set; }

        public object[] ParamValues { get; internal set; }

        public object ReturnValue { get; internal set; }

        public Exception Exception { get; internal set; }

    }
}
