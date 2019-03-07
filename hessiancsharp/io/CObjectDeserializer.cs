/*
***************************************************************************************************** 
* HessianCharp - The .Net implementation of the Hessian Binary Web Service Protocol (www.caucho.com) 
* Copyright (C) 2004-2005  by D. Minich, V. Byelyenkiy, A. Voltmann
* http://www.HessianCSharp.com
*
* This library is free software; you can redistribute it and/or
* modify it under the terms of the GNU Lesser General Public
* License as published by the Free Software Foundation; either
* version 2.1 of the License, or (at your option) any later version.
*
* This library is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
* Lesser General Public License for more details.
*
* You should have received a copy of the GNU Lesser General Public
* License along with this library; if not, write to the Free Software
* Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
* 
* You can find the GNU Lesser General Public here
* http://www.gnu.org/licenses/lgpl.html
* or in the license.txt file in your source directory.
******************************************************************************************************  
* You can find all contact information on http://www.HessianCSharp.com	
******************************************************************************************************
*
*
******************************************************************************************************
* Last change: 2005-12-16
* By Dimitri Minich
* 2005-12-16: GetDeserializableFields added
* 2006-01-03: BUGFIX Non-existing fields by mw
******************************************************************************************************
*/

#region NAMESPACES
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using HessianCSharp.io;
using HessianCSharp.Utilities;

#endregion

namespace HessianCSharp.io
{
    /// <summary>
    /// Deserializing an object for known object types.
    /// Analog to the JavaDeserializer - Class from 
    /// the Hessian implementation
    /// </summary>
    public class CObjectDeserializer : AbstractDeserializer
    {
        #region CLASS_FIELDS

        /// <summary>
        /// Object type
        /// </summary>
        private Type m_type;

        /// <summary>
        /// Hashmap with class fields (&lt;field name&gt;&lt;field info instance&gt;)
        /// </summary>
        private Hashtable m_htFields = new Hashtable();

        private Hashtable _fieldMap = new Hashtable();
        #endregion

        #region CONSTRUCTORS

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">Type of the objects, that have to be
        /// deserialized</param>
        public CObjectDeserializer(Type type)
        {
            this.m_type = type;

            BindingFlags bindingAttr = BindingFlags.Public |
                                       BindingFlags.Instance |
                                       BindingFlags.GetField |
                                       BindingFlags.DeclaredOnly;
            var m_alFields = ReflectionUtils.GetFieldsAndProperties(m_type, bindingAttr);

            foreach (var memberInfo in m_alFields)
            {
                this.m_htFields.Add(memberInfo.Name, memberInfo);
                _fieldMap.Add(memberInfo.Name, new ObjectFieldDeserializer(memberInfo));
            }
        }


        #endregion

        #region PUBLIC_METHODS

        public override Type GetOwnType()
        {
            return m_type;
        }

        /// <summary>
        /// Reads object as map
        /// </summary>
        /// <param name="abstractHessianInput">HessianInput to read from</param>
        /// <returns>Read object or null</returns>
        public override object ReadObject(AbstractHessianInput abstractHessianInput)
        {
            return this.ReadMap(abstractHessianInput);
        }

        //        /// <summary>
        //        /// Reads map
        //        /// </summary>
        //        /// <param name="abstractHessianInput">HessianInput to read from</param>
        //        /// <returns>Read object or null</returns>
        //        public override object ReadMap(AbstractHessianInput abstractHessianInput)
        //        {
        //            object result = null;
        //#if COMPACT_FRAMEWORK
        //            object result = Activator.CreateInstance(this.m_type);				
        //#else
        //            result = Activator.CreateInstance(this.m_type.Assembly.FullName, this.m_type.FullName).Unwrap();
        //            //			object result = Activator.CreateInstance(this.m_type);
        //            //			object result = null;
        //#endif

        //            return ReadMap(abstractHessianInput, result);
        //        }


        //        /// <summary>
        //        /// Reads map
        //        /// </summary>
        //        /// <param name="abstractHessianInput">HessianInput to read from</param>
        //        /// <returns>Read object or null</returns>
        //        public object ReadMap(AbstractHessianInput abstractHessianInput, Object result)
        //        {
        //            int refer = abstractHessianInput.AddRef(result);
        //            var deserFields = GetDeserializableFields();
        //            while (!abstractHessianInput.IsEnd())
        //            {
        //                object objKey = abstractHessianInput.ReadObject();
        //                var field = (MemberInfo)deserFields[objKey];

        //                if (field != null)
        //                {
        //                    //if (ReflectionUtils.CanSetMemberValue(field, false, false)) continue;
        //                    object objFieldValue = abstractHessianInput.ReadObject(ReflectionUtils.GetMemberUnderlyingType(field));
        //                    ReflectionUtils.SetMemberValue(field, result, objFieldValue);
        //                }
        //                else
        //                {
        //                    // mw BUGFIX!!!
        //                    object ignoreme = abstractHessianInput.ReadObject();
        //                }

        //            }
        //            abstractHessianInput.ReadEnd();
        //            return result;
        //        }

        public override Object[] CreateFields(int len)
        {
            return new FieldDeserializer[len];
        }

