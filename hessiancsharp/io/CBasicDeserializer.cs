/*
***************************************************************************************************** 
* HessianCharp - The .Net implementation of the Hessian Binary Web Service Protocol (www.caucho.com) 
* Copyright (C) 2004-2005  by D. Minich, V. Byelyenkiy, A. Voltmann
* http://www.HessianCSharp.org
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
* You can find all contact information on http://www.HessianCSharp.org
******************************************************************************************************
*
*
******************************************************************************************************
* Last change: 2006-01-03
* By Dimitri Minich
* 2005-12-16: SBYTE_ARRAY added.
* 2006-01-03: BUGFIX Date by mw
******************************************************************************************************
*/

#region NAMESPACES
using System;
using System.Collections;

#endregion
namespace HessianCSharp.io
{
    /// <summary>
    /// Deserializing an object for known object types.
    /// </summary>
    public class CBasicDeserializer : AbstractDeserializer
    {
        #region CLASS_FIELDS
        private int m_intCode;
        #endregion

        //public new Type GetType()
        //{
        //    switch (m_intCode)
        //    {
        //        case BOOLEAN:
        //            return typeof(bool);
        //        case BYTE:
        //            return typeof(byte);
        //        case SHORT:
        //            return typeof(short);
        //        case INTEGER:
        //            return typeof(int);
        //        case LONG:
        //            return typeof(long);
        //        case FLOAT:
        //            return typeof(float);
        //        case DOUBLE:
        //            return typeof(double);
        //        case CHARACTER:
        //            return typeof(char);
        //        case STRING:
        //            return typeof(string);
        //        case DATE:
        //            return typeof(DateTime);
        //        case BOOLEAN_ARRAY:
        //            return typeof(bool[]);
        //        case BYTE_ARRAY:
        //            return typeof(byte[]);
        //        case SHORT_ARRAY:
        //            return typeof(short[]);
        //        case INTEGER_ARRAY:
        //            return typeof(int[]);
        //        case LONG_ARRAY:
        //            return typeof(long[]);
        //        case FLOAT_ARRAY:
        //            return typeof(float[]);
        //        case DOUBLE_ARRAY:
        //            return typeof(double[]);
        //        case CHARACTER_ARRAY:
        //            return typeof(char[]);
        //        case STRING_ARRAY:
        //            return typeof(string[]);
        //        case OBJECT_ARRAY:
        //            return typeof(object[]);
        //        default:
        //            throw new InvalidOperationException();
        //    }
        //}


        #region PUBLIC_METHODS
        /// <summary>
        /// Reads the basic (primitive & Date ) data types
        /// and arrays of them
        /// </summary>
        /// <param name="abstractHessianInput">Hessian Input instance</param>
        /// <exception cref="CHessianException"/>
        /// <returns>Read object</returns>
        public override object ReadObject(AbstractHessianInput abstractHessianInput)
        {
            switch (m_intCode)
            {
                case CSerializationConstants.NULL:
                    {
                        // hessian/3490
                        abstractHessianInput.ReadObject();
                        return null;
                    }
                case BOOLEAN:
                    return abstractHessianInput.ReadBoolean();
                case BYTE:
                    return (byte)abstractHessianInput.ReadInt();
                case SBYTE:
                    return (sbyte)abstractHessianInput.ReadInt();
                case FLOAT:
                    return (float)abstractHessianInput.ReadDouble();
                case SHORT:
                    return (short)abstractHessianInput.ReadInt();
                case INTEGER:
                    return abstractHessianInput.ReadInt();
                case LONG:
                    return abstractHessianInput.ReadLong();
                case DOUBLE:
                    return abstractHessianInput.ReadDouble();
                case STRING:
                    return abstractHessianInput.ReadString();
                case CHARACTER:
                    {
                        //int charResult = abstractHessianInput.ReadInt();
                        //return (char)charResult;
                        //Bei caucho ist hier ein Bug 
                        //TODO:Test
                        string strResult = abstractHessianInput.ReadString();
                        if (strResult == null || strResult.Length == 0)
                            return null;
                        else
                            return strResult[0];

                    }

                case BOOLEAN_ARRAY:
                case SHORT_ARRAY:
                case INTEGER_ARRAY:
                case SBYTE_ARRAY:
                case LONG_ARRAY:
                case FLOAT_ARRAY:
                case DOUBLE_ARRAY:
                case STRING_ARRAY:
                    {
                        int code = abstractHessianInput.ReadListStart();

                        switch (code)
                        {
                            case 'N':
                                return null;

                            case 0x10:
                            case 0x11:
                            case 0x12:
                            case 0x13:
                            case 0x14:
                            case 0x15:
                            case 0x16:
                            case 0x17:
                            case 0x18:
                            case 0x19:
                            case 0x1a:
                            case 0x1b:
                            case 0x1c:
                            case 0x1d:
                            case 0x1e:
                            case 0x1f:
                                int length = code - 0x10;
                                abstractHessianInput.ReadInt();

                                return ReadLengthList(abstractHessianInput, length);

                            default:
                                string type = abstractHessianInput.ReadType();
                                length = abstractHessianInput.ReadLength();
                                return ReadList(abstractHessianInput, length);
                        }
                    }
                case BYTE_ARRAY:
                    return abstractHessianInput.ReadBytes();

                case CHARACTER_ARRAY:
                    {
                        string strResult = abstractHessianInput.ReadString();

                        if (strResult == null)
                            return null;
                        else
                        {
                            int intLength = strResult.Length;
                            char[] arrChars = new char[intLength];
                            arrChars = strResult.ToCharArray();
                            return arrChars;
                        }
                    }
                case DATE:
                    long javaTime = abstractHessianInput.ReadUTCDate();
                    const long timeShift = 62135596800000;
                    DateTime dt = new DateTime((javaTime + timeShift) * 10000, DateTimeKind.Utc);
                    dt = dt.ToLocalTime(); // der Einfachheit halber
                    return dt;

                default:
                    throw new CHessianException("not supperted type for deserialization");
            }
        }


