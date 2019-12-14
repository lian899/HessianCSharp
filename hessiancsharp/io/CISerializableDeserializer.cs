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
using System.Runtime.Serialization;

#endregion

namespace HessianCSharp.io
{
    /// <summary>
    /// Deserializing an object for known object types.
    /// Analog to the CISerializableDeserializer - Class from 
    /// the Hessian implementation
    /// </summary>
    public class CISerializableDeserializer : AbstractDeserializer
    {
        #region CLASS_FIELDS

        /// <summary>
        /// Object type
        /// </summary>
        private Type m_type;

        #endregion

        #region CONSTRUCTORS

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">Type of the objects, that have to be
        /// deserialized</param>
        public CISerializableDeserializer(Type type)
        {
            this.m_type = type;
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
            return new string[len];
        }

        public override Object CreateField(String name)
        {
            return name;
        }

        public override Object ReadMap(AbstractHessianInput abstractHessianInput)
        {
            try
            {
                return ReadMap(abstractHessianInput, null);
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
                SerializationInfo serializationInfo = new SerializationInfo(m_type, new FormatterConverter());

                while (!abstractHessianInput.IsEnd())
                {
                    Object key = abstractHessianInput.ReadObject();
                    serializationInfo.AddValue((string)key, abstractHessianInput.ReadObject());
                }

                abstractHessianInput.ReadMapEnd();

                obj = Instantiate(serializationInfo);

                int iref = abstractHessianInput.AddRef(obj);

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

                return ReadObject(abstractHessianInput, null, (string[])fields);
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

        public override Object ReadObject(AbstractHessianInput abstractHessianInput, string[] fieldNames)
        {
            try
            {
                return ReadObject(abstractHessianInput, null, fieldNames);
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

        public Object ReadObject(AbstractHessianInput abstractHessianInput, object obj, string[] fieldNames)
        {
            try
            {
                SerializationInfo serializationInfo = new SerializationInfo(m_type, new FormatterConverter());

                foreach (var fieldName in fieldNames)
                {
                    serializationInfo.AddValue(fieldName, abstractHessianInput.ReadObject());
                }

                obj = Instantiate(serializationInfo);

                int iref = abstractHessianInput.AddRef(obj);

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

        private object Instantiate(SerializationInfo serializationInfo)
        {
            object result = null;
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            ConstructorInfo constructorInfo = m_type.GetConstructor(bindingFlags, null, new[] { typeof(SerializationInfo), typeof(StreamingContext) }, null);
            result = constructorInfo.Invoke(bindingFlags, null, new object[] { serializationInfo, new StreamingContext() }, System.Globalization.CultureInfo.InvariantCulture);
            return result;
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

        #endregion
    }
}

