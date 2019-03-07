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
using System.Data;
using System.Reflection;
#endregion

namespace HessianCSharp.io
{
    /// <summary>
    /// Deserializing an object for known object types.
    /// Analog to the JavaDeserializer - Class from 
    /// the Hessian implementation
    /// </summary>
    public class CDataTableDeserializer : AbstractDeserializer
    {

        #region PUBLIC_METHODS

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
            var dt = new DataTable();
            int refer = abstractHessianInput.AddRef(dt);
            dt.TableName = abstractHessianInput.ReadString();
            while (!abstractHessianInput.IsEnd())
            {
                var columnName = abstractHessianInput.ReadString();
                var typeFullName = abstractHessianInput.ReadString();
                var type = Type.GetType(typeFullName);
                if (type == null) type = FindType(typeFullName);
                dt.Columns.Add(new DataColumn(columnName, type));
            }
            abstractHessianInput.ReadEnd();

            if (abstractHessianInput.ReadMapStart() == CHessian2Constants.BC_MAP)
                abstractHessianInput.ReadType();
            while (!abstractHessianInput.IsEnd())
            {
                ArrayList objects = new ArrayList();
                if (abstractHessianInput.ReadMapStart() == CHessian2Constants.BC_MAP)
                    abstractHessianInput.ReadType();
                while (!abstractHessianInput.IsEnd())
                {
                    var obj = abstractHessianInput.ReadObject();
                    objects.Add(obj);
                }
                abstractHessianInput.ReadEnd();
                var row = dt.NewRow();
                row.ItemArray = objects.ToArray();
                dt.Rows.Add(row);
            }
            abstractHessianInput.ReadEnd();
            return dt;
        }

        public Type FindType(string strType)
        {
            Assembly[] ass = AppDomain.CurrentDomain.GetAssemblies();
            Type t = null;
            foreach (Assembly a in ass)
            {
                try
                {
                    t = a.GetType(strType);
                    if (t != null)
                    {
                        break;
                    }
                }
                catch
                {
                    continue;
                }
            }
            return t;
        }

        #endregion
    }
}
