/*
***************************************************************************************************** 
* HessianCharp - The .Net implementation of the Hessian Binary Web Service Protocol (www.caucho.com) 
* Copyright (C) 2004-2005  by D. Minich, V. Byelyenkiy, A. Voltmann
* http://www.hessiancsharp.com
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
* You can find all contact information on http://www.hessiancsharp.com	
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
using Common.Serializer;
using System;
using System.Collections;
using System.Reflection;
#endregion

namespace hessiancsharp.io
{
    /// <summary>
    /// Deserializing an object for known object types.
    /// Analog to the JavaDeserializer - Class from 
    /// the Hessian implementation
    /// </summary>
    public class CJsonDeserializer : AbstractDeserializer
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
        #endregion
        #region CONSTRUCTORS
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">Type of the objects, that have to be
        /// deserialized</param>
        public CJsonDeserializer(Type type)
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

        /// <summary>
        /// Reads map
        /// </summary>
        /// <param name="abstractHessianInput">HessianInput to read from</param>
        /// <returns>Read object or null</returns>
        public override object ReadMap(AbstractHessianInput abstractHessianInput)
        {
            object objKey = abstractHessianInput.ReadObject();
            var result = JsonHelper.DeserializeObject(this.m_type, (string)objKey);
            return result;
        }

        public virtual IDictionary GetDeserializableFields()
        {
            return m_htFields;
        }

        #endregion
    }
}
