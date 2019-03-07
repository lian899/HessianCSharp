using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HessianCSharp.Utilities;

namespace HessianCSharp.io
{
    /// <summary>
    /// Summary description for CExceptionDeserializer.
    /// </summary>
    public class CExceptionDeserializer : CObjectDeserializer
    {
        private Hashtable m_deserFields = new Hashtable();
        private Type m_type = null;
        public CExceptionDeserializer(Type type) : base(type)
        {
            List<MemberInfo> fieldList = CExceptionSerializer.GetSerializableFields();
            foreach (MemberInfo fieldInfo in fieldList)
            {
                if (m_deserFields.ContainsKey(fieldInfo.Name))
                    m_deserFields[fieldInfo.Name] = fieldInfo;
                else
                    m_deserFields.Add(fieldInfo.Name, fieldInfo);

            }
            m_type = type;
        }

        public override IDictionary GetDeserializableFields()
        {
            return m_deserFields;
        }

        public override object ReadMap(AbstractHessianInput abstractHessianInput)
        {
            Hashtable fieldValueMap = new Hashtable();
            string _message = null;
            Exception _innerException = null;
            while (!abstractHessianInput.IsEnd())
            {
                object objKey = abstractHessianInput.ReadObject();
                if (objKey != null)
                {
                    var deserFields = GetDeserializableFields();
                    var field = (MemberInfo)deserFields[objKey];
                    // try to convert a Java Exception in a .NET exception
                    if (objKey.ToString() == "_message" || objKey.ToString() == "detailMessage")
                    {
                        if (field != null)
                            _message = abstractHessianInput.ReadObject(ReflectionUtils.GetMemberUnderlyingType(field)) as string;
                        else
                            _message = abstractHessianInput.ReadObject().ToString();
                    }
                    else if (objKey.ToString() == "_innerException" || objKey.ToString() == "cause")
                    {
                        try
                        {
                            if (field != null)
                                _innerException = abstractHessianInput.ReadObject(ReflectionUtils.GetMemberUnderlyingType(field)) as Exception;
                            else
                                _innerException = abstractHessianInput.ReadObject(typeof(Exception)) as Exception;
                        }
                        catch (Exception e)
                        {
                            // als Cause ist bei Java gerne mal eine zirkuläre Referenz auf die Exception selbst
                            // angegeben. Das klappt nicht, weil die Referenz noch nicht registriert ist,
                            // weil der Typ noch nicht klar ist (s.u.)
                        }
                    }
                    else
                    {
                        if (field != null)
                        {
                            object objFieldValue = abstractHessianInput.ReadObject(ReflectionUtils.GetMemberUnderlyingType(field));
                            fieldValueMap.Add(field, objFieldValue);
                        }
                        else
                            // ignore (z. B. Exception Stacktrace "stackTrace" von Java)
                            abstractHessianInput.ReadObject();
                        //field.SetValue(result, objFieldValue);
                    }
                }

            }
            abstractHessianInput.ReadEnd();

            object result = null;
            try
            {
#if COMPACT_FRAMEWORK
            	//CF TODO: tbd
#else
                try
                {
                    result = Activator.CreateInstance(this.m_type, new object[2] { _message, _innerException });
                }
                catch (Exception)
                {
                    try
                    {
                        result = Activator.CreateInstance(this.m_type, new object[1] { _innerException });
                    }
                    catch (Exception)
                    {
                        try
                        {
                            result = Activator.CreateInstance(this.m_type, new object[1] { _message });
                        }
                        catch (Exception)
                        {
                            result = Activator.CreateInstance(this.m_type);
                        }
                    }
                }
#endif

            }
            catch (Exception)
            {
                result = new Exception(_message, _innerException);
            }
            foreach (DictionaryEntry entry in fieldValueMap)
            {
                MemberInfo fieldInfo = (MemberInfo)entry.Key;
                object value = entry.Value;
                try { ReflectionUtils.SetMemberValue(fieldInfo, result, value); } catch (Exception) { }
            }

            // besser spät als gar nicht.
            int refer = abstractHessianInput.AddRef(result);


            return result;
        }
    }
}
