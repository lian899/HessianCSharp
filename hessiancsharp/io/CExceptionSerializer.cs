using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HessianCSharp.Utilities;

namespace HessianCSharp.io
{
    public class CExceptionSerializer : CObjectSerializer
    {
        private readonly List<MemberInfo> m_serFields;
        public CExceptionSerializer() : base(typeof(Exception))
        {
            m_serFields = GetSerializableFields();
        }

        public static List<MemberInfo> GetSerializableFields()
        {
            Type type = typeof(Exception);
            BindingFlags bindingAttr = BindingFlags.Public |
                                       BindingFlags.Instance |
                                       BindingFlags.GetField |
                                       BindingFlags.DeclaredOnly;

            var serFields = ReflectionUtils.GetFieldsAndProperties(type, bindingAttr);
            return serFields;
        }

        public override List<MemberInfo> GetSerializableFieldList()
        {
            return m_serFields;
        }

    }
}
