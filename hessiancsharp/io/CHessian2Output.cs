using System;
using System.Collections.Generic;
using System.IO;

namespace HessianCSharp.io
{
    public class CHessian2Output : AbstractHessianOutput
    {
        // should match Resin buffer size for perf
        public static int SIZE = 8 * 1024;

        // the output stream/
        protected Stream _os;

        // map of references
        private IdentityIntMap _refs = new IdentityIntMap(256);

        private int _refCount = 0;

        private bool _isCloseStreamOnClose;

        // map of classes
        private IdentityIntMap _classRefs = new IdentityIntMap(256);

        // map of types
        private Dictionary<String, int> _typeRefs;

        private byte[] _buffer = new byte[SIZE];
        private int _offset;

        private bool _isPacket;

        private bool _isUnshared;

        public CHessian2Output(Stream outStream)
        {
            Init(outStream);
        }

        public override void Init(Stream stream)
        {
            Reset();

            _os = stream;
        }

        /// <summary>
        /// * If the object has already been written, just write its ref.
        /// *
        /// * @return true if we're writing a ref.
        /// </summary>
        public override bool AddRef(object objReference)
        {
            if (_isUnshared)
            {
                _refCount++;
                return false;
            }

            int newRef = _refCount;

            int iref = AddRef(objReference, newRef, false);

            if (iref != newRef)
            {
                WriteRef(iref);

                return true;
            }
            else
            {
                _refCount++;

                return false;
            }
        }

        private int AddRef(Object value, int newRef, bool isReplace)
        {
            int prevRef = _refs.Put(value, newRef, isReplace);

            return prevRef;
        }

        public override int GetRef(object obj)
        {
            if (_isUnshared)
                return -1;

            return _refs.Get(obj);
        }

        /// <summary>
        /// * Writes the method tag.
        /// *
        /// * <code><pre>
        /// * string
        /// * </code></code>
        /// *
        /// * @param method the method name to call.
        /// </summary>
        public override void WriteMethod(string method)
        {
            WriteString(method);
        }

        /// <summary>
        /// * Completes.
        /// *
        /// * <code><pre>
        /// * z
        /// * </code></code>
        /// </summary>
        public override void CompleteCall()
        {
        }

        /// <summary>
        /// * Completes reading the reply
        /// *
        /// * <p>A successful completion will have a single value:
        /// *
        /// * <pre>
        /// * z
        /// * </pre>
        /// </summary>
        public override void CompleteReply()
        {
        }


        /// <summary>
        /// * Starts a packet
        /// *
        /// * <p>A message contains several objects encapsulated by a length</p>
        /// *
        /// * <pre>
        /// * p x02 x00
        /// * </pre>
        /// </summary>
        public void StartMessage()
        {
            FlushIfFull();

            _buffer[_offset++] = (byte)'p';
            _buffer[_offset++] = (byte)2;
            _buffer[_offset++] = (byte)0;
        }


        /// <summary>
        /// * Completes reading the message
        /// *
        /// * <p>A successful completion will have a single value:
        /// *
        /// * <pre>
        /// * z
        /// * </pre>
        /// </summary>
        public void CompleteMessage()
        {
            FlushIfFull();

            _buffer[_offset++] = (byte)'z';
        }