        /// <summary>
        /// Reads arrays
        /// </summary>
        /// <param name="abstractHessianInput">Hessian Input instance</param>
        /// <param name="intLength">Array length</param>
        /// <exception cref="CHessianException"/>
        /// <returns>Read object</returns>
        public override object ReadList(AbstractHessianInput abstractHessianInput, int intLength)
        {

            switch (m_intCode)
            {
                case INTEGER_ARRAY:
                    {
                        if (intLength >= 0)
                        {
                            int[] arrData = new int[intLength];

                            abstractHessianInput.AddRef(arrData);

                            for (int i = 0; i < arrData.Length; i++)
                                arrData[i] = abstractHessianInput.ReadInt();

                            abstractHessianInput.ReadEnd();

                            return arrData;
                        }
                        else
                        {
                            ArrayList arrayList = new ArrayList();

                            while (!abstractHessianInput.IsEnd())
                                arrayList.Add(abstractHessianInput.ReadInt());

                            abstractHessianInput.ReadEnd();

                            int[] arrData = new int[arrayList.Count];
                            for (int i = 0; i < arrData.Length; i++)
                                arrData[i] = (int)arrayList[i];


                            abstractHessianInput.AddRef(arrData);

                            return arrData;
                        }
                    }

                case SBYTE_ARRAY:
                    {
                        if (intLength >= 0)
                        {
                            sbyte[] arrData = new sbyte[intLength];

                            abstractHessianInput.AddRef(arrData);

                            for (int i = 0; i < arrData.Length; i++)
                                arrData[i] = (sbyte)abstractHessianInput.ReadInt();

                            abstractHessianInput.ReadEnd();

                            return arrData;
                        }
                        else
                        {
                            ArrayList arrayList = new ArrayList();

                            while (!abstractHessianInput.IsEnd())
                                arrayList.Add(abstractHessianInput.ReadInt());

                            abstractHessianInput.ReadEnd();

                            sbyte[] arrData = new sbyte[arrayList.Count];
                            for (int i = 0; i < arrData.Length; i++)
                                arrData[i] = (sbyte)arrayList[i];


                            abstractHessianInput.AddRef(arrData);

                            return arrData;
                        }
                    }

                case STRING_ARRAY:
                    {
                        if (intLength >= 0)
                        {
                            string[] arrData = new String[intLength];
                            abstractHessianInput.AddRef(arrData);

                            for (int i = 0; i < arrData.Length; i++)
                                arrData[i] = abstractHessianInput.ReadString();

                            abstractHessianInput.ReadEnd();

                            return arrData;
                        }
                        else
                        {
                            ArrayList arrayList = new ArrayList();

                            while (!abstractHessianInput.IsEnd())
                                arrayList.Add(abstractHessianInput.ReadString());

                            abstractHessianInput.ReadEnd();

                            string[] arrData = new String[arrayList.Count];
                            abstractHessianInput.AddRef(arrData);
                            for (int i = 0; i < arrData.Length; i++)
                                arrData[i] = (string)arrayList[i];


                            return arrData;
                        }
                    }


                case BOOLEAN_ARRAY:
                    {

                        if (intLength >= 0)
                        {
                            bool[] arrData = new bool[intLength];

                            abstractHessianInput.AddRef(arrData);

                            for (int i = 0; i < arrData.Length; i++)
                                arrData[i] = abstractHessianInput.ReadBoolean();

                            abstractHessianInput.ReadEnd();

                            return arrData;
                        }
                        else
                        {
                            ArrayList arrayList = new ArrayList();

                            while (!abstractHessianInput.IsEnd())
                                arrayList.Add(abstractHessianInput.ReadBoolean());

                            abstractHessianInput.ReadEnd();

                            bool[] arrData = new bool[arrayList.Count];

                            abstractHessianInput.AddRef(arrData);

                            for (int i = 0; i < arrData.Length; i++)
                                arrData[i] = (bool)arrayList[i];
                            return arrData;
                        }
                    }

                case SHORT_ARRAY:
                    {
                        if (intLength >= 0)
                        {
                            short[] arrData = new short[intLength];

                            abstractHessianInput.AddRef(arrData);

                            for (int i = 0; i < arrData.Length; i++)
                                arrData[i] = (short)abstractHessianInput.ReadInt();

                            abstractHessianInput.ReadEnd();

                            return arrData;
                        }
                        else
                        {
                            ArrayList arrayList = new ArrayList();

                            while (!abstractHessianInput.IsEnd())
                                arrayList.Add((short)abstractHessianInput.ReadInt());

                            abstractHessianInput.ReadEnd();

                            short[] arrData = new short[arrayList.Count];
                            for (int i = 0; i < arrData.Length; i++)
                                arrData[i] = (short)arrayList[i];

                            abstractHessianInput.AddRef(arrData);

                            return arrData;
                        }
                    }



                case LONG_ARRAY:
                    {
                        if (intLength >= 0)
                        {
                            long[] arrData = new long[intLength];

                            abstractHessianInput.AddRef(arrData);

                            for (int i = 0; i < arrData.Length; i++)
                                arrData[i] = abstractHessianInput.ReadLong();

                            abstractHessianInput.ReadEnd();

                            return arrData;
                        }
                        else
                        {
                            ArrayList arrayList = new ArrayList();

                            while (!abstractHessianInput.IsEnd())
                                arrayList.Add(abstractHessianInput.ReadLong());

                            abstractHessianInput.ReadEnd();

                            long[] arrData = new long[arrayList.Count];
                            for (int i = 0; i < arrData.Length; i++)
                                arrData[i] = (long)arrayList[i];

                            abstractHessianInput.AddRef(arrData);

                            return arrData;
                        }
                    }

                case FLOAT_ARRAY:
                    {
                        if (intLength >= 0)
                        {
                            float[] arrData = new float[intLength];
                            abstractHessianInput.AddRef(arrData);

                            for (int i = 0; i < arrData.Length; i++)
                                arrData[i] = (float)(abstractHessianInput.ReadDouble());

                            abstractHessianInput.ReadEnd();

                            return arrData;
                        }
                        else
                        {
                            ArrayList arrayList = new ArrayList();

                            while (!abstractHessianInput.IsEnd())
                                arrayList.Add(abstractHessianInput.ReadDouble());

                            abstractHessianInput.ReadEnd();

                            float[] arrData = new float[arrayList.Count];
                            for (int i = 0; i < arrData.Length; i++)
                                arrData[i] = (float)arrayList[i];

                            abstractHessianInput.AddRef(arrData);

                            return arrData;
                        }
                    }

                case DOUBLE_ARRAY:
                    {
                        if (intLength >= 0)
                        {
                            double[] arrData = new double[intLength];
                            abstractHessianInput.AddRef(arrData);

                            for (int i = 0; i < arrData.Length; i++)
                                arrData[i] = abstractHessianInput.ReadDouble();

                            abstractHessianInput.ReadEnd();

                            return arrData;
                        }
                        else
                        {
                            ArrayList arrayList = new ArrayList();

                            while (!abstractHessianInput.IsEnd())
                                arrayList.Add(abstractHessianInput.ReadDouble());

                            abstractHessianInput.ReadEnd();

                            double[] data = new double[arrayList.Count];
                            abstractHessianInput.AddRef(data);
                            for (int i = 0; i < data.Length; i++)
                                data[i] = (double)arrayList[i];


                            return data;
                        }
                    }



                case OBJECT_ARRAY:
                    {
                        if (intLength >= 0)
                        {
                            object[] arrData = new Object[intLength];
                            abstractHessianInput.AddRef(arrData);

                            for (int i = 0; i < arrData.Length; i++)
                                arrData[i] = abstractHessianInput.ReadObject();

                            abstractHessianInput.ReadEnd();

                            return arrData;
                        }
                        else
                        {
                            ArrayList arrayList = new ArrayList();

                            abstractHessianInput.AddRef(arrayList); // XXX: potential issues here

                            while (!abstractHessianInput.IsEnd())
                                arrayList.Add(abstractHessianInput.ReadObject());

                            abstractHessianInput.ReadEnd();

                            object[] arrData = new Object[arrayList.Count];
                            for (int i = 0; i < arrData.Length; i++)
                                arrData[i] = arrayList[i];

                            return arrData;
                        }
                    }

                default:
                    throw new CHessianException("not supperted type for deserialization");
            }
        }

