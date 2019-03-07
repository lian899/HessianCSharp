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
* Last change: 2005-08-14
* By Andre Voltmann	
* Licence added.
******************************************************************************************************
*/

#region NAMESPACES
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HessianCSharp.Utilities;

#endregion

namespace HessianCSharp.io
{
    /// <summary>
    /// Serializing an object for known object types.
    /// Analog to the JavaSerializer - Class from 
    /// the Hessian implementation
    /// </summary>
    public class CObjectSerializer : AbstractSerializer
    {
        #region CLASS_FIELDS
        /// <summary>
        /// Fields of the objectType
        /// </summary>
        private readonly List<MemberInfo> m_alFields;
        #endregion
        #region CONSTRUCTORS
        /// <summary>
        /// Construktor.
        /// </summary>
        /// <param name="type">Type of the objects, that have to be
        /// serialized</param>
        public CObjectSerializer(Type type)
        {
            BindingFlags bindingAttr = BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.GetField |
                BindingFlags.DeclaredOnly;

            m_alFields = ReflectionUtils.GetFieldsAndProperties(type, bindingAttr);
        }

        #endregion
        #region PUBLIC_METHODS

        ///// <summary>
        ///// Serialiaztion of objects
        ///// </summary>
        ///// <param name="obj">Object to serialize</param>
        ///// <param name="abstractHessianOutput">HessianOutput - Instance</param>
        //public override void WriteObject(object obj, AbstractHessianOutput abstractHessianOutput)
        //{
        //    if (abstractHessianOutput.AddRef(obj))
        //        return;
        //    Type type = obj.GetType();
        //    abstractHessianOutput.WriteMapBegin(type.FullName);
        //    List<MemberInfo> serFields = GetSerializableFieldList();
        //    for (int i = 0; i < serFields.Count; i++)
        //    {
        //        MemberInfo field = serFields[i];
        //        //if (!field.CanWrite) continue;
        //        if (field.GetCustomAttributes(typeof(IgnoreAttribute), true).Length > 0) continue;
        //        abstractHessianOutput.WriteString(field.Name);
        //        abstractHessianOutput.WriteObject(ReflectionUtils.GetMemberValue(field, obj));
        //    }
        //    abstractHessianOutput.WriteMapEnd();
        //}

        /// <summary>
        /// Serialiaztion of objects
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        /// <param name="abstractHessianOutput">HessianOutput - Instance</param>
        public override void WriteObject(object obj, AbstractHessianOutput abstractHessianOutput)
        {
            if (abstractHessianOutput.AddRef(obj))
            {
                return;
            }

            Type cl = obj.GetType();

            int iref = abstractHessianOutput.WriteObjectBegin(cl.FullName);

            if (iref >= 0)
            {
                WriteInstance(obj, abstractHessianOutput);
            }
            else if (iref == -1)
            {
                writeDefinition20(abstractHessianOutput);
                abstractHessianOutput.WriteObjectBegin(cl.FullName);
                WriteInstance(obj, abstractHessianOutput);
            }
            else
            {
                WriteObject10(obj, abstractHessianOutput);
            }
        }

        protected void WriteObject10(Object obj, AbstractHessianOutput abstractHessianOutput)
        {
            List<MemberInfo> serFields = GetSerializableFieldList();
            for (int i = 0; i < serFields.Count; i++)
            {
                MemberInfo field = serFields[i];
                abstractHessianOutput.WriteString(field.Name);
                abstractHessianOutput.WriteObject(ReflectionUtils.GetMemberValue(field, obj));
            }
            abstractHessianOutput.WriteMapEnd();
        }

        private void writeDefinition20(AbstractHessianOutput abstractHessianOutput)
        {
            abstractHessianOutput.WriteClassFieldLength(m_alFields.Count);

            for (int i = 0; i < m_alFields.Count; i++)
            {
                MemberInfo field = m_alFields[i];

                abstractHessianOutput.WriteString(field.Name);
            }
        }

        public void WriteInstance(Object obj, AbstractHessianOutput abstractHessianOutput)
        {
            try
            {
                List<MemberInfo> serFields = GetSerializableFieldList();
                foreach (MemberInfo field in serFields)
                {
                    abstractHessianOutput.WriteObject(ReflectionUtils.GetMemberValue(field, obj));
                }
            }
            catch (IOException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new CHessianException(e.Message + "\n class: "
                                                      + obj.GetType().FullName
                                                      + " (object=" + obj + ")",
                    e);
            }
        }

        public virtual List<MemberInfo> GetSerializableFieldList()
        {
            return m_alFields;
        }


        #endregion
    }
}
