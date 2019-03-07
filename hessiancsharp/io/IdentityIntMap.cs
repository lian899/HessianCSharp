using System;
using System.Collections;
using System.Text;

namespace HessianCSharp.io
{
    /// <summary>
    /// * The IntMap provides a simple hashmap from keys to integers.  The API is
    /// * an abbreviation of the HashMap collection API.
    /// *
    /// * &lt;p&gt;The convenience of IntMap is avoiding all the silly wrapping of
    /// * integers.
    /// </summary>
    public class IdentityIntMap
    {
        /// <summary>
        /// * Encoding of a null entry.  Since NULL is equal to Integer.MIN_VALUE,
        /// * it's impossible to distinguish between the two.
        /// </summary>
        public static int NULL = unchecked((int)0xdeadbeef); // Integer.MIN_VALUE + 1;

        private Object[] _keys;
        private int[] _values;

        private int _size;
        private int _prime;

        Hashtable _hs = new Hashtable();

        /**
         * Create a new IntMap.  Default size is 16.
         */
        public IdentityIntMap(int capacity)
        {
            //_keys = new Object[capacity];
            //_values = new int[capacity];

            //_prime = getBiggestPrime(_keys.Length);
            //_size = 0;
        }

        /**
         * Clear the hashmap.
         */
        public void Clear()
        {
            //Object[] keys = _keys;
            //int[] values = _values;

            //for (int i = keys.Length - 1; i >= 0; i--)
            //{
            //    keys[i] = null;
            //    values[i] = 0;
            //}

            //_size = 0;
            _hs.Clear();
        }
        /**
         * Returns the current number of entries in the map.
         */
        public int Size()
        {
            //return _size;
            return _hs.Count;

        }

        /**
         * Puts a new value in the property table with the appropriate flags
         */
        public int Get(Object key)
        {
            //int prime = _prime;
            //int hash = (key).GetHashCode() % prime;
            //// int hash = key.hashCode() & mask;

            //Object[] keys = _keys;

            //while (true)
            //{
            //    Object mapKey = keys[hash];

            //    if (mapKey == null)
            //        return NULL;
            //    else if (mapKey == key)
            //        return _values[hash];

            //    hash = (hash + 1) % prime;
            //}
            return (int)_hs[key];
        }

        /**
         * Puts a new value in the property table with the appropriate flags
         */
        public int Put(Object key, int value, bool isReplace)
        {
            //int prime = _prime;
            //int hash = Math.Abs((key).GetHashCode() % prime);
            //// int hash = key.hashCode() % prime;

            //Object[] keys = _keys;

            //while (true)
            //{
            //    Object testKey = keys[hash];

            //    if (testKey == null)
            //    {
            //        keys[hash] = key;
            //        _values[hash] = value;

            //        _size++;

            //        if (keys.Length <= 4 * _size)
            //            resize(4 * keys.Length);

            //        return value;
            //    }
            //    else if (key != testKey)
            //    {
            //        hash = (hash + 1) % prime;

            //        continue;
            //    }
            //    else if (isReplace)
            //    {
            //        int old = _values[hash];

            //        _values[hash] = value;

            //        return old;
            //    }
            //    else
            //    {
            //        return _values[hash];
            //    }
            //}

            var testvalue = _hs[key];
            if (testvalue == null)
            {
                _hs.Add(key, value);
                return value;
            }
            else if (isReplace)
            {
                int old = (int)_hs[key];

                _hs[key] = value;

                return old;
            }
            else
                return (int)testvalue;

        }

        /**
         * Removes a value in the property table.
         */
        public void Remove(Object key)
        {
            //if (put(key, NULL, true) != NULL)
            //{
            //    _size--;
            //}
            _hs.Clear();
        }

        /**
         * Expands the property table
         */
        private void Resize(int newSize)
        {
            Object[] keys = _keys;
            int[] values = _values;

            _keys = new Object[newSize];
            _values = new int[newSize];
            _size = 0;

            _prime = GetBiggestPrime(_keys.Length);

            for (int i = keys.Length - 1; i >= 0; i--)
            {
                Object key = keys[i];
                int value = values[i];

                if (key != null && value != NULL)
                {
                    Put(key, value, true);
                }
            }
        }

        protected int HashCode(Object value)
        {
            return (value).GetHashCode();
        }

        public String toString()
        {
            StringBuilder sbuf = new StringBuilder();

            sbuf.Append("IntMap[");
            bool isFirst = true;

            for (int i = 0; i <= _keys.Length; i++)
            {
                if (_keys[i] != null)
                {
                    if (!isFirst)
                        sbuf.Append(", ");

                    isFirst = false;
                    sbuf.Append(_keys[i]);
                    sbuf.Append(":");
                    sbuf.Append(_values[i]);
                }
            }
            sbuf.Append("]");

            return sbuf.ToString();
        }

        public static int[] PRIMES =
        {
           1,       /* 1<< 0 = 1 */
           2,       /* 1<< 1 = 2 */
           3,       /* 1<< 2 = 4 */
           7,       /* 1<< 3 = 8 */
           13,      /* 1<< 4 = 16 */
           31,      /* 1<< 5 = 32 */
           61,      /* 1<< 6 = 64 */
           127,     /* 1<< 7 = 128 */
           251,     /* 1<< 8 = 256 */
           509,     /* 1<< 9 = 512 */
           1021,    /* 1<<10 = 1024 */
           2039,    /* 1<<11 = 2048 */
           4093,    /* 1<<12 = 4096 */
           8191,    /* 1<<13 = 8192 */
           16381,   /* 1<<14 = 16384 */
           32749,   /* 1<<15 = 32768 */
           65521,   /* 1<<16 = 65536 */
           131071,  /* 1<<17 = 131072 */
           262139,  /* 1<<18 = 262144 */
           524287,  /* 1<<19 = 524288 */
           1048573, /* 1<<20 = 1048576 */
           2097143, /* 1<<21 = 2097152 */
           4194301, /* 1<<22 = 4194304 */
           8388593, /* 1<<23 = 8388608 */
           16777213, /* 1<<24 = 16777216 */
           33554393, /* 1<<25 = 33554432 */
           67108859, /* 1<<26 = 67108864 */
           134217689, /* 1<<27 = 134217728 */
           268435399, /* 1<<28 = 268435456 */
          };

        public static int GetBiggestPrime(int value)
        {
            for (int i = PRIMES.Length - 1; i >= 0; i--)
            {
                if (PRIMES[i] <= value)
                    return PRIMES[i];
            }

            return 2;
        }
    }
}
