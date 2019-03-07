using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HessianCSharp.io
{

    /// <summary>
    /// * Deserializing an object. Custom deserializers should extend
    /// * from AbstractDeserializer to avoid issues with signature
    /// * changes.
    /// </summary>
    public interface IDeserializer
    {
        Type GetOwnType();

        bool IsReadResolve();

        object ReadObject(AbstractHessianInput hessianInput);

        Object ReadList(AbstractHessianInput hessianInput, int length);

        Object ReadLengthList(AbstractHessianInput hessianInput, int length);

        Object ReadMap(AbstractHessianInput hessianInput);

        /**
         * Creates an empty array for the deserializers field
         * entries.
         * 
         * @param len number of fields to be read
         * @return empty array of the proper field type.
         */
        Object[] CreateFields(int len);

        /**
         * Returns the deserializer's field reader for the given name.
         * 
         * @param name the field name
         * @return the deserializer's internal field reader
         */
        Object CreateField(String name);

        /**
         * Reads the object from the input stream, given the field
         * definition.
         * 
         * @param in the input stream
         * @param fields the deserializer's own field marshal
         * @return the new object
         * @throws IOException
         */
        Object ReadObject(AbstractHessianInput hessianInput, Object[] fields);

        Object ReadObject(AbstractHessianInput hessianInput, String[] fieldNames);
    }
}
