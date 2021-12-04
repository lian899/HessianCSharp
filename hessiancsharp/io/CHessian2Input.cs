using HessianCSharp.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace HessianCSharp.io
{
    public class CHessian2Input : AbstractHessianInput
    {

        private static int SIZE = 1024;
        protected List<Object> _refs = new List<Object>();

        // the underlying input stream
        private Stream _is;
        private byte[] _buffer = new byte[SIZE];

        // a peek character
        private int _offset;
        private int _length;

        protected List<ObjectDefinition> _classDefs = new List<ObjectDefinition>();
        protected List<String> _types = new List<String>();
        // the method for a call
        private String _method;

        private StringBuilder _sbuf = new StringBuilder();

        // true if this is the last chunk
        private bool _isLastChunk;
        // the chunk length
        private int _chunkLength;

        private Exception _replyFault;

        private static MemberInfo _detailMessageField;

        /// <summary>
        /// Creates a new Hessian output stream, initialized with an
        /// underlying output stream.
        /// </summary>
        /// <param name="inputStream">the underlying output stream.</param>
        public CHessian2Input(Stream inputStream)
        {
            Init(inputStream);
        }

        public void Init(Stream inputStream)
        {

            _is = inputStream;
            m_serializerFactory = new CSerializerFactory();
            Reset();
        }

        public override int ReadCall()
        {
            int tag = Read();

            if (tag != 'C')
                throw Error("expected hessian call ('C') at " + CodeName(tag));

            return 0;
        }

        /// <summary>
        /// * Starts reading the envelope
        /// *
        /// * <code>
        /// * E major minor
        /// * </pre>
        /// </summary>
        public int ReadEnvelope()
        {
            int tag = Read();
            int version = 0;

            if (tag == 'H')
            {
                int major = Read();
                int minor = Read();

                version = (major << 16) + minor;

                tag = Read();
            }

            if (tag != 'E')
                throw Error("expected hessian Envelope ('E') at " + CodeName(tag));

            return version;
        }

        /// <summary>
        /// * Completes reading the envelope
        /// *
        /// * <p>A successful completion will have a single value:
        /// *
        /// * <code>
        /// * Z
        /// * </pre>
        /// </summary>
        public void CompleteEnvelope()
        {
            int tag = Read();

            if (tag != 'Z')
                Error("expected end of envelope at " + CodeName(tag));
        }

        /// <summary>
        /// * Reads a header, returning null if there are no headers.
        /// *
        /// * <code>
        /// * H b16 b8 value
        /// * </pre>
        /// </summary>
        /// <returns></returns>
        public override string ReadHeader()
        {
            return null;
        }


        /// <summary>
        /// * Starts reading a packet
        /// *
        /// * <code>
        /// * p major minor
        /// * </pre>
        /// </summary>
        /// <returns></returns>
        public int StartMessage()
        {
            int tag = Read();

            if (tag == 'p')
            {
            }
            else if (tag == 'P')
            {
            }
            else
                throw Error("expected Hessian message ('p') at " + CodeName(tag));

            int major = Read();
            int minor = Read();

            return (major << 16) + minor;
        }

        /// <summary>
        /// * Completes reading the message
        /// *
        /// * <p>A successful completion will have a single value:
        /// *
        /// * <code>
        /// * z
        /// * </pre>
        /// </summary>
        public void CompleteMessage()
        {
            int tag = Read();

            if (tag != 'Z')
                Error("expected end of message at " + CodeName(tag));
        }

        /// <summary>
        /// * Starts reading the call
        /// *
        /// * <p>A successful completion will have a single value:
        /// *
        /// * <code>
        /// * string
        /// * </pre>
        /// </summary>
        /// <returns></returns>
        public override string ReadMethod()
        {
            _method = ReadString();

            return _method;
        }

        /// <summary>
        /// * Returns the number of method arguments
        /// *
        /// * <code>
        /// * int
        /// * </pre>
        /// </summary>
        public override int ReadMethodArgLength()
        {
            return ReadInt();
        }

        public override int AddRef(object objReference)
        {
            if (_refs == null)
                _refs = new List<object>();

            _refs.Add(objReference);

            return _refs.Count - 1;
        }

        /// <summary>
        /// Adds a list/map reference.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="obj"></param>
        public override void SetRef(int i, object obj)
        {
            _refs[i] = obj;
        }

        /// <summary>
        /// Resets the references for streaming.
        /// </summary>
        public void ResetReferences()
        {
            _refs.Clear();
        }

        public override void CompleteCall()
        {
        }

        public override void CompleteReply()
        {
        }

        /// <summary>
        /// * Completes reading the call
        /// *
        /// * <p>A successful completion will have a single value:
        /// *
        /// * <code>
        /// * z
        /// * </pre>
        /// </summary>
        public void CompleteValueReply()
        {
            int tag = Read();

            if (tag != 'Z')
                Error("expected end of reply at " + CodeName(tag));
        }

        public override bool IsEnd()
        {
            int code;

            if (_offset < _length)
                code = (_buffer[_offset] & 0xff);
            else
            {
                code = Read();

                if (code >= 0)
                    _offset--;
            }

            return (code < 0 || code == 'Z');
        }

        /// <summary>
        /// * Reads a boolean
        /// *
        /// * <code>
        /// * T
        /// * F
        /// * </pre>
        /// </summary>
        public override bool ReadBoolean()
        {
            int tag = _offset < _length ? (_buffer[_offset++] & 0xff) : Read();

            switch (tag)
            {
                case 'T': return true;
                case 'F': return false;

                // direct integer
                case 0x80:
                case 0x81:
                case 0x82:
                case 0x83:
                case 0x84:
                case 0x85:
                case 0x86:
                case 0x87:
                case 0x88:
                case 0x89:
                case 0x8a:
                case 0x8b:
                case 0x8c:
                case 0x8d:
                case 0x8e:
                case 0x8f:

                case 0x90:
                case 0x91:
                case 0x92:
                case 0x93:
                case 0x94:
                case 0x95:
                case 0x96:
                case 0x97:
                case 0x98:
                case 0x99:
                case 0x9a:
                case 0x9b:
                case 0x9c:
                case 0x9d:
                case 0x9e:
                case 0x9f:

                case 0xa0:
                case 0xa1:
                case 0xa2:
                case 0xa3:
                case 0xa4:
                case 0xa5:
                case 0xa6:
                case 0xa7:
                case 0xa8:
                case 0xa9:
                case 0xaa:
                case 0xab:
                case 0xac:
                case 0xad:
                case 0xae:
                case 0xaf:

                case 0xb0:
                case 0xb1:
                case 0xb2:
                case 0xb3:
                case 0xb4:
                case 0xb5:
                case 0xb6:
                case 0xb7:
                case 0xb8:
                case 0xb9:
                case 0xba:
                case 0xbb:
                case 0xbc:
                case 0xbd:
                case 0xbe:
                case 0xbf:
                    return tag != CHessian2Constants.BC_INT_ZERO;

                // INT_BYTE = 0
                case 0xc8:
                    return Read() != 0;

                // INT_BYTE != 0
                case 0xc0:
                case 0xc1:
                case 0xc2:
                case 0xc3:
                case 0xc4:
                case 0xc5:
                case 0xc6:
                case 0xc7:
                case 0xc9:
                case 0xca:
                case 0xcb:
                case 0xcc:
                case 0xcd:
                case 0xce:
                case 0xcf:
                    Read();
                    return true;

                // INT_SHORT = 0
                case 0xd4:
                    return (256 * Read() + Read()) != 0;

                // INT_SHORT != 0
                case 0xd0:
                case 0xd1:
                case 0xd2:
                case 0xd3:
                case 0xd5:
                case 0xd6:
                case 0xd7:
                    Read();
                    Read();
                    return true;

                case 'I':
                    return
              ParseInt() != 0;

                case 0xd8:
                case 0xd9:
                case 0xda:
                case 0xdb:
                case 0xdc:
                case 0xdd:
                case 0xde:
                case 0xdf:

                case 0xe0:
                case 0xe1:
                case 0xe2:
                case 0xe3:
                case 0xe4:
                case 0xe5:
                case 0xe6:
                case 0xe7:
                case 0xe8:
                case 0xe9:
                case 0xea:
                case 0xeb:
                case 0xec:
                case 0xed:
                case 0xee:
                case 0xef:
                    return tag != CHessian2Constants.BC_LONG_ZERO;

                // LONG_BYTE = 0
                case 0xf8:
                    return Read() != 0;

                // LONG_BYTE != 0
                case 0xf0:
                case 0xf1:
                case 0xf2:
                case 0xf3:
                case 0xf4:
                case 0xf5:
                case 0xf6:
                case 0xf7:
                case 0xf9:
                case 0xfa:
                case 0xfb:
                case 0xfc:
                case 0xfd:
                case 0xfe:
                case 0xff:
                    Read();
                    return true;

                // INT_SHORT = 0
                case 0x3c:
                    return (256 * Read() + Read()) != 0;

                // INT_SHORT != 0
                case 0x38:
                case 0x39:
                case 0x3a:
                case 0x3b:
                case 0x3d:
                case 0x3e:
                case 0x3f:
                    Read();
                    Read();
                    return true;

                case CHessian2Constants.BC_LONG_INT:
                    return (0x1000000L * Read()
                            + 0x10000L * Read()
                            + 0x100 * Read()
                            + Read()) != 0;

                case 'L':
                    return ParseLong() != 0;

                case CHessian2Constants.BC_DOUBLE_ZERO:
                    return false;

                case CHessian2Constants.BC_DOUBLE_ONE:
                    return true;

                case CHessian2Constants.BC_DOUBLE_BYTE:
                    return Read() != 0;

                case CHessian2Constants.BC_DOUBLE_SHORT:
                    return (0x100 * Read() + Read()) != 0;

                case CHessian2Constants.BC_DOUBLE_MILL:
                    {
                        int mills = ParseInt();

                        return mills != 0;
                    }

                case 'D':
                    return ParseDouble() != 0.0;

                case 'N':
                    return false;

                default:
                    throw Expect("boolean", tag);
            }
        }


        /// <summary>
        /// * Reads a short
        /// *
        /// * <code>
        /// * I b32 b24 b16 b8
        /// * </pre>
        /// </summary>
        /// <returns></returns>
        public short ReadShort()
        {
            return (short)ReadInt();
        }

        /// <summary>
        /// * Reads a null
        /// *
        /// * <code>
        /// * N
        /// * </pre>
        /// </summary>
        public override void ReadNull()
        {
            int tag = Read();

            switch (tag)
            {
                case 'N': return;

                default:
                    throw Expect("null", tag);
            }
        }

        /// <summary>
        /// * Reads a byte array
        /// *
        /// * <code>
        /// * B b16 b8 data value
        /// * </pre>
        /// </summary>
        public override byte[] ReadBytes()
        {
            int tag = Read();

            switch (tag)
            {
                case 'N':
                    return null;

                case CHessian2Constants.BC_BINARY:
                case CHessian2Constants.BC_BINARY_CHUNK:
                    _isLastChunk = tag == CHessian2Constants.BC_BINARY;
                    _chunkLength = (Read() << 8) + Read();

                    MemoryStream bos = new MemoryStream();

                    int data;
                    while ((data = ParseByte()) >= 0)
                        bos.WriteByte((byte)data);

                    return bos.ToArray();

                case 0x20:
                case 0x21:
                case 0x22:
                case 0x23:
                case 0x24:
                case 0x25:
                case 0x26:
                case 0x27:
                case 0x28:
                case 0x29:
                case 0x2a:
                case 0x2b:
                case 0x2c:
                case 0x2d:
                case 0x2e:
                case 0x2f:
                    {
                        _isLastChunk = true;
                        _chunkLength = tag - 0x20;

                        byte[] buffer = new byte[_chunkLength];

                        int offset = 0;
                        while (offset < _chunkLength)
                        {
                            int sublen = Read(buffer, 0, _chunkLength - offset);

                            if (sublen <= 0)
                                break;

                            offset += sublen;
                        }

                        return buffer;
                    }

                case 0x34:
                case 0x35:
                case 0x36:
                case 0x37:
                    {
                        _isLastChunk = true;
                        _chunkLength = (tag - 0x34) * 256 + Read();

                        byte[] buffer = new byte[_chunkLength];

                        int offset = 0;
                        while (offset < _chunkLength)
                        {
                            int sublen = Read(buffer, 0, _chunkLength - offset);

                            if (sublen <= 0)
                                break;

                            offset += sublen;
                        }

                        return buffer;
                    }

                default:
                    throw Expect("bytes", tag);
            }
        }

        /// <summary>
        /// Reads a byte from the stream.
        /// </summary>
        public int ReadByte()
        {
            if (_chunkLength > 0)
            {
                _chunkLength--;
                if (_chunkLength == 0 && _isLastChunk)
                    _chunkLength = CHessian2Constants.END_OF_DATA;

                return Read();
            }
            else if (_chunkLength == CHessian2Constants.END_OF_DATA)
            {
                _chunkLength = 0;
                return -1;
            }

            int tag = Read();

            switch (tag)
            {
                case 'N':
                    return -1;

                case 'B':
                case CHessian2Constants.BC_BINARY_CHUNK:
                    {
                        _isLastChunk = tag == 'B';
                        _chunkLength = (Read() << 8) + Read();

                        int value = ParseByte();

                        // special code so successive read byte won't
                        // be read as a single object.
                        if (_chunkLength == 0 && _isLastChunk)
                            _chunkLength = CHessian2Constants.END_OF_DATA;

                        return value;
                    }

                case 0x20:
                case 0x21:
                case 0x22:
                case 0x23:
                case 0x24:
                case 0x25:
                case 0x26:
                case 0x27:
                case 0x28:
                case 0x29:
                case 0x2a:
                case 0x2b:
                case 0x2c:
                case 0x2d:
                case 0x2e:
                case 0x2f:
                    {
                        _isLastChunk = true;
                        _chunkLength = tag - 0x20;

                        int value = ParseByte();

                        // special code so successive read byte won't
                        // be read as a single object.
                        if (_chunkLength == 0)
                            _chunkLength = CHessian2Constants.END_OF_DATA;

                        return value;
                    }

                case 0x34:
                case 0x35:
                case 0x36:
                case 0x37:
                    {
                        _isLastChunk = true;
                        _chunkLength = (tag - 0x34) * 256 + Read();

                        int value = ParseByte();

                        // special code so successive read byte won't
                        // be read as a single object.
                        if (_chunkLength == 0)
                            _chunkLength = CHessian2Constants.END_OF_DATA;

                        return value;
                    }

                default:
                    throw Expect("binary", tag);
            }
        }

        /// <summary>
        /// Reads a byte array from the stream.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public int ReadBytes(byte[] buffer, int offset, int length)
        {
            int readLength = 0;

            if (_chunkLength == CHessian2Constants.END_OF_DATA)
            {
                _chunkLength = 0;
                return -1;
            }
            else if (_chunkLength == 0)
            {
                int tag = Read();

                switch (tag)
                {
                    case 'N':
                        return -1;

                    case 'B':
                    case CHessian2Constants.BC_BINARY_CHUNK:
                        _isLastChunk = tag == 'B';
                        _chunkLength = (Read() << 8) + Read();
                        break;

                    case 0x20:
                    case 0x21:
                    case 0x22:
                    case 0x23:
                    case 0x24:
                    case 0x25:
                    case 0x26:
                    case 0x27:
                    case 0x28:
                    case 0x29:
                    case 0x2a:
                    case 0x2b:
                    case 0x2c:
                    case 0x2d:
                    case 0x2e:
                    case 0x2f:
                        {
                            _isLastChunk = true;
                            _chunkLength = tag - 0x20;
                            break;
                        }

                    case 0x34:
                    case 0x35:
                    case 0x36:
                    case 0x37:
                        {
                            _isLastChunk = true;
                            _chunkLength = (tag - 0x34) * 256 + Read();
                            break;
                        }

                    default:
                        throw Expect("binary", tag);
                }
            }

            while (length > 0)
            {
                if (_chunkLength > 0)
                {
                    buffer[offset++] = (byte)Read();
                    _chunkLength--;
                    length--;
                    readLength++;
                }
                else if (_isLastChunk)
                {
                    if (readLength == 0)
                        return -1;
                    else
                    {
                        _chunkLength = CHessian2Constants.END_OF_DATA;
                        return readLength;
                    }
                }
                else
                {
                    int tag = Read();

                    switch (tag)
                    {
                        case 'B':
                        case CHessian2Constants.BC_BINARY_CHUNK:
                            _isLastChunk = tag == 'B';
                            _chunkLength = (Read() << 8) + Read();
                            break;

                        default:
                            throw Expect("binary", tag);
                    }
                }
            }

            if (readLength == 0)
                return -1;
            else if (_chunkLength > 0 || !_isLastChunk)
                return readLength;
            else
            {
                _chunkLength = CHessian2Constants.END_OF_DATA;
                return readLength;
            }
        }

        /// <summary>
        /// Reads a fault.
        /// </summary>
        private Hashtable ReadFault()
        {
            Hashtable map = new Hashtable();

            int code = Read();
            for (; code > 0 && code != 'Z'; code = Read())
            {
                _offset--;

                Object key = ReadObject();
                Object value = ReadObject();

                if (key != null && value != null)
                    map.Add(key, value);
            }

            if (code != 'Z')
                throw Expect("fault", code);

            return map;
        }

        //public override DBNull ReadDBNull()
        //{
        //    int intTag = Read();
        //    switch (intTag)
        //    {
        //        case CHessian2Constants.PROT_DBNULL_TYPE: return DBNull.Value;

        //        default:
        //            throw expect(CHessian2Constants.PROT_DBNULL_TYPE.ToString(), intTag);
        //    }
        //}

        //public override decimal ReadDecimal()
        //{
        //    int intTag = Read();
        //    switch (intTag)
        //    {
        //        case CHessianProtocolConstants.PROT_BOOLEAN_TRUE: return 1;
        //        case CHessianProtocolConstants.PROT_BOOLEAN_FALSE: return 0;
        //        case CHessianProtocolConstants.PROT_INTEGER_TYPE: return ParseInt();
        //        case CHessianProtocolConstants.PROT_LONG_TYPE: return (decimal)ParseLong();
        //        case CHessianProtocolConstants.PROT_DOUBLE_TYPE: return (decimal)ParseDouble();
        //        case CHessian2Constants.PROT_DECIMAL_TYPE: return ParseDecimal();

        //        default:
        //            throw expect(CHessian2Constants.PROT_DECIMAL_TYPE.ToString(), intTag);
        //    }
        //}

        //private decimal ParseDecimal()
        //{
        //    var str = this.ReadString();
        //    return decimal.Parse(str);
        //    //byte[] bytes = new byte[16];
        //    //m_srInput.Read(bytes, 0, bytes.Length);
        //    //Int32[] bits = new Int32[4];
        //    //for (int i = 0; i <= 15; i += 4)
        //    //{
        //    //    //convert every 4 bytes into an int32
        //    //    bits[i / 4] = BitConverter.ToInt32(bytes, i);
        //    //}
        //    ////Use the decimal's new constructor to
        //    ////create an instance of decimal
        //    //return new decimal(bits);
        //}


        /// <summary>
        /// * Reads a double
        /// *
        /// * <code>
        /// * D b64 b56 b48 b40 b32 b24 b16 b8
        /// * </pre>
        /// </summary>
        /// <returns></returns>
        public override double ReadDouble()
        {
            int tag = Read();

            switch (tag)
            {
                case 'N':
                    return 0;

                case 'F':
                    return 0;

                case 'T':
                    return 1;

                // direct integer
                case 0x80:
                case 0x81:
                case 0x82:
                case 0x83:
                case 0x84:
                case 0x85:
                case 0x86:
                case 0x87:
                case 0x88:
                case 0x89:
                case 0x8a:
                case 0x8b:
                case 0x8c:
                case 0x8d:
                case 0x8e:
                case 0x8f:

                case 0x90:
                case 0x91:
                case 0x92:
                case 0x93:
                case 0x94:
                case 0x95:
                case 0x96:
                case 0x97:
                case 0x98:
                case 0x99:
                case 0x9a:
                case 0x9b:
                case 0x9c:
                case 0x9d:
                case 0x9e:
                case 0x9f:

                case 0xa0:
                case 0xa1:
                case 0xa2:
                case 0xa3:
                case 0xa4:
                case 0xa5:
                case 0xa6:
                case 0xa7:
                case 0xa8:
                case 0xa9:
                case 0xaa:
                case 0xab:
                case 0xac:
                case 0xad:
                case 0xae:
                case 0xaf:

                case 0xb0:
                case 0xb1:
                case 0xb2:
                case 0xb3:
                case 0xb4:
                case 0xb5:
                case 0xb6:
                case 0xb7:
                case 0xb8:
                case 0xb9:
                case 0xba:
                case 0xbb:
                case 0xbc:
                case 0xbd:
                case 0xbe:
                case 0xbf:
                    return tag - 0x90;

                /* byte int */
                case 0xc0:
                case 0xc1:
                case 0xc2:
                case 0xc3:
                case 0xc4:
                case 0xc5:
                case 0xc6:
                case 0xc7:
                case 0xc8:
                case 0xc9:
                case 0xca:
                case 0xcb:
                case 0xcc:
                case 0xcd:
                case 0xce:
                case 0xcf:
                    return ((tag - CHessian2Constants.BC_INT_BYTE_ZERO) << 8) + Read();

                /* short int */
                case 0xd0:
                case 0xd1:
                case 0xd2:
                case 0xd3:
                case 0xd4:
                case 0xd5:
                case 0xd6:
                case 0xd7:
                    return ((tag - CHessian2Constants.BC_INT_SHORT_ZERO) << 16) + 256 * Read() + Read();

                case 'I':
                case CHessian2Constants.BC_LONG_INT:
                    return ParseInt();

                // direct long
                case 0xd8:
                case 0xd9:
                case 0xda:
                case 0xdb:
                case 0xdc:
                case 0xdd:
                case 0xde:
                case 0xdf:

                case 0xe0:
                case 0xe1:
                case 0xe2:
                case 0xe3:
                case 0xe4:
                case 0xe5:
                case 0xe6:
                case 0xe7:
                case 0xe8:
                case 0xe9:
                case 0xea:
                case 0xeb:
                case 0xec:
                case 0xed:
                case 0xee:
                case 0xef:
                    return tag - CHessian2Constants.BC_LONG_ZERO;

                /* byte long */
                case 0xf0:
                case 0xf1:
                case 0xf2:
                case 0xf3:
                case 0xf4:
                case 0xf5:
                case 0xf6:
                case 0xf7:
                case 0xf8:
                case 0xf9:
                case 0xfa:
                case 0xfb:
                case 0xfc:
                case 0xfd:
                case 0xfe:
                case 0xff:
                    return ((tag - CHessian2Constants.BC_LONG_BYTE_ZERO) << 8) + Read();

                /* short long */
                case 0x38:
                case 0x39:
                case 0x3a:
                case 0x3b:
                case 0x3c:
                case 0x3d:
                case 0x3e:
                case 0x3f:
                    return ((tag - CHessian2Constants.BC_LONG_SHORT_ZERO) << 16) + 256 * Read() + Read();

                case 'L':
                    return (double)ParseLong();

                case CHessian2Constants.BC_DOUBLE_ZERO:
                    return 0;

                case CHessian2Constants.BC_DOUBLE_ONE:
                    return 1;

                case CHessian2Constants.BC_DOUBLE_BYTE:
                    //因为Java中的byte取值范围是-128~127，C#中的byte的取值范围是0~255，Java byte在C#中对应sbyte,所有要转成sbyte.
                    return _offset < _length ? (sbyte)_buffer[_offset++] : (sbyte)Read();

                case CHessian2Constants.BC_DOUBLE_SHORT:
                    return (short)(256 * Read() + Read());

                case CHessian2Constants.BC_DOUBLE_MILL:
                    {
                        int mills = ParseInt();

                        return 0.001 * mills;
                    }

                case 'D':
                    return ParseDouble();

                default:
                    throw Expect("double", tag);
            }
        }

        /// <summary>
        /// Reads the end byte.
        /// </summary>
        public override void ReadEnd()
        {
            int code = _offset < _length ? (_buffer[_offset++] & 0xff) : Read();

            if (code == 'Z')
                return;
            else if (code < 0)
                throw Error("unexpected end of file");
            else
                throw Error("unknown code:" + CodeName(code));
        }

        public override Stream ReadInputStream()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// * Reads an integer
        /// *
        /// * <code>
        /// * I b32 b24 b16 b8
        /// * </pre>
        /// </summary>
        /// <returns></returns>
        public override int ReadInt()
        {

            //int tag = _offset < _length ? (_buffer[_offset++] & 0xff) : read();
            int tag = Read();

            switch (tag)
            {
                case 'N':
                    return 0;

                case 'F':
                    return 0;

                case 'T':
                    return 1;

                // direct integer
                case 0x80:
                case 0x81:
                case 0x82:
                case 0x83:
                case 0x84:
                case 0x85:
                case 0x86:
                case 0x87:
                case 0x88:
                case 0x89:
                case 0x8a:
                case 0x8b:
                case 0x8c:
                case 0x8d:
                case 0x8e:
                case 0x8f:

                case 0x90:
                case 0x91:
                case 0x92:
                case 0x93:
                case 0x94:
                case 0x95:
                case 0x96:
                case 0x97:
                case 0x98:
                case 0x99:
                case 0x9a:
                case 0x9b:
                case 0x9c:
                case 0x9d:
                case 0x9e:
                case 0x9f:

                case 0xa0:
                case 0xa1:
                case 0xa2:
                case 0xa3:
                case 0xa4:
                case 0xa5:
                case 0xa6:
                case 0xa7:
                case 0xa8:
                case 0xa9:
                case 0xaa:
                case 0xab:
                case 0xac:
                case 0xad:
                case 0xae:
                case 0xaf:

                case 0xb0:
                case 0xb1:
                case 0xb2:
                case 0xb3:
                case 0xb4:
                case 0xb5:
                case 0xb6:
                case 0xb7:
                case 0xb8:
                case 0xb9:
                case 0xba:
                case 0xbb:
                case 0xbc:
                case 0xbd:
                case 0xbe:
                case 0xbf:
                    return tag - CHessian2Constants.BC_INT_ZERO;

                /* byte int */
                case 0xc0:
                case 0xc1:
                case 0xc2:
                case 0xc3:
                case 0xc4:
                case 0xc5:
                case 0xc6:
                case 0xc7:
                case 0xc8:
                case 0xc9:
                case 0xca:
                case 0xcb:
                case 0xcc:
                case 0xcd:
                case 0xce:
                case 0xcf:
                    return ((tag - CHessian2Constants.BC_INT_BYTE_ZERO) << 8) + Read();

                /* short int */
                case 0xd0:
                case 0xd1:
                case 0xd2:
                case 0xd3:
                case 0xd4:
                case 0xd5:
                case 0xd6:
                case 0xd7:
                    return ((tag - CHessian2Constants.BC_INT_SHORT_ZERO) << 16) + 256 * Read() + Read();

                case 'I':
                case CHessian2Constants.BC_LONG_INT:
                    return ((Read() << 24)
                            + (Read() << 16)
                            + (Read() << 8)
                            + Read());

                // direct long
                case 0xd8:
                case 0xd9:
                case 0xda:
                case 0xdb:
                case 0xdc:
                case 0xdd:
                case 0xde:
                case 0xdf:

                case 0xe0:
                case 0xe1:
                case 0xe2:
                case 0xe3:
                case 0xe4:
                case 0xe5:
                case 0xe6:
                case 0xe7:
                case 0xe8:
                case 0xe9:
                case 0xea:
                case 0xeb:
                case 0xec:
                case 0xed:
                case 0xee:
                case 0xef:
                    return tag - CHessian2Constants.BC_LONG_ZERO;

                /* byte long */
                case 0xf0:
                case 0xf1:
                case 0xf2:
                case 0xf3:
                case 0xf4:
                case 0xf5:
                case 0xf6:
                case 0xf7:
                case 0xf8:
                case 0xf9:
                case 0xfa:
                case 0xfb:
                case 0xfc:
                case 0xfd:
                case 0xfe:
                case 0xff:
                    return ((tag - CHessian2Constants.BC_LONG_BYTE_ZERO) << 8) + Read();

                /* short long */
                case 0x38:
                case 0x39:
                case 0x3a:
                case 0x3b:
                case 0x3c:
                case 0x3d:
                case 0x3e:
                case 0x3f:
                    return ((tag - CHessian2Constants.BC_LONG_SHORT_ZERO) << 16) + 256 * Read() + Read();

                case 'L':
                    return (int)ParseLong();

                case CHessian2Constants.BC_DOUBLE_ZERO:
                    return 0;

                case CHessian2Constants.BC_DOUBLE_ONE:
                    return 1;

                //case LONG_BYTE:
                case CHessian2Constants.BC_DOUBLE_BYTE:
                    //因为Java中的byte取值范围是-128~127，C#中的byte的取值范围是0~255，Java byte在C#中对应sbyte,所有要转成sbyte.
                    return _offset < _length ? (sbyte)_buffer[_offset++] : (sbyte)Read();

                //case INT_SHORT:
                //case LONG_SHORT:
                case CHessian2Constants.BC_DOUBLE_SHORT:
                    return (short)(256 * Read() + Read());

                case CHessian2Constants.BC_DOUBLE_MILL:
                    {
                        int mills = ParseInt();

                        return (int)(0.001 * mills);
                    }

                case 'D':
                    return (int)ParseDouble();

                default:
                    throw Expect("integer", tag);
            }
        }

        public override int ReadLength()
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Reads the end byte.
        /// </summary>
        public override void ReadListEnd()
        {
            int code = _offset < _length ? (_buffer[_offset++] & 0xff) : Read();

            if (code != 'Z')
                throw Error("expected end of list ('Z') at '" + CodeName(code) + "'");
        }

        /// <summary>
        /// Reads the start of a list.
        /// </summary>
        /// <returns></returns>
        public override int ReadListStart()
        {
            return Read();
        }

        /// <summary>
        /// * Reads a long
        /// *
        /// * <code>
        /// * L b64 b56 b48 b40 b32 b24 b16 b8
        /// * </pre>
        /// </summary>
        /// <returns></returns>
        public override long ReadLong()
        {
            int tag = Read();

            switch (tag)
            {
                case 'N':
                    return 0;

                case 'F':
                    return 0;

                case 'T':
                    return 1;

                // direct integer
                case 0x80:
                case 0x81:
                case 0x82:
                case 0x83:
                case 0x84:
                case 0x85:
                case 0x86:
                case 0x87:
                case 0x88:
                case 0x89:
                case 0x8a:
                case 0x8b:
                case 0x8c:
                case 0x8d:
                case 0x8e:
                case 0x8f:

                case 0x90:
                case 0x91:
                case 0x92:
                case 0x93:
                case 0x94:
                case 0x95:
                case 0x96:
                case 0x97:
                case 0x98:
                case 0x99:
                case 0x9a:
                case 0x9b:
                case 0x9c:
                case 0x9d:
                case 0x9e:
                case 0x9f:

                case 0xa0:
                case 0xa1:
                case 0xa2:
                case 0xa3:
                case 0xa4:
                case 0xa5:
                case 0xa6:
                case 0xa7:
                case 0xa8:
                case 0xa9:
                case 0xaa:
                case 0xab:
                case 0xac:
                case 0xad:
                case 0xae:
                case 0xaf:

                case 0xb0:
                case 0xb1:
                case 0xb2:
                case 0xb3:
                case 0xb4:
                case 0xb5:
                case 0xb6:
                case 0xb7:
                case 0xb8:
                case 0xb9:
                case 0xba:
                case 0xbb:
                case 0xbc:
                case 0xbd:
                case 0xbe:
                case 0xbf:
                    return tag - CHessian2Constants.BC_INT_ZERO;

                /* byte int */
                case 0xc0:
                case 0xc1:
                case 0xc2:
                case 0xc3:
                case 0xc4:
                case 0xc5:
                case 0xc6:
                case 0xc7:
                case 0xc8:
                case 0xc9:
                case 0xca:
                case 0xcb:
                case 0xcc:
                case 0xcd:
                case 0xce:
                case 0xcf:
                    return ((tag - CHessian2Constants.BC_INT_BYTE_ZERO) << 8) + Read();

                /* short int */
                case 0xd0:
                case 0xd1:
                case 0xd2:
                case 0xd3:
                case 0xd4:
                case 0xd5:
                case 0xd6:
                case 0xd7:
                    return ((tag - CHessian2Constants.BC_INT_SHORT_ZERO) << 16) + 256 * Read() + Read();

                //case LONG_BYTE:
                case CHessian2Constants.BC_DOUBLE_BYTE:
                    //因为Java中的byte取值范围是-128~127，C#中的byte的取值范围是0~255，Java byte在C#中对应sbyte,所有要转成sbyte.
                    return _offset < _length ? (sbyte)_buffer[_offset++] : (sbyte)Read();

                //case INT_SHORT:
                //case LONG_SHORT:
                case CHessian2Constants.BC_DOUBLE_SHORT:
                    return (short)(256 * Read() + Read());

                case 'I':
                case CHessian2Constants.BC_LONG_INT:
                    return ParseInt();

                // direct long
                case 0xd8:
                case 0xd9:
                case 0xda:
                case 0xdb:
                case 0xdc:
                case 0xdd:
                case 0xde:
                case 0xdf:

                case 0xe0:
                case 0xe1:
                case 0xe2:
                case 0xe3:
                case 0xe4:
                case 0xe5:
                case 0xe6:
                case 0xe7:
                case 0xe8:
                case 0xe9:
                case 0xea:
                case 0xeb:
                case 0xec:
                case 0xed:
                case 0xee:
                case 0xef:
                    return tag - CHessian2Constants.BC_LONG_ZERO;

                /* byte long */
                case 0xf0:
                case 0xf1:
                case 0xf2:
                case 0xf3:
                case 0xf4:
                case 0xf5:
                case 0xf6:
                case 0xf7:
                case 0xf8:
                case 0xf9:
                case 0xfa:
                case 0xfb:
                case 0xfc:
                case 0xfd:
                case 0xfe:
                case 0xff:
                    return ((tag - CHessian2Constants.BC_LONG_BYTE_ZERO) << 8) + Read();

                /* short long */
                case 0x38:
                case 0x39:
                case 0x3a:
                case 0x3b:
                case 0x3c:
                case 0x3d:
                case 0x3e:
                case 0x3f:
                    return ((tag - CHessian2Constants.BC_LONG_SHORT_ZERO) << 16) + 256 * Read() + Read();

                case 'L':
                    return ParseLong();

                case CHessian2Constants.BC_DOUBLE_ZERO:
                    return 0;

                case CHessian2Constants.BC_DOUBLE_ONE:
                    return 1;

                case CHessian2Constants.BC_DOUBLE_MILL:
                    {
                        int mills = ParseInt();

                        return (long)(0.001 * mills);
                    }

                case 'D':
                    return (long)ParseDouble();

                default:
                    throw Expect("long", tag);
            }
        }

        /// <summary>
        /// * Reads a float
        /// *
        /// * <code>
        /// * D b64 b56 b48 b40 b32 b24 b16 b8
        /// * </pre>
        /// </summary>
        /// <returns></returns>
        public float ReadFloat()
        {
            return (float)ReadDouble();
        }

        /// <summary>
        /// eads the end byte.
        /// </summary>
        public override void ReadMapEnd()
        {
            int code = _offset < _length ? (_buffer[_offset++] & 0xff) : Read();

            if (code != 'Z')
                throw Error("expected end of map ('Z') at '" + CodeName(code) + "'");
        }

        /// <summary>
        /// Reads the start of a list.
        /// </summary>
        /// <returns></returns>
        public override int ReadMapStart()
        {
            return Read();
        }

        /// <summary>
        /// Reads an object from the input stream with an expected type.
        /// </summary>
        /// <param name="expectedType"></param>
        /// <returns></returns>
        public override object ReadObject(Type expectedType)
        {
            if (expectedType == null || expectedType == typeof(object))
                return ReadObject();

            int tag = _offset < _length ? (_buffer[_offset++] & 0xff) : Read();

            switch (tag)
            {
                case 'N':
                    return null;

                case 'H':
                    {
                        IDeserializer reader = FindSerializerFactory().GetDeserializer(expectedType);

                        return reader.ReadMap(this);
                    }

                case 'M':
                    {
                        String type = ReadType();

                        // hessian/3bb3
                        if ("".Equals(type))
                        {
                            IDeserializer reader;
                            reader = FindSerializerFactory().GetDeserializer(expectedType);

                            return reader.ReadMap(this);
                        }
                        else
                        {
                            IDeserializer reader;
                            reader = FindSerializerFactory().GetObjectDeserializer(type, expectedType);

                            return reader.ReadMap(this);
                        }
                    }

                case 'C':
                    {
                        ReadObjectDefinition(expectedType);

                        return ReadObject(expectedType);
                    }

                case 0x60:
                case 0x61:
                case 0x62:
                case 0x63:
                case 0x64:
                case 0x65:
                case 0x66:
                case 0x67:
                case 0x68:
                case 0x69:
                case 0x6a:
                case 0x6b:
                case 0x6c:
                case 0x6d:
                case 0x6e:
                case 0x6f:
                    {
                        int iref = tag - 0x60;
                        int size = _classDefs.Count;

                        if (iref < 0 || size <= iref)
                            throw new CHessianException("'" + iref + "' is an unknown class definition");

                        ObjectDefinition def = _classDefs[iref];

                        return ReadObjectInstance(expectedType, def);
                    }

                case 'O':
                    {
                        int iref = ReadInt();
                        int size = _classDefs.Count;

                        if (iref < 0 || size <= iref)
                            throw new CHessianException("'" + iref + "' is an unknown class definition");

                        ObjectDefinition def = _classDefs[iref];

                        return ReadObjectInstance(expectedType, def);
                    }

                case CHessian2Constants.BC_LIST_VARIABLE:
                    {
                        String type = ReadType();

                        IDeserializer reader;
                        reader = FindSerializerFactory().GetListDeserializer(type, expectedType);

                        Object v = reader.ReadList(this, -1);

                        return v;
                    }

                case CHessian2Constants.BC_LIST_FIXED:
                    {
                        String type = ReadType();
                        int length = ReadInt();

                        IDeserializer reader;
                        reader = FindSerializerFactory().GetListDeserializer(type, expectedType);

                        Object v = reader.ReadLengthList(this, length);

                        return v;
                    }

                case 0x70:
                case 0x71:
                case 0x72:
                case 0x73:
                case 0x74:
                case 0x75:
                case 0x76:
                case 0x77:
                    {
                        int length = tag - 0x70;

                        String type = ReadType();

                        IDeserializer reader;
                        reader = FindSerializerFactory().GetListDeserializer(type, expectedType);

                        Object v = reader.ReadLengthList(this, length);

                        return v;
                    }

                case CHessian2Constants.BC_LIST_VARIABLE_UNTYPED:
                    {
                        IDeserializer reader;
                        reader = FindSerializerFactory().GetListDeserializer(null, expectedType);

                        Object v = reader.ReadList(this, -1);

                        return v;
                    }

                case CHessian2Constants.BC_LIST_FIXED_UNTYPED:
                    {
                        int length = ReadInt();

                        IDeserializer reader;
                        reader = FindSerializerFactory().GetListDeserializer(null, expectedType);

                        Object v = reader.ReadLengthList(this, length);

                        return v;
                    }

                case 0x78:
                case 0x79:
                case 0x7a:
                case 0x7b:
                case 0x7c:
                case 0x7d:
                case 0x7e:
                case 0x7f:
                    {
                        int length = tag - 0x78;

                        IDeserializer reader;
                        reader = FindSerializerFactory().GetListDeserializer(null, expectedType);

                        Object v = reader.ReadLengthList(this, length);

                        return v;
                    }

                case CHessian2Constants.BC_REF:
                    {
                        int iref = ReadInt();

                        return _refs[iref];
                    }
            }

            if (tag >= 0)
                _offset--;

            // hessian/3b2i vs hessian/3406
            // return readObject();
            Object value = FindSerializerFactory().GetDeserializer(expectedType).ReadObject(this);
            return value;
        }

        /// <summary>
        /// * Reads an arbitrary object from the input stream when the type
        /// * is unknown.
        /// </summary>
        /// <returns></returns>
        public override object ReadObject()
        {

            int tag = _offset < _length ? (_buffer[_offset++] & 0xff) : Read();

            switch (tag)
            {
                case 'N':
                    return null;

                case 'T':
                    return true;

                case 'F':
                    return false;

                // direct integer
                case 0x80:
                case 0x81:
                case 0x82:
                case 0x83:
                case 0x84:
                case 0x85:
                case 0x86:
                case 0x87:
                case 0x88:
                case 0x89:
                case 0x8a:
                case 0x8b:
                case 0x8c:
                case 0x8d:
                case 0x8e:
                case 0x8f:

                case 0x90:
                case 0x91:
                case 0x92:
                case 0x93:
                case 0x94:
                case 0x95:
                case 0x96:
                case 0x97:
                case 0x98:
                case 0x99:
                case 0x9a:
                case 0x9b:
                case 0x9c:
                case 0x9d:
                case 0x9e:
                case 0x9f:

                case 0xa0:
                case 0xa1:
                case 0xa2:
                case 0xa3:
                case 0xa4:
                case 0xa5:
                case 0xa6:
                case 0xa7:
                case 0xa8:
                case 0xa9:
                case 0xaa:
                case 0xab:
                case 0xac:
                case 0xad:
                case 0xae:
                case 0xaf:

                case 0xb0:
                case 0xb1:
                case 0xb2:
                case 0xb3:
                case 0xb4:
                case 0xb5:
                case 0xb6:
                case 0xb7:
                case 0xb8:
                case 0xb9:
                case 0xba:
                case 0xbb:
                case 0xbc:
                case 0xbd:
                case 0xbe:
                case 0xbf:
                    return (tag - CHessian2Constants.BC_INT_ZERO);

                /* byte int */
                case 0xc0:
                case 0xc1:
                case 0xc2:
                case 0xc3:
                case 0xc4:
                case 0xc5:
                case 0xc6:
                case 0xc7:
                case 0xc8:
                case 0xc9:
                case 0xca:
                case 0xcb:
                case 0xcc:
                case 0xcd:
                case 0xce:
                case 0xcf:
                    return (((tag - CHessian2Constants.BC_INT_BYTE_ZERO) << 8) + Read());

                /* short int */
                case 0xd0:
                case 0xd1:
                case 0xd2:
                case 0xd3:
                case 0xd4:
                case 0xd5:
                case 0xd6:
                case 0xd7:
                    return (((tag - CHessian2Constants.BC_INT_SHORT_ZERO) << 16)
                                           + 256 * Read() + Read());

                case 'I':
                    return (ParseInt());

                // direct long
                case 0xd8:
                case 0xd9:
                case 0xda:
                case 0xdb:
                case 0xdc:
                case 0xdd:
                case 0xde:
                case 0xdf:

                case 0xe0:
                case 0xe1:
                case 0xe2:
                case 0xe3:
                case 0xe4:
                case 0xe5:
                case 0xe6:
                case 0xe7:
                case 0xe8:
                case 0xe9:
                case 0xea:
                case 0xeb:
                case 0xec:
                case 0xed:
                case 0xee:
                case 0xef:
                    return (tag - CHessian2Constants.BC_LONG_ZERO);

                /* byte long */
                case 0xf0:
                case 0xf1:
                case 0xf2:
                case 0xf3:
                case 0xf4:
                case 0xf5:
                case 0xf6:
                case 0xf7:
                case 0xf8:
                case 0xf9:
                case 0xfa:
                case 0xfb:
                case 0xfc:
                case 0xfd:
                case 0xfe:
                case 0xff:
                    return (((tag - CHessian2Constants.BC_LONG_BYTE_ZERO) << 8) + Read());

                /* short long */
                case 0x38:
                case 0x39:
                case 0x3a:
                case 0x3b:
                case 0x3c:
                case 0x3d:
                case 0x3e:
                case 0x3f:
                    return (((tag - CHessian2Constants.BC_LONG_SHORT_ZERO) << 16) + 256 * Read() + Read());

                case CHessian2Constants.BC_LONG_INT:
                    return (ParseInt());

                case 'L':
                    return (ParseLong());

                case CHessian2Constants.BC_DOUBLE_ZERO:
                    return (double)(0);

                case CHessian2Constants.BC_DOUBLE_ONE:
                    return (double)(1);

                case CHessian2Constants.BC_DOUBLE_BYTE:
                    //因为Java中的byte取值范围是-128~127，C#中的byte的取值范围是0~255，Java byte在C#中对应sbyte,所有要转成sbyte.
                    return (double)((sbyte)Read());

                case CHessian2Constants.BC_DOUBLE_SHORT:
                    return (double)((short)(256 * Read() + Read()));

                case CHessian2Constants.BC_DOUBLE_MILL:
                    {
                        int mills = ParseInt();

                        return (double)(0.001 * mills);
                    }

                case 'D':
                    return (double)(ParseDouble());

                case CHessian2Constants.BC_DATE:
                    {
                        long javaTime = ParseLong();
                        const long timeShift = 62135596800000;
                        DateTime dt = new DateTime((javaTime + timeShift) * 10000, DateTimeKind.Utc);
                        if (dt != DateTime.MinValue)
                        {
                            dt = dt.ToLocalTime(); // der Einfachheit halber
                        }
                        return dt;
                    }
                //return new DateTime(ParseLong());

                case CHessian2Constants.BC_DATE_MINUTE:
                    {
                        long javaTime = ParseInt() * 60000L;
                        const long timeShift = 62135596800000;
                        DateTime dt = new DateTime((javaTime + timeShift) * 10000, DateTimeKind.Utc);
                        if (dt != DateTime.MinValue)
                        {
                            dt = dt.ToLocalTime(); // der Einfachheit halber
                        }
                        return dt;
                    }
                //return new DateTime(ParseInt() * 60000L);

                case CHessian2Constants.BC_STRING_CHUNK:
                case 'S':
                    {
                        _isLastChunk = tag == 'S';
                        _chunkLength = (Read() << 8) + Read();

                        _sbuf.Length = 0;

                        ParseString(_sbuf);

                        return _sbuf.ToString();
                    }

                case 0x00:
                case 0x01:
                case 0x02:
                case 0x03:
                case 0x04:
                case 0x05:
                case 0x06:
                case 0x07:
                case 0x08:
                case 0x09:
                case 0x0a:
                case 0x0b:
                case 0x0c:
                case 0x0d:
                case 0x0e:
                case 0x0f:

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
                    {
                        _isLastChunk = true;
                        _chunkLength = tag - 0x00;

                        int data;
                        _sbuf.Length = (0);

                        ParseString(_sbuf);

                        return _sbuf.ToString();
                    }

                case 0x30:
                case 0x31:
                case 0x32:
                case 0x33:
                    {
                        _isLastChunk = true;
                        _chunkLength = (tag - 0x30) * 256 + Read();

                        _sbuf.Length = (0);

                        ParseString(_sbuf);

                        return _sbuf.ToString();
                    }

                case CHessian2Constants.BC_BINARY_CHUNK:
                case 'B':
                    {
                        _isLastChunk = tag == 'B';
                        _chunkLength = (Read() << 8) + Read();

                        int data;
                        MemoryStream bos = new MemoryStream();

                        while ((data = ParseByte()) >= 0)
                            bos.WriteByte((byte)data);

                        return bos.ToArray();
                    }

                case 0x20:
                case 0x21:
                case 0x22:
                case 0x23:
                case 0x24:
                case 0x25:
                case 0x26:
                case 0x27:
                case 0x28:
                case 0x29:
                case 0x2a:
                case 0x2b:
                case 0x2c:
                case 0x2d:
                case 0x2e:
                case 0x2f:
                    {
                        _isLastChunk = true;
                        int len = tag - 0x20;
                        _chunkLength = 0;

                        byte[] data = new byte[len];

                        for (int i = 0; i < len; i++)
                            data[i] = (byte)Read();

                        return data;
                    }

                case 0x34:
                case 0x35:
                case 0x36:
                case 0x37:
                    {
                        _isLastChunk = true;
                        int len = (tag - 0x34) * 256 + Read();
                        _chunkLength = 0;

                        byte[] buffer = new byte[len];

                        for (int i = 0; i < len; i++)
                        {
                            buffer[i] = (byte)Read();
                        }

                        return buffer;
                    }

                case CHessian2Constants.BC_LIST_VARIABLE:
                    {
                        // variable length list
                        String type = ReadType();

                        return FindSerializerFactory().ReadList(this, -1, type);
                    }

                case CHessian2Constants.BC_LIST_VARIABLE_UNTYPED:
                    {
                        return FindSerializerFactory().ReadList(this, -1, null);
                    }

                case CHessian2Constants.BC_LIST_FIXED:
                    {
                        // fixed length lists
                        String type = ReadType();
                        int length = ReadInt();

                        IDeserializer reader;
                        reader = FindSerializerFactory().GetListDeserializer(type, null);

                        return reader.ReadLengthList(this, length);
                    }

                case CHessian2Constants.BC_LIST_FIXED_UNTYPED:
                    {
                        // fixed length lists
                        int length = ReadInt();

                        IDeserializer reader;
                        reader = FindSerializerFactory().GetListDeserializer(null, null);

                        return reader.ReadLengthList(this, length);
                    }

                // compact fixed list
                case 0x70:
                case 0x71:
                case 0x72:
                case 0x73:
                case 0x74:
                case 0x75:
                case 0x76:
                case 0x77:
                    {
                        // fixed length lists
                        String type = ReadType();
                        int length = tag - 0x70;

                        IDeserializer reader;
                        reader = FindSerializerFactory().GetListDeserializer(type, null);

                        return reader.ReadLengthList(this, length);
                    }

                // compact fixed untyped list
                case 0x78:
                case 0x79:
                case 0x7a:
                case 0x7b:
                case 0x7c:
                case 0x7d:
                case 0x7e:
                case 0x7f:
                    {
                        // fixed length lists
                        int length = tag - 0x78;

                        IDeserializer reader;
                        reader = FindSerializerFactory().GetListDeserializer(null, null);

                        return reader.ReadLengthList(this, length);
                    }

                case 'H':
                    {
                        return FindSerializerFactory().ReadMap(this, null);
                    }

                case 'M':
                    {
                        String type = ReadType();

                        return FindSerializerFactory().ReadMap(this, type);
                    }

                case 'C':
                    {
                        ReadObjectDefinition(null);

                        return ReadObject();
                    }

                case 0x60:
                case 0x61:
                case 0x62:
                case 0x63:
                case 0x64:
                case 0x65:
                case 0x66:
                case 0x67:
                case 0x68:
                case 0x69:
                case 0x6a:
                case 0x6b:
                case 0x6c:
                case 0x6d:
                case 0x6e:
                case 0x6f:
                    {
                        int iref = tag - 0x60;

                        if (_classDefs.Count <= iref)
                            throw Error("No classes defined at reference '"
                                        + string.Format("{0:x2}", tag) + "'");

                        ObjectDefinition def = _classDefs[iref];

                        return ReadObjectInstance(null, def);
                    }

                case 'O':
                    {
                        int iref = ReadInt();

                        if (_classDefs.Count <= iref)
                            throw Error("Illegal object reference #" + iref);

                        ObjectDefinition def = _classDefs[iref];

                        return ReadObjectInstance(null, def);
                    }

                case CHessian2Constants.BC_REF:
                    {
                        int iref = ReadInt();

                        return _refs[iref];
                    }

                default:
                    if (tag < 0)
                        throw new Exception("readObject: unexpected end of file");
                    else
                        throw Error("readObject: unknown code " + CodeName(tag));
            }
        }

        /// <summary>
        /// * Reads an object definition:
        /// *
        /// * <code>
        /// * O string <int> (string)* <value>*
        /// * </pre>
        /// </summary>
        /// <param name="expectedType"></param>
        private void ReadObjectDefinition(Type expectedType)
        {
            String type = ReadString();
            int len = ReadInt();

            CSerializerFactory factory = FindSerializerFactory();

            IDeserializer reader = factory.GetObjectDeserializer(type, null);

            Object[] fields = reader.CreateFields(len);
            String[] fieldNames = new String[len];

            for (int i = 0; i < len; i++)
            {
                String name = ReadString();

                fields[i] = reader.CreateField(name);
                fieldNames[i] = name;
            }

            ObjectDefinition def
                = new ObjectDefinition(type, reader, fields, fieldNames);

            _classDefs.Add(def);
        }

        private Object ReadObjectInstance(Type expectedType, ObjectDefinition def)
        {
            String type = def.GetTypeName();
            IDeserializer reader = def.GetReader();
            Object[] fields = def.GetFields();

            CSerializerFactory factory = FindSerializerFactory();

            if (expectedType != null && expectedType != reader.GetOwnType() && !expectedType.IsAssignableFrom(reader.GetOwnType()))
            {
                reader = factory.GetObjectDeserializer(type, expectedType);

                return reader.ReadObject(this, def.GetFieldNames());
            }
            else
            {
                return reader.ReadObject(this, fields);
            }
        }

        /// <summary>
        /// Reads a reference.
        /// </summary>
        /// <returns></returns>
        public override object ReadRef()
        {
            int value = ParseInt();

            return _refs[value];
        }

        /// <summary>
        /// * Reads a reply as an object.
        /// * If the reply has a fault, throws the exception.
        /// </summary>
        /// <param name="expectedType"></param>
        /// <returns></returns>
        public override object ReadReply(Type expectedType)
        {
            int tag = Read();

            if (tag == 'R')
                return ReadObject(expectedType);
            else if (tag == 'F')
            {
                Hashtable map = (Hashtable)ReadObject(typeof(Hashtable));

                throw PrepareFault(map);
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.Append((char)tag);

                try
                {
                    int ch;

                    while ((ch = Read()) >= 0)
                    {
                        sb.Append((char)ch);
                    }
                }
                catch (IOException e)
                {
                }

                throw Error("expected hessian reply at " + CodeName(tag) + "\n"
                            + sb);
            }
        }

        /// <summary>
        /// Reads a byte from the stream.
        /// </summary>
        /// <returns></returns>
        public int ReadChar()
        {
            if (_chunkLength > 0)
            {
                _chunkLength--;
                if (_chunkLength == 0 && _isLastChunk)
                    _chunkLength = CHessian2Constants.END_OF_DATA;

                int ch = ParseUTF8Char();
                return ch;
            }
            else if (_chunkLength == CHessian2Constants.END_OF_DATA)
            {
                _chunkLength = 0;
                return -1;
            }

            int tag = Read();

            switch (tag)
            {
                case 'N':
                    return -1;

                case 'S':
                case CHessian2Constants.BC_STRING_CHUNK:
                    _isLastChunk = tag == 'S';
                    _chunkLength = (Read() << 8) + Read();

                    _chunkLength--;
                    int value = ParseUTF8Char();

                    // special code so successive read byte won't
                    // be read as a single object.
                    if (_chunkLength == 0 && _isLastChunk)
                        _chunkLength = CHessian2Constants.END_OF_DATA;

                    return value;

                default:
                    throw Expect("char", tag);
            }
        }

        /// <summary>
        /// * Reads a string
        /// *
        /// * <code>
        /// * S b16 b8 string value
        /// * </pre>
        /// </summary>
        /// <returns></returns>
        public override string ReadString()
        {

            int tag = Read();

            switch (tag)
            {
                case 'N':
                    return null;
                case 'T':
                    return "true";
                case 'F':
                    return "false";

                // direct integer
                case 0x80:
                case 0x81:
                case 0x82:
                case 0x83:
                case 0x84:
                case 0x85:
                case 0x86:
                case 0x87:
                case 0x88:
                case 0x89:
                case 0x8a:
                case 0x8b:
                case 0x8c:
                case 0x8d:
                case 0x8e:
                case 0x8f:

                case 0x90:
                case 0x91:
                case 0x92:
                case 0x93:
                case 0x94:
                case 0x95:
                case 0x96:
                case 0x97:
                case 0x98:
                case 0x99:
                case 0x9a:
                case 0x9b:
                case 0x9c:
                case 0x9d:
                case 0x9e:
                case 0x9f:

                case 0xa0:
                case 0xa1:
                case 0xa2:
                case 0xa3:
                case 0xa4:
                case 0xa5:
                case 0xa6:
                case 0xa7:
                case 0xa8:
                case 0xa9:
                case 0xaa:
                case 0xab:
                case 0xac:
                case 0xad:
                case 0xae:
                case 0xaf:

                case 0xb0:
                case 0xb1:
                case 0xb2:
                case 0xb3:
                case 0xb4:
                case 0xb5:
                case 0xb6:
                case 0xb7:
                case 0xb8:
                case 0xb9:
                case 0xba:
                case 0xbb:
                case 0xbc:
                case 0xbd:
                case 0xbe:
                case 0xbf:
                    return ((tag - 0x90)).ToString();

                /* byte int */
                case 0xc0:
                case 0xc1:
                case 0xc2:
                case 0xc3:
                case 0xc4:
                case 0xc5:
                case 0xc6:
                case 0xc7:
                case 0xc8:
                case 0xc9:
                case 0xca:
                case 0xcb:
                case 0xcc:
                case 0xcd:
                case 0xce:
                case 0xcf:
                    return (((tag - CHessian2Constants.BC_INT_BYTE_ZERO) << 8) + Read()).ToString();

                /* short int */
                case 0xd0:
                case 0xd1:
                case 0xd2:
                case 0xd3:
                case 0xd4:
                case 0xd5:
                case 0xd6:
                case 0xd7:
                    return (((tag - CHessian2Constants.BC_INT_SHORT_ZERO) << 16)
                                          + 256 * Read() + Read()).ToString();

                case 'I':
                case CHessian2Constants.BC_LONG_INT:
                    return (ParseInt()).ToString();

                // direct long
                case 0xd8:
                case 0xd9:
                case 0xda:
                case 0xdb:
                case 0xdc:
                case 0xdd:
                case 0xde:
                case 0xdf:

                case 0xe0:
                case 0xe1:
                case 0xe2:
                case 0xe3:
                case 0xe4:
                case 0xe5:
                case 0xe6:
                case 0xe7:
                case 0xe8:
                case 0xe9:
                case 0xea:
                case 0xeb:
                case 0xec:
                case 0xed:
                case 0xee:
                case 0xef:
                    return (tag - CHessian2Constants.BC_LONG_ZERO).ToString();

                /* byte long */
                case 0xf0:
                case 0xf1:
                case 0xf2:
                case 0xf3:
                case 0xf4:
                case 0xf5:
                case 0xf6:
                case 0xf7:
                case 0xf8:
                case 0xf9:
                case 0xfa:
                case 0xfb:
                case 0xfc:
                case 0xfd:
                case 0xfe:
                case 0xff:
                    return (((tag - CHessian2Constants.BC_LONG_BYTE_ZERO) << 8) + Read()).ToString();

                /* short long */
                case 0x38:
                case 0x39:
                case 0x3a:
                case 0x3b:
                case 0x3c:
                case 0x3d:
                case 0x3e:
                case 0x3f:
                    return (((tag - CHessian2Constants.BC_LONG_SHORT_ZERO) << 16)
                                          + 256 * Read() + Read()).ToString();

                case 'L':
                    return (ParseLong()).ToString();

                case CHessian2Constants.BC_DOUBLE_ZERO:
                    return "0.0";

                case CHessian2Constants.BC_DOUBLE_ONE:
                    return "1.0";

                case CHessian2Constants.BC_DOUBLE_BYTE:
                    //因为Java中的byte取值范围是-128~127，C#中的byte的取值范围是0~255，Java byte在C#中对应sbyte,所有要转成sbyte.
                    return ((sbyte)(_offset < _length
                                                  ? _buffer[_offset++]
                                                  : Read())).ToString();

                case CHessian2Constants.BC_DOUBLE_SHORT:
                    return (((short)(256 * Read() + Read()))).ToString();

                case CHessian2Constants.BC_DOUBLE_MILL:
                    {
                        int mills = ParseInt();

                        return (0.001 * mills).ToString(CultureInfo.InvariantCulture);
                    }

                case 'D':
                    return (ParseDouble()).ToString(CultureInfo.InvariantCulture);

                case 'S':
                case CHessian2Constants.BC_STRING_CHUNK:
                    _isLastChunk = tag == 'S';
                    _chunkLength = (Read() << 8) + Read();

                    _sbuf.Length = 0;
                    int ch;

                    while ((ch = ParseChar()) >= 0)
                        _sbuf.Append((char)ch);

                    return _sbuf.ToString();

                // 0-byte string
                case 0x00:
                case 0x01:
                case 0x02:
                case 0x03:
                case 0x04:
                case 0x05:
                case 0x06:
                case 0x07:
                case 0x08:
                case 0x09:
                case 0x0a:
                case 0x0b:
                case 0x0c:
                case 0x0d:
                case 0x0e:
                case 0x0f:

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
                    _isLastChunk = true;
                    _chunkLength = tag - 0x00;

                    _sbuf.Length = 0;

                    while ((ch = ParseChar()) >= 0)
                    {
                        _sbuf.Append((char)ch);
                    }

                    return _sbuf.ToString();

                case 0x30:
                case 0x31:
                case 0x32:
                case 0x33:
                    _isLastChunk = true;
                    _chunkLength = (tag - 0x30) * 256 + Read();

                    _sbuf.Length = 0;

                    while ((ch = ParseChar()) >= 0)
                        _sbuf.Append((char)ch);

                    return _sbuf.ToString();

                default:
                    throw Expect("string", tag);
            }
        }

        /// <summary>
        /// Reads a byte array from the stream.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public int ReadString(char[] buffer, int offset, int length)
        {
            int readLength = 0;

            if (_chunkLength == CHessian2Constants.END_OF_DATA)
            {
                _chunkLength = 0;
                return -1;
            }
            else if (_chunkLength == 0)
            {
                int tag = Read();

                switch (tag)
                {
                    case 'N':
                        return -1;

                    case 'S':
                    case CHessian2Constants.BC_STRING_CHUNK:
                        _isLastChunk = tag == 'S';
                        _chunkLength = (Read() << 8) + Read();
                        break;

                    case 0x00:
                    case 0x01:
                    case 0x02:
                    case 0x03:
                    case 0x04:
                    case 0x05:
                    case 0x06:
                    case 0x07:
                    case 0x08:
                    case 0x09:
                    case 0x0a:
                    case 0x0b:
                    case 0x0c:
                    case 0x0d:
                    case 0x0e:
                    case 0x0f:

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
                        _isLastChunk = true;
                        _chunkLength = tag - 0x00;
                        break;

                    case 0x30:
                    case 0x31:
                    case 0x32:
                    case 0x33:
                        _isLastChunk = true;
                        _chunkLength = (tag - 0x30) * 256 + Read();
                        break;

                    default:
                        throw Expect("string", tag);
                }
            }

            while (length > 0)
            {
                if (_chunkLength > 0)
                {
                    buffer[offset++] = (char)ParseUTF8Char();
                    _chunkLength--;
                    length--;
                    readLength++;
                }
                else if (_isLastChunk)
                {
                    if (readLength == 0)
                        return -1;
                    else
                    {
                        _chunkLength = CHessian2Constants.END_OF_DATA;
                        return readLength;
                    }
                }
                else
                {
                    int tag = Read();

                    switch (tag)
                    {
                        case 'S':
                        case CHessian2Constants.BC_STRING_CHUNK:
                            _isLastChunk = tag == 'S';
                            _chunkLength = (Read() << 8) + Read();
                            break;

                        case 0x00:
                        case 0x01:
                        case 0x02:
                        case 0x03:
                        case 0x04:
                        case 0x05:
                        case 0x06:
                        case 0x07:
                        case 0x08:
                        case 0x09:
                        case 0x0a:
                        case 0x0b:
                        case 0x0c:
                        case 0x0d:
                        case 0x0e:
                        case 0x0f:

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
                            _isLastChunk = true;
                            _chunkLength = tag - 0x00;
                            break;

                        case 0x30:
                        case 0x31:
                        case 0x32:
                        case 0x33:
                            _isLastChunk = true;
                            _chunkLength = (tag - 0x30) * 256 + Read();
                            break;

                        default:
                            throw Expect("string", tag);
                    }
                }
            }

            if (readLength == 0)
                return -1;
            else if (_chunkLength > 0 || !_isLastChunk)
                return readLength;
            else
            {
                _chunkLength = CHessian2Constants.END_OF_DATA;
                return readLength;
            }
        }

        /// <summary>
        /// * Parses a type from the stream.
        /// *
        /// * <code>
        /// * type ::= string
        /// * type ::= int
        /// * </pre>
        /// </summary>
        /// <returns></returns>
        public override string ReadType()
        {
            int code = _offset < _length ? (_buffer[_offset++] & 0xff) : Read();
            _offset--;

            switch (code)
            {
                case 0x00:
                case 0x01:
                case 0x02:
                case 0x03:
                case 0x04:
                case 0x05:
                case 0x06:
                case 0x07:
                case 0x08:
                case 0x09:
                case 0x0a:
                case 0x0b:
                case 0x0c:
                case 0x0d:
                case 0x0e:
                case 0x0f:

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

                case 0x30:
                case 0x31:
                case 0x32:
                case 0x33:
                case CHessian2Constants.BC_STRING_CHUNK:
                case 'S':
                    {
                        String type = ReadString();

                        if (_types == null)
                            _types = new List<string>();

                        _types.Add(type);

                        return type;
                    }

                default:
                    {
                        int iref = ReadInt();

                        if (_types.Count <= iref)
                            throw new CHessianException("type ref #" + iref + " is greater than the number of valid types (" + _types.Count + ")");

                        return (String)_types[iref];
                    }
            }
        }

        /// <summary>
        /// * Reads a date.
        /// *
        /// * <code>
        /// * T b64 b56 b48 b40 b32 b24 b16 b8
        /// * </pre>
        /// </summary>
        /// <returns></returns>
        public override long ReadUTCDate()
        {
            int tag = Read();

            if (tag == CHessian2Constants.BC_DATE)
            {
                return ParseLong();
            }
            else if (tag == CHessian2Constants.BC_DATE_MINUTE)
            {
                return ParseInt() * 60000L;
            }
            else
                throw Expect("date", tag);
        }

        /// <summary>
        /// * Starts reading the call, including the headers.
        /// *
        /// * <p>The call expects the following protocol data
        /// *
        /// * <code>
        /// * c major minor
        /// * m b16 b8 method
        /// * </pre>
        /// </summary>
        public override void StartCall()
        {
            ReadCall();

            ReadMethod();
        }

        public Object[] ReadArguments()
        {
            int len = ReadInt();

            Object[] args = new Object[len];

            for (int i = 0; i < len; i++)
                args[i] = ReadObject();

            return args;
        }

        /// <summary>
        /// * Starts reading the reply
        /// *
        /// * <p>A successful completion will have a single value:
        /// *
        /// * <code>
        /// * r
        /// * </pre>
        /// </summary>
        public override void StartReply()
        {
            // XXX: for variable length (?)

            ReadReply(typeof(object));
        }

        /// <summary>
        /// Gets the serializer factory.
        /// </summary>
        protected CSerializerFactory FindSerializerFactory()
        {
            return m_serializerFactory;
        }

        /// <summary>
        /// * Parses a 32-bit integer value from the stream.
        /// *
        /// * <code>
        /// * b32 b24 b16 b8
        /// * </pre>
        /// </summary>
        /// <returns></returns>
        private int ParseInt()
        {
            int offset = _offset;

            if (offset + 3 < _length)
            {
                byte[] buffer = _buffer;

                int b32 = buffer[offset + 0] & 0xff;
                int b24 = buffer[offset + 1] & 0xff;
                int b16 = buffer[offset + 2] & 0xff;
                int b8 = buffer[offset + 3] & 0xff;

                _offset = offset + 4;

                return (b32 << 24) + (b24 << 16) + (b16 << 8) + b8;
            }
            else
            {
                int b32 = Read();
                int b24 = Read();
                int b16 = Read();
                int b8 = Read();

                return (b32 << 24) + (b24 << 16) + (b16 << 8) + b8;
            }
        }

        /// <summary>
        /// * Parses a 64-bit long value from the stream.
        /// *
        /// * <code>
        /// * b64 b56 b48 b40 b32 b24 b16 b8
        /// * </pre>
        /// </summary>
        /// <returns></returns>
        private long ParseLong()
        {
            long b64 = Read();
            long b56 = Read();
            long b48 = Read();
            long b40 = Read();
            long b32 = Read();
            long b24 = Read();
            long b16 = Read();
            long b8 = Read();

            return ((b64 << 56)
                    + (b56 << 48)
                    + (b48 << 40)
                    + (b40 << 32)
                    + (b32 << 24)
                    + (b24 << 16)
                    + (b16 << 8)
                    + b8);
        }

        /// <summary>
        /// * Parses a 64-bit double value from the stream.
        /// *
        /// * <code>
        /// * b64 b56 b48 b40 b32 b24 b16 b8
        /// * </pre>
        /// </summary>
        /// <returns></returns>
        private double ParseDouble()
        {
            long bits = ParseLong();

            byte[] lngBytes = BitConverter.GetBytes(bits);
            return BitConverter.ToDouble(lngBytes, 0);
        }

        private void ParseString(StringBuilder sbuf)
        {
            while (true)
            {
                if (_chunkLength <= 0)
                {
                    if (!ParseChunkLength())
                        return;
                }

                int length = _chunkLength;
                _chunkLength = 0;

                while (length-- > 0)
                {
                    sbuf.Append((char)ParseUTF8Char());
                }
            }
        }

        /// <summary>
        /// Reads a character from the underlying stream.
        /// </summary>
        /// <returns></returns>
        private int ParseChar()
        {
            while (_chunkLength <= 0)
            {
                if (!ParseChunkLength())
                    return -1;
            }

            _chunkLength--;

            return ParseUTF8Char();
        }

        /// <summary>
        /// Parses a single UTF8 character.
        /// </summary>
        /// <returns></returns>
        private int ParseUTF8Char()
        {
            int ch = _offset < _length ? (_buffer[_offset++] & 0xff) : Read();

            if (ch < 0x80)
                return ch;
            else if ((ch & 0xe0) == 0xc0)
            {
                int ch1 = Read();
                int v = ((ch & 0x1f) << 6) + (ch1 & 0x3f);

                return v;
            }
            else if ((ch & 0xf0) == 0xe0)
            {
                int ch1 = Read();
                int ch2 = Read();
                int v = ((ch & 0x0f) << 12) + ((ch1 & 0x3f) << 6) + (ch2 & 0x3f);

                return v;
            }
            else
                throw Error("bad utf-8 encoding at " + CodeName(ch));
        }

        private bool ParseChunkLength()
        {
            if (_isLastChunk)
                return false;

            int code = _offset < _length ? (_buffer[_offset++] & 0xff) : Read();

            switch (code)
            {
                case CHessian2Constants.BC_STRING_CHUNK:
                    _isLastChunk = false;

                    _chunkLength = (Read() << 8) + Read();
                    break;

                case 'S':
                    _isLastChunk = true;

                    _chunkLength = (Read() << 8) + Read();
                    break;

                case 0x00:
                case 0x01:
                case 0x02:
                case 0x03:
                case 0x04:
                case 0x05:
                case 0x06:
                case 0x07:
                case 0x08:
                case 0x09:
                case 0x0a:
                case 0x0b:
                case 0x0c:
                case 0x0d:
                case 0x0e:
                case 0x0f:

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
                    _isLastChunk = true;
                    _chunkLength = code - 0x00;
                    break;

                case 0x30:
                case 0x31:
                case 0x32:
                case 0x33:
                    _isLastChunk = true;
                    _chunkLength = (code - 0x30) * 256 + Read();
                    break;

                default:
                    throw Expect("string", code);
            }

            return true;
        }

        /// <summary>
        /// Reads a byte from the underlying stream.
        /// </summary>
        /// <returns></returns>
        private int ParseByte()
        {
            while (_chunkLength <= 0)
            {
                if (_isLastChunk)
                {
                    return -1;
                }

                int code = Read();

                switch (code)
                {
                    case CHessian2Constants.BC_BINARY_CHUNK:
                        _isLastChunk = false;

                        _chunkLength = (Read() << 8) + Read();
                        break;

                    case 'B':
                        _isLastChunk = true;

                        _chunkLength = (Read() << 8) + Read();
                        break;

                    case 0x20:
                    case 0x21:
                    case 0x22:
                    case 0x23:
                    case 0x24:
                    case 0x25:
                    case 0x26:
                    case 0x27:
                    case 0x28:
                    case 0x29:
                    case 0x2a:
                    case 0x2b:
                    case 0x2c:
                    case 0x2d:
                    case 0x2e:
                    case 0x2f:
                        _isLastChunk = true;

                        _chunkLength = code - 0x20;
                        break;

                    case 0x34:
                    case 0x35:
                    case 0x36:
                    case 0x37:
                        _isLastChunk = true;
                        _chunkLength = (code - 0x34) * 256 + Read();
                        break;

                    default:
                        throw Expect("byte[]", code);
                }
            }

            _chunkLength--;

            return Read();
        }

        /// <summary>
        /// Reads bytes from the underlying stream.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        private int Read(byte[] buffer, int offset, int length)
        {
            int readLength = 0;

            while (length > 0)
            {
                while (_chunkLength <= 0)
                {
                    if (_isLastChunk)
                        return readLength == 0 ? -1 : readLength;

                    int code = Read();

                    switch (code)
                    {
                        case CHessian2Constants.BC_BINARY_CHUNK:
                            _isLastChunk = false;

                            _chunkLength = (Read() << 8) + Read();
                            break;

                        case CHessian2Constants.BC_BINARY:
                            _isLastChunk = true;

                            _chunkLength = (Read() << 8) + Read();
                            break;

                        case 0x20:
                        case 0x21:
                        case 0x22:
                        case 0x23:
                        case 0x24:
                        case 0x25:
                        case 0x26:
                        case 0x27:
                        case 0x28:
                        case 0x29:
                        case 0x2a:
                        case 0x2b:
                        case 0x2c:
                        case 0x2d:
                        case 0x2e:
                        case 0x2f:
                            _isLastChunk = true;
                            _chunkLength = code - 0x20;
                            break;

                        case 0x34:
                        case 0x35:
                        case 0x36:
                        case 0x37:
                            _isLastChunk = true;
                            _chunkLength = (code - 0x34) * 256 + Read();
                            break;

                        default:
                            throw Expect("byte[]", code);
                    }
                }

                int sublen = _chunkLength;
                if (length < sublen)
                    sublen = length;

                if (_length <= _offset && !ReadBuffer())
                    return -1;

                if (_length - _offset < sublen)
                    sublen = _length - _offset;

                Array.Copy(_buffer, _offset, buffer, offset, sublen);

                _offset += sublen;

                offset += sublen;
                readLength += sublen;
                length -= sublen;
                _chunkLength -= sublen;
            }

            return readLength;
        }

        /// <summary>
        /// * Normally, shouldn't be called externally, but needed for QA, e.g.
        /// * ejb/3b01.
        /// </summary>
        /// <returns></returns>
        public int Read()
        {
            if (_length <= _offset && !ReadBuffer())
                return -1;

            return _buffer[_offset++] & 0xff;
        }

        protected void Unread()
        {
            if (_offset <= 0)
                throw new InvalidOperationException();

            _offset--;
        }

        private bool ReadBuffer()
        {
            byte[] buffer = _buffer;
            int offset = _offset;
            int length = _length;

            if (offset < length)
            {
                Array.Copy(buffer, offset, buffer, 0, length - offset);
                offset = length - offset;
            }
            else
                offset = 0;

            int len = _is.Read(buffer, offset, SIZE - offset);

            if (len <= 0)
            {
                _length = offset;
                _offset = 0;

                return offset > 0;
            }

            _length = offset + len;
            _offset = 0;

            return true;
        }

        public void Reset()
        {
            ResetReferences();

            _classDefs.Clear();
            _types.Clear();
        }


        public void ResetBuffer()
        {
            int offset = _offset;
            _offset = 0;

            int length = _length;
            _length = 0;

            if (length > 0 && offset != length)
                throw new InvalidOperationException("offset=" + offset + " length=" + length);
        }

        private String buildDebugContext(byte[] buffer, int offset, int length,
            int errorOffset)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("[");
            for (int i = 0; i < errorOffset; i++)
            {
                int ch = buffer[offset + i];
                AddDebugChar(sb, ch);
            }
            sb.Append("] ");
            AddDebugChar(sb, buffer[offset + errorOffset]);
            sb.Append(" [");
            for (int i = errorOffset + 1; i < length; i++)
            {
                int ch = buffer[offset + i];
                AddDebugChar(sb, ch);
            }
            sb.Append("]");

            return sb.ToString();
        }

        private void AddDebugChar(StringBuilder sb, int ch)
        {
            if (ch >= 0x20 && ch < 0x7f)
            {
                sb.Append((char)ch);
            }
            else if (ch == '\n')
                sb.Append((char)ch);
            else
                sb.Append(string.Format("\\x{0:x2}", ch & 0xff));
        }

        protected String CodeName(int ch)
        {
            if (ch < 0)
                return "end of file";
            else
                return "0x" + string.Format("{0:x2}", (ch & 0xff)) + " (" + (char)+ch + ")";
        }

        protected IOException Expect(String expect, int ch)
        {
            if (ch < 0)
                return Error("expected " + expect + " at end of file");
            else
            {
                _offset--;

                try
                {
                    int offset = _offset;
                    String context
                        = buildDebugContext(_buffer, 0, _length, offset);

                    Object obj = ReadObject();

                    if (obj != null)
                    {
                        return Error("expected " + expect
                                                 + " at 0x" + string.Format("{0:x2}", ch & 0xff)
                                                 + " " + obj.GetType().Name + " (" + obj + ")"
                                                 + "\n  " + context + "");
                    }
                    else
                        return Error("expected " + expect
                                                 + " at 0x" + string.Format("{0:x2}", ch & 0xff) + " null");
                }
                catch (Exception e)
                {

                    return Error("expected " + expect
                                             + " at 0x" + string.Format("{0:x2}", (ch & 0xff)));
                }
            }
        }

        protected IOException Error(String message)
        {
            if (_method != null)
                return new IOException(_method + ": " + message);
            else
                return new IOException(message);
        }

        /// <summary>
        /// Prepares the fault.
        /// </summary>
        /// <param name="fault"></param>
        /// <returns></returns>
        private Exception PrepareFault(Hashtable fault)
        {
            Object detail = fault["detail"];
            String message = (String)fault["message"];

            if (detail is Exception)
            {
                _replyFault = (Exception)detail;

                if (message != null && _detailMessageField != null)
                {
                    try
                    {
                        ReflectionUtils.SetMemberValue(_detailMessageField, _replyFault, message);
                    }
                    catch (Exception e)
                    {
                    }
                }

                return _replyFault;
            }

            else
            {
                String code = (String)fault["code"];

                _replyFault = new CHessianException(message, "Code:" + code + "\r\n" + detail?.ToString());

                return _replyFault;
            }
        }
    }

    public class ObjectDefinition
    {
        private String _type;
        private IDeserializer _reader;
        private Object[] _fields;
        private String[] _fieldNames;

        public ObjectDefinition(String type, IDeserializer reader, Object[] fields, String[] fieldNames)
        {
            _type = type;
            _reader = reader;
            _fields = fields;
            _fieldNames = fieldNames;
        }

        public String GetTypeName()
        {
            return _type;
        }

        public IDeserializer GetReader()
        {
            return _reader;
        }

        public object[] GetFields()
        {
            return _fields;
        }

        public String[] GetFieldNames()
        {
            return _fieldNames;
        }
    }
}
