#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using HessianCSharp.io;

namespace HessianCSharp.Utilities
{
    internal static class ReflectionUtils
    {
        public static List<MemberInfo> GetFieldsAndProperties(Type type, BindingFlags bindingAttr)
        {
            List<MemberInfo> targetMembers = new List<MemberInfo>();

            for (; type != null; type = type.BaseType)
            {
                FilterMembers(targetMembers, GetFields(type, bindingAttr));
                FilterMembers(targetMembers, GetProperties(type, bindingAttr));
            }

            targetMembers = targetMembers
                .Where(p => p.GetCustomAttributes(typeof(IgnoreAttribute), true).Length == 0)
                .ToList();

            return targetMembers;
        }

        private static IEnumerable<MemberInfo> GetFields(Type targetType, BindingFlags bindingAttr)
        {
            List<MemberInfo> fieldInfos = new List<MemberInfo>(targetType.GetFields(bindingAttr));
            return fieldInfos;
        }

        private static IEnumerable<MemberInfo> GetProperties(Type targetType, BindingFlags bindingAttr)
        {
            List<PropertyInfo> propertyInfos = new List<PropertyInfo>(targetType.GetProperties(bindingAttr));
            return propertyInfos;
        }

        private static void FilterMembers(List<MemberInfo> targetMembers, IEnumerable<MemberInfo> memberInfos)
        {
            foreach (var memberInfo in memberInfos)
            {
                if (targetMembers.All(p => p.Name != memberInfo.Name))
                {
                    if (CanSetMemberValue(memberInfo, false, false))
                        targetMembers.Add(memberInfo);
                }
            }
        }

        public static bool CanSetMemberValue(MemberInfo member, bool nonPublic, bool canSetReadOnly)
        {
            switch (member.MemberType())
            {
                case MemberTypes.Field:
                    FieldInfo fieldInfo = (FieldInfo)member;

                    if (fieldInfo.IsInitOnly && !canSetReadOnly)
                        return false;
                    if (nonPublic)
                        return true;
                    else if (fieldInfo.IsPublic)
                        return true;
                    return false;
                case MemberTypes.Property:
                    PropertyInfo propertyInfo = (PropertyInfo)member;

                    if (!propertyInfo.CanWrite)
                        return false;
                    if (nonPublic)
                        return true;
                    return (propertyInfo.GetSetMethod(nonPublic) != null);
                default:
                    return false;
            }
        }

        private static MemberTypes MemberType(this MemberInfo memberInfo)
        {
#if !(NETFX_CORE || PORTABLE)
            return memberInfo.MemberType;
#else
              if (memberInfo is PropertyInfo)
                return MemberTypes.Property;
              else if (memberInfo is FieldInfo)
                return MemberTypes.Field;
              else if (memberInfo is EventInfo)
                return MemberTypes.Event;
              else if (memberInfo is MethodInfo)
                return MemberTypes.Method;
              else
                return MemberTypes.Other;
#endif
        }

        public static object GetMemberValue(MemberInfo member, object target)
        {
            switch (member.MemberType())
            {
                case MemberTypes.Field:
                    return ((FieldInfo)member).GetValue(target);
                case MemberTypes.Property:
                    try
                    {
                        return ((PropertyInfo)member).GetValue(target, null);
                    }
                    catch (TargetParameterCountException e)
                    {
                        throw new ArgumentException(string.Format("MemberInfo '{0}' has index parameters", member.Name), e);
                    }
                default:
                    throw new ArgumentException(string.Format("MemberInfo '{0}' is not of type FieldInfo or PropertyInfo", member.Name), "member");
            }
        }

        public static void SetMemberValue(MemberInfo member, object target, object value)
        {
            switch (member.MemberType())
            {
                case MemberTypes.Field:
                    ((FieldInfo)member).SetValue(target, value);
                    break;
                case MemberTypes.Property:
                    ((PropertyInfo)member).SetValue(target, value, null);
                    break;
                default:
                    throw new ArgumentException(string.Format("MemberInfo '{0}' must be of type FieldInfo or PropertyInfo", member.Name), "member");
            }
        }

        public static Type GetMemberUnderlyingType(MemberInfo member)
        {
            switch (member.MemberType())
            {
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
                case MemberTypes.Event:
                    return ((EventInfo)member).EventHandlerType;
                default:
                    throw new ArgumentException("MemberInfo must be of type FieldInfo, PropertyInfo or EventInfo", "member");
            }
        }


    }
}
