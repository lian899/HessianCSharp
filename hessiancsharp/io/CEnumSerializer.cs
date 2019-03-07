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
#endregion

namespace HessianCSharp.io
{
    /// <summary>
    /// Serializing of DateTime - Instances.
    /// </summary>
    public class CEnumSerializer : AbstractSerializer
    {

        #region PUBLIC_METHODS

        /// <summary>
        /// Writes Instance of the Enum class
        /// </summary>
        /// <param name="objData">Instance of the enum class</param>
        /// <param name="abstractHessianOutput">HessianOutput - Stream</param>
        public override void WriteObject(object objData, AbstractHessianOutput abstractHessianOutput)
        {
            if (abstractHessianOutput.AddRef(objData))
                return;
            if (objData == null)
                abstractHessianOutput.WriteNull();
            else
            {
                int iref = abstractHessianOutput.WriteObjectBegin(objData.GetType().FullName);

                if (iref < -1)
                {
                    abstractHessianOutput.WriteString("name");
                    abstractHessianOutput.WriteString(objData.ToString());
                    abstractHessianOutput.WriteMapEnd();
                }
                else
                {
                    if (iref == -1)
                    {
                        abstractHessianOutput.WriteClassFieldLength(1);
                        abstractHessianOutput.WriteString("name");
                        abstractHessianOutput.WriteObjectBegin(objData.GetType().FullName);
                    }

                    abstractHessianOutput.WriteString(objData.ToString());
                }
            }
        }
        #endregion
    }
}
