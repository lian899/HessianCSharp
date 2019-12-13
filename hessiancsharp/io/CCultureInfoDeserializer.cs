using System;
using System.IO;

namespace HessianCSharp.io
{
    /// <summary>
    /// Summary description for CCultureInfoDeserializer.
    /// </summary>
    public class CCultureInfoDeserializer : AbstractDeserializer
    {
        public override Type GetOwnType()
        {
            return typeof(System.Globalization.CultureInfo);
        }

        public override object ReadMap(AbstractHessianInput abstractHessianInput)
        {
            string value = null;

            while (!abstractHessianInput.IsEnd())
            {
                string key = abstractHessianInput.ReadString();
                if (key.Equals("value"))
                    value = abstractHessianInput.ReadString();
                else
                    abstractHessianInput.ReadObject();
            }

            abstractHessianInput.ReadMapEnd();

            if (value == null)
                return null;

            object obj = new System.Globalization.CultureInfo(value);

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
            String[] fieldNames = (string[])fields;

            String value = null;

            for (int i = 0; i < fieldNames.Length; i++)
            {
                if ("value".Equals(fieldNames[i]))
                    value = abstractHessianInput.ReadString();
                else
                    abstractHessianInput.ReadObject();
            }
            if (value == null)
                return null;

            object obj = new System.Globalization.CultureInfo(value);

            abstractHessianInput.AddRef(obj);

            return obj;
        }
    }
}