        public override object ReadLengthList(AbstractHessianInput abstractHessianInput, int length)
        {
            switch (m_intCode)
            {
                case BOOLEAN_ARRAY:
                    {
                        bool[] data = new bool[length];

                        abstractHessianInput.AddRef(data);

                        for (int i = 0; i < data.Length; i++)
                            data[i] = abstractHessianInput.ReadBoolean();

                        return data;
                    }

                case SHORT_ARRAY:
                    {
                        short[] data = new short[length];

                        abstractHessianInput.AddRef(data);

                        for (int i = 0; i < data.Length; i++)
                            data[i] = (short)abstractHessianInput.ReadInt();

                        return data;
                    }

                case INTEGER_ARRAY:
                    {
                        int[] data = new int[length];

                        abstractHessianInput.AddRef(data);

                        for (int i = 0; i < data.Length; i++)
                            data[i] = abstractHessianInput.ReadInt();

                        return data;
                    }

                case LONG_ARRAY:
                    {
                        long[] data = new long[length];

                        abstractHessianInput.AddRef(data);

                        for (int i = 0; i < data.Length; i++)
                            data[i] = abstractHessianInput.ReadLong();

                        return data;
                    }

                case FLOAT_ARRAY:
                    {
                        float[] data = new float[length];
                        abstractHessianInput.AddRef(data);

                        for (int i = 0; i < data.Length; i++)
                            data[i] = (float)abstractHessianInput.ReadDouble();

                        return data;
                    }

                case DOUBLE_ARRAY:
                    {
                        double[] data = new double[length];
                        abstractHessianInput.AddRef(data);

                        for (int i = 0; i < data.Length; i++)
                            data[i] = abstractHessianInput.ReadDouble();

                        return data;
                    }

                case STRING_ARRAY:
                    {
                        string[] data = new string[length];
                        abstractHessianInput.AddRef(data);

                        for (int i = 0; i < data.Length; i++)
                            data[i] = abstractHessianInput.ReadString();

                        return data;
                    }

                case OBJECT_ARRAY:
                    {
                        object[] data = new object[length];
                        abstractHessianInput.AddRef(data);

                        for (int i = 0; i < data.Length; i++)
                            data[i] = abstractHessianInput.ReadObject();

                        return data;
                    }

                default:
                    throw new InvalidOperationException(this.ToString());
            }
        }

        #endregion

        #region CONSTRUCTORS
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="code">Code that identifies this
        /// instance as Deserializer</param>
        public CBasicDeserializer(int code)
        {
            this.m_intCode = code;
        }

        #endregion

        public override string ToString()
        {
            return GetType().Name + "[" + m_intCode + "]";
        }
    }
}
