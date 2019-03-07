using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HessianCSharp.io
{
    /**
     * Serializing an object. 
     */
    public interface ISerializer
    {
        void WriteObject(Object obj, AbstractHessianOutput hessianOutput);
    }
}