        public override Object CreateField(String name)
        {
            Object reader = _fieldMap[name];

            if (reader == null)
                return NullFieldDeserializer.DESER;

            return reader;
        }

        public override Object ReadMap(AbstractHessianInput abstractHessianInput)
        {
            try
            {
                Object obj = Instantiate();

                return ReadMap(abstractHessianInput, obj);
            }
            catch (IOException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new CHessianException(m_type.FullName + ":" + e.Message, e);
            }
        }

        public Object ReadMap(AbstractHessianInput abstractHessianInput, Object obj)
        {
            try
            {
                int iref = abstractHessianInput.AddRef(obj);

                while (!abstractHessianInput.IsEnd())
                {
                    Object key = abstractHessianInput.ReadObject();

                    FieldDeserializer deser = (FieldDeserializer)_fieldMap[key];

                    if (deser != null)
                        deser.Deserialize(abstractHessianInput, obj);
                    else
                        abstractHessianInput.ReadObject();
                }

                abstractHessianInput.ReadMapEnd();

                Object resolve = Resolve(abstractHessianInput, obj);

                if (obj != resolve)
                    abstractHessianInput.SetRef(iref, resolve);

                return resolve;
            }
            catch (IOException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new CHessianException(e.Message);
            }
        }

        public override Object ReadObject(AbstractHessianInput abstractHessianInput, Object[] fields)
        {
            try
            {
                Object obj = Instantiate();

                return ReadObject(abstractHessianInput, obj, (FieldDeserializer[])fields);
            }
            catch (IOException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new CHessianException(m_type.FullName + ":" + e.Message, e);
            }
        }

        public Object ReadObject(AbstractHessianInput abstractHessianInput, Object obj, FieldDeserializer[] fields)
        {
            try
            {
                int iref = abstractHessianInput.AddRef(obj);

                foreach (FieldDeserializer reader in fields)
                {
                    reader.Deserialize(abstractHessianInput, obj);
                }

                Object resolve = Resolve(abstractHessianInput, obj);

                if (obj != resolve)
                    abstractHessianInput.SetRef(iref, resolve);

                return resolve;
            }
            catch (IOException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new CHessianException(m_type.FullName + ":" + e.Message, e);
            }
        }

        public override Object ReadObject(AbstractHessianInput abstractHessianInput, String[] fieldNames)
        {
            try
            {
                Object obj = Instantiate();

                return ReadObject(abstractHessianInput, obj, fieldNames);
            }
            catch (IOException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new CHessianException(m_type.FullName + ":" + e.Message, e);
            }
        }

        public Object ReadObject(AbstractHessianInput abstractHessianInput, Object obj, string[] fieldNames)
        {
            try
            {
                int iref = abstractHessianInput.AddRef(obj);

                foreach (var fieldName in fieldNames)
                {
                    FieldDeserializer reader = (FieldDeserializer)_fieldMap[fieldName];

                    if (reader != null)
                        reader.Deserialize(abstractHessianInput, obj);
                    else
                        abstractHessianInput.ReadObject();
                }

                Object resolve = Resolve(abstractHessianInput, obj);

                if (obj != resolve)
                    abstractHessianInput.SetRef(iref, resolve);

                return resolve;
            }
            catch (IOException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new CHessianException(obj.GetType().FullName + ":" + e, e);
            }
        }

        private object Instantiate()
        {
            object result = null;
#if COMPACT_FRAMEWORK
            object result = Activator.CreateInstance(this.m_type);				
#else
            result = Activator.CreateInstance(
                    m_type.Assembly.FullName,
                    m_type.FullName,
                    false,
                    BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                    binder: null,
                    args: null,
                    culture: null,
                    activationAttributes: null)
                .Unwrap();
            return result;
#endif
        }

        protected Object Resolve(AbstractHessianInput abstractHessianInput, Object obj)
        {
            // if there's a readResolve method, call it
            //        try
            //        {
            //            if (_readResolve != null)
            //                return _readResolve.invoke(obj, new Object[0]);
            //        }
            //        catch (InvocationTargetException e)
            //        {
            //            if (e.getCause() instanceof Exception)
            //throw (Exception)e.getCause();
            //else
            //throw e;
            //        }

            return obj;
        }

        public virtual IDictionary GetDeserializableFields()
        {
            return m_htFields;
        }

        #endregion
    }

    public abstract class FieldDeserializer
    {
        public abstract void Deserialize(AbstractHessianInput abstractHessianInput, Object obj);
    }

    public class NullFieldDeserializer : FieldDeserializer
    {
        public static readonly NullFieldDeserializer DESER = new NullFieldDeserializer();

        public override void Deserialize(AbstractHessianInput abstractHessianInput, Object obj)
        {
            abstractHessianInput.ReadObject();
        }
    }

    public class ObjectFieldDeserializer : FieldDeserializer
    {
        private readonly MemberInfo _field;
        public ObjectFieldDeserializer(MemberInfo field)
        {
            _field = field;
        }
        public override void Deserialize(AbstractHessianInput abstractHessianInput, Object obj)
        {
            var value = abstractHessianInput.ReadObject(ReflectionUtils.GetMemberUnderlyingType(_field));
            ReflectionUtils.SetMemberValue(_field, obj, value);
        }
    }
}

