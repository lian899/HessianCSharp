using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HessianCSharp.io
{
    public class HessianInputFactory
    {


        public HeaderType ReadHeader(Stream inputStream)
        {
            int code = inputStream.ReadByte();

            int major = inputStream.ReadByte();
            int minor = inputStream.ReadByte();

            switch (code)
            {
                case -1:
                    throw new IOException("Unexpected end of file for Hessian message");

                case 'c':
                    if (major >= 2)
                        return HeaderType.CALL_1_REPLY_2;
                    else
                        return HeaderType.CALL_1_REPLY_1;
                case 'r':
                    return HeaderType.REPLY_1;

                case 'H':
                    return HeaderType.HESSIAN_2;

                default:
                    throw new IOException((char)code + " 0x" + string.Format("{0:x2}", code) + " is an unknown Hessian message code.");
            }
        }
    }

    public enum HeaderType
    {
        CALL_1_REPLY_1,
        CALL_1_REPLY_2,
        HESSIAN_2,
        REPLY_1,
        REPLY_2
    }

    public static class HeaderTypeUtil
    {

        public static bool IsCall1(this HeaderType type)
        {
            switch (type)
            {
                case HeaderType.CALL_1_REPLY_1:
                case HeaderType.CALL_1_REPLY_2:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsCall2(this HeaderType type)
        {
            switch (type)
            {
                case HeaderType.HESSIAN_2:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsReply1(this HeaderType type)
        {
            switch (type)
            {
                case HeaderType.CALL_1_REPLY_1:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsReply2(this HeaderType type)
        {
            switch (type)
            {
                case HeaderType.CALL_1_REPLY_2:
                case HeaderType.HESSIAN_2:
                    return true;
                default:
                    return false;
            }
        }
    }
}
