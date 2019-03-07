using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HessianCSharp.io
{
    /// <summary>
    /// Factory for creating HessianInput and HessianOutput streams.
    /// </summary>
    public class HessianFactory
    {
        /**
         * Creates a new Hessian 2.0 deserializer.
         */
        public CHessian2Input CreateHessian2Input(Stream inputStream)
        {
            return new CHessian2Input(inputStream);
        }

        /**
         * Creates a new Hessian 1.0 deserializer.
         */
        public CHessianInput CreateHessianInput(Stream inputStream)
        {
            return new CHessianInput(inputStream);
        }

        /**
         * Creates a new Hessian 2.0 serializer.
         */
        public CHessian2Output CreateHessian2Output(Stream outStream)
        {
            return new CHessian2Output(outStream);
        }

        /**
         * Creates a new Hessian 1.0 serializer.
         */
        public CHessianOutput CreateHessianOutput(Stream outStream)
        {
            return new CHessianOutput(outStream);
        }

    }
}
