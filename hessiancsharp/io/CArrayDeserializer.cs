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
#endregion

namespace HessianCSharp.io
{
    /// <summary>
    /// Deserializing of arrays
    /// </summary>
    public class CArrayDeserializer : AbstractDeserializer
    {
        #region CLASS_FIELDS
        /// <summary>
        /// Type of the array objects
        /// </summary>
        private Type m_componentType;

        private Type m_type;

        #endregion

        #region PROPERTIES
        /// <summary>
        /// Type property
        /// </summary>
        public Type Type
        {
            get
            {
                return m_type;
            }
        }
        #endregion

        #region CONSTRUCTORS
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="componentABSTRACTDeserializer">Deserializer for the instances in the array</param>
        public CArrayDeserializer(AbstractDeserializer componentABSTRACTDeserializer)
        {
            if (componentABSTRACTDeserializer != null)
                m_componentType = componentABSTRACTDeserializer.GetOwnType();

            if (m_componentType != null)
            {
                try
                {
                    m_type = m_componentType.MakeArrayType();
                }
                catch (Exception)
                {
                }
            }

            if (m_type == null)
                m_type = typeof(object[]);
        }
        #endregion

        #region PUBLIC_METHODS
        /// <summary>
        /// Returns the type of the reader
        /// </summary>
        /// <returns>type of the reader</returns>
        public override Type GetOwnType()
        {
            return Type;
        }

        /// <summary>
        /// Reads the array.
        /// </summary>
        /// <param name="abstractHessianInput">HessianInput</param>
        /// <param name="intLength">Length of data</param>
        /// <returns>Array data</returns>
        public override Object ReadList(AbstractHessianInput abstractHessianInput, int intLength)
        {
            if (intLength >= 0)
            {
                Array arrResult = createArray(intLength);

                abstractHessianInput.AddRef(arrResult);

                if (m_componentType != null)
                {
                    for (int i = 0; i < arrResult.Length; i++)
                        arrResult.SetValue(abstractHessianInput.ReadObject(m_componentType), i); //arrResult[i] = abstractHessianInput.ReadObject(m_componentType);
                }
                else
                {
                    for (int i = 0; i < arrResult.Length; i++)
                        arrResult.SetValue(abstractHessianInput.ReadObject(), i); //arrResult[i] = abstractHessianInput.ReadObject();
                }

                abstractHessianInput.ReadListEnd();
                return arrResult;
            }
            else
            {
                ArrayList colList = new ArrayList();
                abstractHessianInput.AddRef(colList);
                if (m_componentType != null)
                {
                    while (!abstractHessianInput.IsEnd())
                        colList.Add(abstractHessianInput.ReadObject(m_componentType));
                }
                else
                {
                    while (!abstractHessianInput.IsEnd())
                        colList.Add(abstractHessianInput.ReadObject());
                }

                abstractHessianInput.ReadListEnd();

                Array arrResult = createArray(colList.Count);
                for (int i = 0; i < arrResult.Length; i++)
                    arrResult.SetValue(colList[i], i); //arrResult[i] = colList[i];
                return arrResult;
            }
        }

        /// <summary>
        /// Reads the array
        /// </summary>
        public override Object ReadLengthList(AbstractHessianInput abstractHessianInput, int length)
        {
            Array data = createArray(length);

            abstractHessianInput.AddRef(data);

            if (m_componentType != null)
            {
                for (int i = 0; i < data.Length; i++)
                    data.SetValue(abstractHessianInput.ReadObject(m_componentType), i); //data[i] = abstractHessianInput.ReadObject(m_componentType);
            }
            else
            {
                for (int i = 0; i < data.Length; i++)
                    data.SetValue(abstractHessianInput.ReadObject(), i); //data[i] = abstractHessianInput.ReadObject();
            }

            return data;
        }

        /// <summary>
        /// Overriden toString - Method
        /// </summary>
        /// <returns>String description of the used Instances</returns>
        public override String ToString()
        {
            return "ArrayDeserializer[" + m_componentType + "]";
        }

        /// <summary>
        /// Reads object
        /// </summary>
        /// <param name="abstractHessianInput">Instance of AbstractHessianInput</param>
        /// <returns>Object that was read</returns>
        public override object ReadObject(AbstractHessianInput abstractHessianInput)
        {
            int intCode = abstractHessianInput.ReadListStart();
            switch (intCode)
            {
                case CHessianProtocolConstants.PROT_NULL:
                    return null;
                case CHessianProtocolConstants.PROT_REF_TYPE:
                    return abstractHessianInput.ReadRef();
            }
            int intLength = abstractHessianInput.ReadLength();
            return ReadList(abstractHessianInput, intLength);
        }

        #endregion
        #region PROTECTED_METHODS

        /// <summary>
        /// Creates new array with given length
        /// </summary>
        /// <param name="intLength">Length of the array</param>
        /// <returns>Array-Instance</returns>
        protected internal virtual Array createArray(int intLength)
        {
            if (m_componentType != null)
                return Array.CreateInstance(m_componentType, intLength);
            else
                return new Object[intLength];
        }
        #endregion
    }
}