        /// <summary>
        /// Removes a reference.
        /// </summary>
        public override bool RemoveRef(object objReference)
        {
            if (_isUnshared)
            {
                return false;
            }
            else if (_refs != null)
            {
                _refs.Remove(objReference);

                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Replaces a reference from one object to another.
        /// </summary>
        public override bool ReplaceRef(object objOldReference, object objNewReference)
        {
            if (_isUnshared)
            {
                return false;
            }

            int value = _refs.Get(objOldReference);

            if (value >= 0)
            {
                AddRef(objNewReference, value, true);

                _refs.Remove(objOldReference);

                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Writes a complete method call.
        /// </summary>
        public override void Call(String method, Object[] args)
        {
            WriteVersion();

            int length = args != null ? args.Length : 0;

            StartCall(method, length);

            for (int i = 0; i < length; i++)
            {
                WriteObject(args[i]);
            }

            CompleteCall();

            Flush();
        }

        /// <summary>
        /// * Writes the call tag.  This would be followed by the
        /// * method and the arguments
        /// *
        /// * <code><pre>
        /// * C
        /// * </code></code>
        /// *
        /// * @param method the method name to call.
        /// </summary>
        public override void StartCall()
        {
            FlushIfFull();

            _buffer[_offset++] = (byte)'C';
        }

        public override void StartCall(string strMethod)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// * Starts the method call.  Clients would use <code>startCall</code>
        /// * instead of <code>call</code> if they wanted finer control over
        /// * writing the arguments, or needed to write headers.
        /// *
        /// * <code><pre>
        /// * C
        /// * string # method name
        /// * int    # arg count
        /// * </code></code>
        /// *
        /// * @param method the method name to call.
        /// </summary>
        public override void StartCall(string method, int length)
        {
            int offset = _offset;

            if (SIZE < offset + 32)
            {
                FlushBuffer();
                offset = _offset;
            }

            byte[] buffer = _buffer;

            buffer[_offset++] = (byte)'C';

            WriteString(method);
            WriteInt(length);
        }


        /// <summary>
        ///          /**
        /// * Starts an envelope.
        /// *
        /// * <code><pre>
        /// * E major minor
        /// * m b16 b8 method-name
        /// * </code></code>
        /// *
        /// * @param method the method name to call.
        /// </summary>
        public void StartEnvelope(String method)
        {
            int offset = _offset;

            if (SIZE < offset + 32)
            {
                FlushBuffer();
                offset = _offset;
            }

            _buffer[_offset++] = (byte)'E';

            WriteString(method);
        }


        /// <summary>
        /// * Completes an envelope.
        /// *
        /// * <p>A successful completion will have a single value:
        /// *
        /// * <pre>
        /// * Z
        /// * </pre>
        /// </summary>
        public void CompleteEnvelope()
        {
            FlushIfFull();

            _buffer[_offset++] = (byte)'Z';
        }

        /// <summary>
        /// * Starts the reply
        /// *
        /// * <p>A successful completion will have a single value:
        /// *
        /// * <pre>
        /// * R
        /// * </pre>
        /// </summary>
        public override void StartReply()
        {
            WriteVersion();

            FlushIfFull();

            _buffer[_offset++] = (byte)'R';
        }

        /// <summary>
        /// * Writes a boolean value to the stream.  The boolean will be written
        /// * with the following syntax:
        /// *
        /// * <code><pre>
        /// * T
        /// * F
        /// * </code></code>
        /// *
        /// * @param value the boolean value to write.
        /// </summary>
        public override void WriteBoolean(bool blnValue)
        {
            if (SIZE < _offset + 16)
                FlushBuffer();

            if (blnValue)
                _buffer[_offset++] = (byte)'T';
            else
                _buffer[_offset++] = (byte)'F';
        }

        /// <summary>
        /// * Writes a byte buffer to the stream.
        /// *
        /// * <code><pre>
        /// * b b16 b18 bytes
        /// * </code></code>
        /// </summary>
        public override void WriteByteBufferEnd(byte[] arrbuffer, int intOffset, int intLength)
        {
            WriteBytes(arrbuffer, intOffset, intLength);
        }

        /// <summary>
        /// * Writes a byte buffer to the stream.
        /// *
        /// * <code><pre>
        /// * b b16 b18 bytes
        /// * </code></code>
        /// </summary>
        public override void WriteByteBufferPart(byte[] arrBuffer, int intOffset, int intLength)
        {
            while (intLength > 0)
            {
                FlushIfFull();

                int sublen = _buffer.Length - _offset;

                if (intLength < sublen)
                    sublen = intLength;

                _buffer[_offset++] = CHessian2Constants.BC_BINARY_CHUNK;
                _buffer[_offset++] = (byte)(sublen >> 8);
                _buffer[_offset++] = (byte)sublen;

                Array.Copy(arrBuffer, intOffset, _buffer, _offset, sublen);

                _offset += sublen;
                intLength -= sublen;
                intOffset += sublen;
            }
        }

        /// <summary>
        /// * Writes a byte array to the stream.
        /// * The array will be written with the following syntax:
        /// *
        /// * <code><pre>
        /// * B b16 b18 bytes
        /// * </code></code>
        /// *
        /// * If the value is null, it will be written as
        /// *
        /// * <code><pre>
        /// * N
        /// * </code></code>
        /// *
        /// * @param value the string value to write.
        /// </summary>
        /// <param name="arrBuffer"></param>
        public override void WriteBytes(byte[] arrBuffer)
        {
            if (arrBuffer == null)
            {
                if (SIZE < _offset + 16)
                    FlushBuffer();

                _buffer[_offset++] = (byte)'N';
            }
            else
                WriteBytes(arrBuffer, 0, arrBuffer.Length);
        }

        /// <summary>
        /// * Writes a byte array to the stream.
        /// * The array will be written with the following syntax:
        /// *
        /// * <code><pre>
        /// * B b16 b18 bytes
        /// * </code></code>
        /// *
        /// * If the value is null, it will be written as
        /// *
        /// * <code><pre>
        /// * N
        /// * </code></code>
        /// *
        /// * @param value the string value to write.
        /// </summary>
        public override void WriteBytes(byte[] buffer, int offset, int length)
        {
            if (buffer == null)
            {
                if (SIZE < _offset + 16)
                    FlushBuffer();

                _buffer[_offset++] = (byte)'N';
            }
            else
            {
                while (SIZE - _offset - 3 < length)
                {
                    int sublen = SIZE - _offset - 3;

                    if (sublen < 16)
                    {
                        FlushBuffer();

                        sublen = SIZE - _offset - 3;

                        if (length < sublen)
                            sublen = length;
                    }

                    _buffer[_offset++] = (byte)CHessian2Constants.BC_BINARY_CHUNK;
                    _buffer[_offset++] = (byte)(sublen >> 8);
                    _buffer[_offset++] = (byte)sublen;

                    Array.Copy(buffer, offset, _buffer, _offset, sublen);
                    _offset += sublen;

                    length -= sublen;
                    offset += sublen;

                    FlushBuffer();
                }

                if (SIZE < _offset + 16)
                    FlushBuffer();

                if (length <= CHessian2Constants.BINARY_DIRECT_MAX)
                {
                    _buffer[_offset++] = (byte)(CHessian2Constants.BC_BINARY_DIRECT + length);
                }
                else if (length <= CHessian2Constants.BINARY_SHORT_MAX)
                {
                    _buffer[_offset++] = (byte)(CHessian2Constants.BC_BINARY_SHORT + (length >> 8));
                    _buffer[_offset++] = (byte)(length);
                }
                else
                {
                    _buffer[_offset++] = (byte)'B';
                    _buffer[_offset++] = (byte)(length >> 8);
                    _buffer[_offset++] = (byte)(length);
                }

                Array.Copy(buffer, offset, _buffer, _offset, length);

                _offset += length;
            }
        }

        /// <summary>
        /// * Writes a byte buffer to the stream.
        /// *
        /// * <code><pre>
        /// * </code></code>
        /// </summary>
        public void WriteByteBufferStart()
        {
        }

        //public override void WriteDBNull()
        //{
        //    int offset = _offset;
        //    byte[] buffer = _buffer;
        //    buffer[offset++] = (byte)CHessian2Constants.PROT_DBNULL_TYPE;
        //    _offset = offset;
        //}

        //public override void WriteDecimal(decimal decimalValue)
        //{
        //    int offset = _offset;
        //    byte[] buffer = _buffer;
        //    buffer[offset++] = (byte)CHessian2Constants.PROT_DECIMAL_TYPE;
        //    _offset = offset;
        //    this.WriteString(decimalValue.ToString("G29"));
        //}

        /// <summary>
        /// * Writes a double value to the stream.  The double will be written
        /// * with the following syntax:
        /// *
        /// * <code><pre>
        /// * D b64 b56 b48 b40 b32 b24 b16 b8
        /// * </code></code>
        /// *
        /// * @param value the double value to write.
        /// </summary>
        public override void WriteDouble(double value)
        {
            int offset = _offset;
            byte[] buffer = _buffer;

            if (SIZE <= offset + 16)
            {
                FlushBuffer();
                offset = _offset;
            }

            int intValue = (int)value;

            if (intValue == value)
            {
                if (intValue == 0)
                {
                    buffer[offset++] = (byte)CHessian2Constants.BC_DOUBLE_ZERO;

                    _offset = offset;

                    return;
                }
                else if (intValue == 1)
                {
                    buffer[offset++] = (byte)CHessian2Constants.BC_DOUBLE_ONE;

                    _offset = offset;

                    return;
                }
                else if (-0x80 <= intValue && intValue < 0x80)
                {
                    buffer[offset++] = (byte)CHessian2Constants.BC_DOUBLE_BYTE;
                    buffer[offset++] = (byte)intValue;

                    _offset = offset;

                    return;
                }
                else if (-0x8000 <= intValue && intValue < 0x8000)
                {
                    buffer[offset + 0] = (byte)CHessian2Constants.BC_DOUBLE_SHORT;
                    buffer[offset + 1] = (byte)(intValue >> 8);
                    buffer[offset + 2] = (byte)intValue;

                    _offset = offset + 3;

                    return;
                }
            }

            int mills = (int)(value * 1000);

            if (0.001 * mills == value)
            {
                buffer[offset + 0] = (byte)(CHessian2Constants.BC_DOUBLE_MILL);
                buffer[offset + 1] = (byte)(mills >> 24);
                buffer[offset + 2] = (byte)(mills >> 16);
                buffer[offset + 3] = (byte)(mills >> 8);
                buffer[offset + 4] = (byte)(mills);

                _offset = offset + 5;

                return;
            }

            long bits = BitConverter.DoubleToInt64Bits(value);

            buffer[offset + 0] = (byte)'D';
            buffer[offset + 1] = (byte)(bits >> 56);
            buffer[offset + 2] = (byte)(bits >> 48);
            buffer[offset + 3] = (byte)(bits >> 40);
            buffer[offset + 4] = (byte)(bits >> 32);
            buffer[offset + 5] = (byte)(bits >> 24);
            buffer[offset + 6] = (byte)(bits >> 16);
            buffer[offset + 7] = (byte)(bits >> 8);
            buffer[offset + 8] = (byte)(bits);

            _offset = offset + 9;
        }

        /// <summary>
        /// * Writes a fault.  The fault will be written
        /// * as a descriptive string followed by an object:
        /// *
        /// * <code><pre>
        /// * F map
        /// * </code></code>
        /// *
        /// * <code><pre>
        /// * F H
        /// * \x04code
        /// * \x10the fault code
        /// *
        /// * \x07message
        /// * \x11the fault message
        /// *
        /// * \x06detail
        /// * M\xnnjavax.ejb.FinderException
        /// *     ...
        /// * Z
        /// * Z
        /// * </code></code>
        /// *
        /// * @param code the fault code, a three digit
        /// </summary>
        public override void WriteFault(string code, string message, object detail)
        {
            FlushIfFull();

            WriteVersion();

            _buffer[_offset++] = (byte)'F';
            _buffer[_offset++] = (byte)'H';

            AddRef(new Object(), _refCount++, false);

            WriteString("code");
            WriteString(code);

            WriteString("message");
            WriteString(message);

            if (detail != null)
            {
                WriteString("detail");
                WriteObject(detail);
            }

            FlushIfFull();
            _buffer[_offset++] = (byte)'Z';
        }

        /// <summary>
        /// * Writes an integer value to the stream.  The integer will be written
        /// * with the following syntax:
        /// *
        /// * <code><pre>
        /// * I b32 b24 b16 b8
        /// * </code></code>
        /// *
        /// * @param value the integer value to write.
        /// </summary>
        public override void WriteInt(int value)
        {
            int offset = _offset;
            byte[] buffer = _buffer;

            if (SIZE <= offset + 16)
            {
                FlushBuffer();
                offset = _offset;
            }

            if (CHessian2Constants.INT_DIRECT_MIN <= value && value <= CHessian2Constants.INT_DIRECT_MAX)
                buffer[offset++] = (byte)(value + CHessian2Constants.BC_INT_ZERO);
            else if (CHessian2Constants.INT_BYTE_MIN <= value && value <= CHessian2Constants.INT_BYTE_MAX)
            {
                buffer[offset++] = (byte)(CHessian2Constants.BC_INT_BYTE_ZERO + (value >> 8));
                buffer[offset++] = (byte)(value);
            }
            else if (CHessian2Constants.INT_SHORT_MIN <= value && value <= CHessian2Constants.INT_SHORT_MAX)
            {
                buffer[offset++] = (byte)(CHessian2Constants.BC_INT_SHORT_ZERO + (value >> 16));
                buffer[offset++] = (byte)(value >> 8);
                buffer[offset++] = (byte)(value);
            }
            else
            {
                buffer[offset++] = (byte)('I');
                buffer[offset++] = (byte)(value >> 24);
                buffer[offset++] = (byte)(value >> 16);
                buffer[offset++] = (byte)(value >> 8);
                buffer[offset++] = (byte)(value);
            }

            _offset = offset;
        }

        /// <summary>
        /// * Writes the list header to the stream.  List writers will call
        /// * <code>writeListBegin</code> followed by the list contents and then
        /// * call <code>writeListEnd</code>.
        /// *
        /// * <code><pre>
        /// * list ::= V type value* Z
        /// *      ::= v type int value*
        /// * </code></code>
        /// *
        /// * @return true for variable lists, false for fixed lists
        /// </summary>
        public override bool WriteListBegin(int length, string type)
        {
            FlushIfFull();

            if (length < 0)
            {
                if (type != null)
                {
                    _buffer[_offset++] = (byte)CHessian2Constants.BC_LIST_VARIABLE;
                    WriteType(type);
                }
                else
                    _buffer[_offset++] = (byte)CHessian2Constants.BC_LIST_VARIABLE_UNTYPED;

                return true;
            }
            else if (length <= CHessian2Constants.LIST_DIRECT_MAX)
            {
                if (type != null)
                {
                    _buffer[_offset++] = (byte)(CHessian2Constants.BC_LIST_DIRECT + length);
                    WriteType(type);
                }
                else
                {
                    _buffer[_offset++] = (byte)(CHessian2Constants.BC_LIST_DIRECT_UNTYPED + length);
                }

                return false;
            }
            else
            {
                if (type != null)
                {
                    _buffer[_offset++] = (byte)CHessian2Constants.BC_LIST_FIXED;
                    WriteType(type);
                }
                else
                {
                    _buffer[_offset++] = (byte)CHessian2Constants.BC_LIST_FIXED_UNTYPED;
                }

                WriteInt(length);

                return false;
            }
        }

        public override void WriteListEnd()
        {
            FlushIfFull();

            _buffer[_offset++] = (byte)CHessian2Constants.BC_END;
        }

        /// <summary>
        /// * Writes a long value to the stream.  The long will be written
        /// * with the following syntax:
        /// *
        /// * <code><pre>
        /// * L b64 b56 b48 b40 b32 b24 b16 b8
        /// * </code></code>
        /// *
        /// * @param value the long value to write.
        /// </summary>
        public override void WriteLong(long value)
        {
            int offset = _offset;
            byte[] buffer = _buffer;

            if (SIZE <= offset + 16)
            {
                FlushBuffer();
                offset = _offset;
            }

            if (CHessian2Constants.LONG_DIRECT_MIN <= value && value <= CHessian2Constants.LONG_DIRECT_MAX)
            {
                buffer[offset++] = (byte)(value + CHessian2Constants.BC_LONG_ZERO);
            }
            else if (CHessian2Constants.LONG_BYTE_MIN <= value && value <= CHessian2Constants.LONG_BYTE_MAX)
            {
                buffer[offset++] = (byte)(CHessian2Constants.BC_LONG_BYTE_ZERO + (value >> 8));
                buffer[offset++] = (byte)(value);
            }
            else if (CHessian2Constants.LONG_SHORT_MIN <= value && value <= CHessian2Constants.LONG_SHORT_MAX)
            {
                buffer[offset++] = (byte)(CHessian2Constants.BC_LONG_SHORT_ZERO + (value >> 16));
                buffer[offset++] = (byte)(value >> 8);
                buffer[offset++] = (byte)(value);
            }
            else if (-0x80000000L <= value && value <= 0x7fffffffL)
            {
                buffer[offset + 0] = (byte)CHessian2Constants.BC_LONG_INT;
                buffer[offset + 1] = (byte)(value >> 24);
                buffer[offset + 2] = (byte)(value >> 16);
                buffer[offset + 3] = (byte)(value >> 8);
                buffer[offset + 4] = (byte)(value);

                offset += 5;
            }
            else
            {
                buffer[offset + 0] = (byte)'L';
                buffer[offset + 1] = (byte)(value >> 56);
                buffer[offset + 2] = (byte)(value >> 48);
                buffer[offset + 3] = (byte)(value >> 40);
                buffer[offset + 4] = (byte)(value >> 32);
                buffer[offset + 5] = (byte)(value >> 24);
                buffer[offset + 6] = (byte)(value >> 16);
                buffer[offset + 7] = (byte)(value >> 8);
                buffer[offset + 8] = (byte)(value);

                offset += 9;
            }

            _offset = offset;
        }

        /// <summary>
        /// * Writes the map header to the stream.  Map writers will call
        /// * <code>writeMapBegin</code> followed by the map contents and then
        /// * call <code>writeMapEnd</code>.
        /// *
        /// * <code><pre>
        /// * map ::= M type (<value> <value>)* Z
        /// *     ::= H (<value> <value>)* Z
        /// * </code></code>
        /// </summary>
        public override void WriteMapBegin(string type)
        {
            if (SIZE < _offset + 32)
                FlushBuffer();

            if (type != null)
            {
                _buffer[_offset++] = CHessian2Constants.BC_MAP;

                WriteType(type);
            }
            else
                _buffer[_offset++] = CHessian2Constants.BC_MAP_UNTYPED;
        }

        /// <summary>
        /// Writes the tail of the map to the stream.
        /// </summary>
        public override void WriteMapEnd()
        {
            if (SIZE < _offset + 32)
                FlushBuffer();

            _buffer[_offset++] = (byte)CHessian2Constants.BC_END;
        }

        /// <summary>
        /// * Writes the object definition
        /// *
        /// * <code><pre>
        /// * C &amp;lt;string> &amp;lt;int> &amp;lt;string>*
        /// * </code></code>
        /// </summary>
        public override int WriteObjectBegin(String type)
        {
            int newRef = _classRefs.Size();
            int iref = _classRefs.Put(type, newRef, false);

            if (newRef != iref)
            {
                if (SIZE < _offset + 32)
                    FlushBuffer();

                if (iref <= CHessian2Constants.OBJECT_DIRECT_MAX)
                {
                    _buffer[_offset++] = (byte)(CHessian2Constants.BC_OBJECT_DIRECT + iref);
                }
                else
                {
                    _buffer[_offset++] = (byte)'O';
                    WriteInt(iref);
                }

                return iref;
            }
            else
            {
                if (SIZE < _offset + 32)
                    FlushBuffer();

                _buffer[_offset++] = (byte)'C';

                WriteString(type);

                return -1;
            }
        }

        /// <summary>
        /// Writes the tail of the class definition to the stream.
        /// </summary>
        public override void WriteClassFieldLength(int len)
        {
            WriteInt(len);
        }

        /// <summary>
        /// Writes the tail of the object definition to the stream.
        /// </summary>
        public override void WriteObjectEnd()
        {
        }

        /// <summary>
        /// * Writes a null value to the stream.
        /// * The null will be written with the following syntax
        /// *
        /// * <code><pre>
        /// * N
        /// * </code></code>
        /// *
        /// * @param value the string value to write.
        /// </summary>
        public override void WriteNull()
        {
            int offset = _offset;
            byte[] buffer = _buffer;

            if (SIZE <= offset + 16)
            {
                FlushBuffer();
                offset = _offset;
            }

            buffer[offset++] = (byte)'N';

            _offset = offset;
        }

        /// <summary>
        /// Writes any object to the output stream.
        /// </summary>
        public override void WriteObject(object obj)
        {
            if (obj == null)
            {
                WriteNull();
                return;
            }

            ISerializer serializer = findSerializerFactory().GetObjectSerializer(obj.GetType());

            serializer.WriteObject(obj, this);
        }

        /// <summary>
        /// * Writes a reference.
        /// *
        /// * <code><pre>
        /// * x51 &amp;lt;int>
        /// * </code></code>
        /// *
        /// * @param value the integer value to write.
        /// </summary>
        public override void WriteRef(int intValue)
        {
            if (SIZE < _offset + 16)
                FlushBuffer();

            _buffer[_offset++] = (byte)CHessian2Constants.BC_REF;

            WriteInt(intValue);
        }

        public override void WriteRemote(string strType, string strUrl)
        {
            throw new NotImplementedException();
        }

        public override void WriteString(string value)
        {
            int offset = _offset;
            byte[] buffer = _buffer;

            if (SIZE <= offset + 16)
            {
                FlushBuffer();
                offset = _offset;
            }

            if (value == null)
            {
                buffer[offset++] = (byte)'N';

                _offset = offset;
            }
            else
            {
                int length = value.Length;
                int strOffset = 0;

                while (length > 0x8000)
                {
                    int sublen = 0x8000;

                    offset = _offset;

                    if (SIZE <= offset + 16)
                    {
                        FlushBuffer();
                        offset = _offset;
                    }

                    // chunk can't end in high surrogate
                    char tail = value[strOffset + sublen - 1];

                    if (0xd800 <= tail && tail <= 0xdbff)
                        sublen--;

                    buffer[offset + 0] = (byte)CHessian2Constants.BC_STRING_CHUNK;
                    buffer[offset + 1] = (byte)(sublen >> 8);
                    buffer[offset + 2] = (byte)(sublen);

                    _offset = offset + 3;

                    PrintString(value, strOffset, sublen);

                    length -= sublen;
                    strOffset += sublen;
                }

                offset = _offset;

                if (SIZE <= offset + 16)
                {
                    FlushBuffer();
                    offset = _offset;
                }

                if (length <= CHessian2Constants.STRING_DIRECT_MAX)
                {
                    buffer[offset++] = (byte)(CHessian2Constants.BC_STRING_DIRECT + length);
                }
                else if (length <= CHessian2Constants.STRING_SHORT_MAX)
                {
                    buffer[offset++] = (byte)(CHessian2Constants.BC_STRING_SHORT + (length >> 8));
                    buffer[offset++] = (byte)(length);
                }
                else
                {
                    buffer[offset++] = (byte)('S');
                    buffer[offset++] = (byte)(length >> 8);
                    buffer[offset++] = (byte)(length);
                }

                _offset = offset;

                PrintString(value, strOffset, length);
            }
        }

        public override void WriteString(char[] buffer, int offset, int length)
        {
            if (buffer == null)
            {
                if (SIZE < _offset + 16)
                    FlushBuffer();

                _buffer[_offset++] = (byte)('N');
            }
            else
            {
                while (length > 0x8000)
                {
                    int sublen = 0x8000;

                    if (SIZE < _offset + 16)
                        FlushBuffer();

                    // chunk can't end in high surrogate
                    char tail = buffer[offset + sublen - 1];

                    if (0xd800 <= tail && tail <= 0xdbff)
                        sublen--;

                    _buffer[_offset++] = (byte)CHessian2Constants.BC_STRING_CHUNK;
                    _buffer[_offset++] = (byte)(sublen >> 8);
                    _buffer[_offset++] = (byte)(sublen);

                    PrintString(buffer, offset, sublen);

                    length -= sublen;
                    offset += sublen;
                }

                if (SIZE < _offset + 16)
                    FlushBuffer();

                if (length <= CHessian2Constants.STRING_DIRECT_MAX)
                {
                    _buffer[_offset++] = (byte)(CHessian2Constants.BC_STRING_DIRECT + length);
                }
                else if (length <= CHessian2Constants.STRING_SHORT_MAX)
                {
                    _buffer[_offset++] = (byte)(CHessian2Constants.BC_STRING_SHORT + (length >> 8));
                    _buffer[_offset++] = (byte)length;
                }
                else
                {
                    _buffer[_offset++] = (byte)('S');
                    _buffer[_offset++] = (byte)(length >> 8);
                    _buffer[_offset++] = (byte)(length);
                }

                PrintString(buffer, offset, length);
            }
        }

        /// <summary>
        /// * Writes a date to the stream.
        /// *
        /// * <code><pre>
        /// * date ::= d   b7 b6 b5 b4 b3 b2 b1 b0
        /// *      ::= x65 b3 b2 b1 b0
        /// * </code></code>
        /// *
        /// * @param time the date in milliseconds from the epoch in UTC
        /// </summary>
        public override void WriteUTCDate(long time)
        {
            if (SIZE < _offset + 32)
                FlushBuffer();

            int offset = _offset;
            byte[] buffer = _buffer;

            if (time % 60000L == 0)
            {
                // compact date ::= x65 b3 b2 b1 b0

                long minutes = time / 60000L;

                if ((minutes >> 31) == 0 || (minutes >> 31) == -1)
                {
                    buffer[offset++] = (byte)CHessian2Constants.BC_DATE_MINUTE;
                    buffer[offset++] = ((byte)(minutes >> 24));
                    buffer[offset++] = ((byte)(minutes >> 16));
                    buffer[offset++] = ((byte)(minutes >> 8));
                    buffer[offset++] = ((byte)(minutes >> 0));

                    _offset = offset;
                    return;
                }
            }

            buffer[offset++] = (byte)CHessian2Constants.BC_DATE;
            buffer[offset++] = ((byte)(time >> 56));
            buffer[offset++] = ((byte)(time >> 48));
            buffer[offset++] = ((byte)(time >> 40));
            buffer[offset++] = ((byte)(time >> 32));
            buffer[offset++] = ((byte)(time >> 24));
            buffer[offset++] = ((byte)(time >> 16));
            buffer[offset++] = ((byte)(time >> 8));
            buffer[offset++] = ((byte)(time));

            _offset = offset;
        }

        /// <summary>
        /// Resets all counters and references
        /// </summary>
        public void Reset()
        {
            if (_refs != null)
            {
                _refs.Clear();
                _refCount = 0;
            }

            _classRefs.Clear();
            _typeRefs = null;
            _offset = 0;
            _isPacket = false;
            _isUnshared = false;
        }

        public void PrintLenString(String v)
        {
            if (SIZE < _offset + 16)
                FlushBuffer();

            if (v == null)
            {
                _buffer[_offset++] = (byte)(0);
                _buffer[_offset++] = (byte)(0);
            }
            else
            {
                int len = v.Length;
                _buffer[_offset++] = (byte)(len >> 8);
                _buffer[_offset++] = (byte)(len);

                PrintString(v, 0, len);
            }
        }

        /// <summary>
        /// * Prints a string to the stream, encoded as UTF-8
        /// *
        /// * @param v the string to print.
        /// </summary>
        public void PrintString(String v)
        {
            PrintString(v, 0, v.Length);
        }

        /// <summary>
        /// * Prints a string to the stream, encoded as UTF-8
        /// *
        /// * @param v the string to print.
        /// </summary>
        public void PrintString(String v, int strOffset, int length)
        {
            int offset = _offset;
            byte[] buffer = _buffer;

            for (int i = 0; i < length; i++)
            {
                if (SIZE <= offset + 16)
                {
                    _offset = offset;
                    FlushBuffer();
                    offset = _offset;
                }

                char ch = v[i + strOffset];

                if (ch < 0x80)
                    buffer[offset++] = (byte)(ch);
                else if (ch < 0x800)
                {
                    buffer[offset++] = (byte)(0xc0 + ((ch >> 6) & 0x1f));
                    buffer[offset++] = (byte)(0x80 + (ch & 0x3f));
                }
                else
                {
                    buffer[offset++] = (byte)(0xe0 + ((ch >> 12) & 0xf));
                    buffer[offset++] = (byte)(0x80 + ((ch >> 6) & 0x3f));
                    buffer[offset++] = (byte)(0x80 + (ch & 0x3f));
                }
            }

            _offset = offset;
        }

        /// <summary>
        /// * Prints a string to the stream, encoded as UTF-8
        /// *
        /// * @param v the string to print.
        /// </summary>
        public void PrintString(char[] v, int strOffset, int length)
        {
            int offset = _offset;
            byte[] buffer = _buffer;

            for (int i = 0; i < length; i++)
            {
                if (SIZE <= offset + 16)
                {
                    _offset = offset;
                    FlushBuffer();
                    offset = _offset;
                }

                char ch = v[i + strOffset];

                if (ch < 0x80)
                    buffer[offset++] = (byte)(ch);
                else if (ch < 0x800)
                {
                    buffer[offset++] = (byte)(0xc0 + ((ch >> 6) & 0x1f));
                    buffer[offset++] = (byte)(0x80 + (ch & 0x3f));
                }
                else
                {
                    buffer[offset++] = (byte)(0xe0 + ((ch >> 12) & 0xf));
                    buffer[offset++] = (byte)(0x80 + ((ch >> 6) & 0x3f));
                    buffer[offset++] = (byte)(0x80 + (ch & 0x3f));
                }
            }

            _offset = offset;
        }

        /// <summary>
        ///   * <code><pre>
        /// * type ::= string
        /// *      ::= int
        /// * </code></pre>
        /// </summary>
        private void WriteType(String type)
        {
            FlushIfFull();

            int len = type.Length;
            if (len == 0)
            {
                throw new ArgumentException("empty type is not allowed");
            }

            if (_typeRefs == null)
                _typeRefs = new Dictionary<string, int>();

            if (_typeRefs.ContainsKey(type))
            {
                int typeRef = _typeRefs[type];

                WriteInt(typeRef);
            }
            else
            {
                _typeRefs.Add(type, _typeRefs.Count);

                WriteString(type);
            }
        }

        public void WriteVersion()
        {
            FlushIfFull();

            _buffer[_offset++] = (byte)'H';
            _buffer[_offset++] = (byte)2;
            _buffer[_offset++] = (byte)0;
        }

        private void FlushIfFull()
        {
            int offset = _offset;

            if (SIZE < offset + 32)
            {
                FlushBuffer();
            }
        }

        public void FlushBuffer()
        {
            int offset = _offset;

            Stream os = _os;

            if (!_isPacket && offset > 0)
            {
                _offset = 0;
                if (os != null)
                    os.Write(_buffer, 0, offset);
            }
            else if (_isPacket && offset > 4)
            {
                int len = offset - 4;

                _buffer[0] |= (byte)0x80;
                _buffer[1] = (byte)(0x7e);
                _buffer[2] = (byte)(len >> 8);
                _buffer[3] = (byte)(len);
                _offset = 4;

                if (os != null)
                    os.Write(_buffer, 0, offset);

                _buffer[0] = (byte)0x00;
                _buffer[1] = (byte)0x56;
                _buffer[2] = (byte)0x56;
                _buffer[3] = (byte)0x56;
            }
        }


        public override void Flush()
        {
            FlushBuffer();

            if (_os != null)
                _os.Flush();
        }
    }
}
