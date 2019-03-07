using HessianCSharp.io;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HessianCSharp
{
    public class HessianStream : IDisposable
    {
        public AbstractHessianInput Input { get; set; }
        public Stream InputStream { get; set; }

        public void Dispose()
        {
            if (InputStream != null)
                InputStream.Close();
        }
    }

    public delegate void OutputAction(AbstractHessianOutput _output);
}
