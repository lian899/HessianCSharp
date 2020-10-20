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
using System.Reflection;
#endregion


namespace HessianCSharp.io
{
    /// <summary>
    /// Date - Deserialization.
    /// </summary>
    public class CEnumDeserializer : AbstractDeserializer
    {
        #region CLASS_FIELDS
        /// <summary>
        /// Type of map
        /// </summary>
        private Type m_type = null;
        #endregion

        #region CONSTRUCTORS
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">Type of map</param>
        public CEnumDeserializer(Type type)
        {
            this.m_type = type;
        }
        #endregion

        #region PUBLIC_METHODS

        public override Type GetOwnType()
        {
            return m_type;
        }

        public override object ReadMap(AbstractHessianInput abstractHessianInput)
        {
            String name = null;

            while (!abstractHessianInput.IsEnd())
            {
                string key = abstractHessianInput.ReadString();

                if (key.Equals("name"))
                    name = abstractHessianInput.ReadString();
                else
                    abstractHessianInput.ReadObject();
            }

            abstractHessianInput.ReadMapEnd();

            if (name == null)
                return null;

            Object obj = Enum.Parse(m_type, name);

            abstractHessianInput.AddRef(obj);

            return obj;
        }

        /// <summary>
        /// Reads date
        /// </summary>
        /// <param name="abstractHessianInput">HessianInput - Instance</param>
        /// <param name="fields"></param>
        public override object ReadObject(AbstractHessianInput abstractHessianInput, object[] fields)
        {
            String[] fieldNames = (String[])fields;
            String name = null;

            for (int i = 0; i < fieldNames.Length; i++)
            {
                if ("name".Equals(fieldNames[i]))
                    name = abstractHessianInput.ReadString();
                else
                    abstractHessianInput.ReadObject();
            }

            if (name == null)
                return null;

            Object obj = Enum.Parse(m_type, name);

            abstractHessianInput.AddRef(obj);

            return obj;
        }
        #endregion

    }
}
