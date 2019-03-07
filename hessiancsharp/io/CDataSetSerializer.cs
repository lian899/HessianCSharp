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
using System.Data;
using System.Reflection;
#endregion

namespace HessianCSharp.io
{
    /// <summary>
    /// Serializing an object for known object types.
    /// Analog to the JavaSerializer - Class from 
    /// the Hessian implementation
    /// </summary>
    public class CDataSetSerializer : AbstractSerializer
    {
        #region PUBLIC_METHODS

        /// <summary>
        /// Serialiaztion of objects
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        /// <param name="abstractHessianOutput">HessianOutput - Instance</param>
        public override void WriteObject(object obj, AbstractHessianOutput abstractHessianOutput)
        {
            if (abstractHessianOutput.AddRef(obj))
                return;

            var ds = obj as DataSet;
            abstractHessianOutput.WriteMapBegin(obj.GetType().FullName);
            foreach (DataTable dt in ds.Tables)
            {
                abstractHessianOutput.WriteMapBegin(null);
                abstractHessianOutput.WriteString(dt.TableName);
                foreach (DataColumn column in dt.Columns)
                {
                    abstractHessianOutput.WriteString(column.ColumnName);
                    abstractHessianOutput.WriteString(column.DataType.FullName);
                }
                abstractHessianOutput.WriteMapEnd();

                abstractHessianOutput.WriteMapBegin(null);
                foreach (DataRow dr in dt.Rows)
                {
                    abstractHessianOutput.WriteMapBegin(null);
                    foreach (object value in dr.ItemArray)
                    {
                        abstractHessianOutput.WriteObject(value);
                    }
                    abstractHessianOutput.WriteMapEnd();
                }
                abstractHessianOutput.WriteMapEnd();
            }
            abstractHessianOutput.WriteMapEnd();

        }
        #endregion
    }
}
