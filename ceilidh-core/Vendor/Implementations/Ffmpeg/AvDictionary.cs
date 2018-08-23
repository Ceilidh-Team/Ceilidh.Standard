using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Ceilidh.Core.Util;

namespace Ceilidh.Core.Vendor.Implementations.Ffmpeg
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct AvDictionaryEntry
    {
        private readonly IntPtr _key;
        private readonly IntPtr _value;

        public string Key => Marshal.PtrToStringUTF8(_key);
        public string Value => Marshal.PtrToStringUTF8(_value);
    }

    internal struct AvDictionaryStruct { }

    internal sealed unsafe class AvDictionary : IDictionary<string, string>, IReadOnlyDictionary<string, string>, IDisposable
    {
        [Flags]
        private enum AvDictionaryFlags
        {
            MatchCase = 1,
            IgnoreSuffix = 2,
            DontStrdupKey = 4,
            DontStrdupVal = 8,
            DontOverwrite = 16,
            Append = 32
        }

        private struct Enumerator : IEnumerator<KeyValuePair<string, string>>
        {
            private readonly AvDictionary _dict;
            private AvDictionaryEntry* _currentEntry;

            public KeyValuePair<string, string> Current => _currentEntry == null ? default : new KeyValuePair<string, string>(_currentEntry->Key, _currentEntry->Value);

            object IEnumerator.Current => Current;

            public Enumerator(AvDictionary dict)
            {
                _dict = dict;
                _currentEntry = null;
            }

            public bool MoveNext()
            {
                fixed (byte* ptr = new byte[] { 0 })
                    return (_currentEntry = av_dict_get(_dict._dictPtr, ptr, _currentEntry, AvDictionaryFlags.IgnoreSuffix)) != null;
            }

            public void Reset()
            {
                _currentEntry = null;
            }

            public void Dispose() { }
        }

        private readonly bool _ownPtr;
        private AvDictionaryStruct* _dictPtr;

        public ICollection<string> Keys => this.Select(x => x.Key).ToList();
        IEnumerable<string> IReadOnlyDictionary<string, string>.Values => Values;

        IEnumerable<string> IReadOnlyDictionary<string, string>.Keys => Keys;

        public ICollection<string> Values => this.Select(x => x.Value).ToList();

        public int Count => this.Count();

        public bool IsReadOnly => false;

        public string this[string key]
        {
            get => TryGetValue(key, out var value) ? value : throw new KeyNotFoundException();
            set
            {
                fixed (byte* keyData = Encoding.UTF8.GetBytesNullTerminated(key), valueData =
                    Encoding.UTF8.GetBytesNullTerminated(value))
                    av_dict_set(ref _dictPtr, keyData, valueData, 0);
            }
        }

        public AvDictionary() : this(default)
        {

        }

        public AvDictionary(IReadOnlyDictionary<string, string> initialValues)
        {
            _ownPtr = true;

            if (initialValues == null) return;
            foreach (var pair in initialValues)
                Add(pair);
        }

        public AvDictionary(AvDictionaryStruct* dictPtr, bool ownPtr = true)
        {
            _dictPtr = dictPtr;
            _ownPtr = ownPtr;
        }

        public AvDictionary(IntPtr dictPtr, bool ownPtr = true) : this((AvDictionaryStruct*)dictPtr.ToPointer(), ownPtr)
        {
        }

        public ref AvDictionaryStruct GetPinnableReference()
        {
            return ref *_dictPtr;
        }

        public void Add(string key, string value)
        {
            fixed (byte* keyData = Encoding.UTF8.GetBytesNullTerminated(key), valueData =
                Encoding.UTF8.GetBytesNullTerminated(value))
                if (av_dict_set(ref _dictPtr, keyData, valueData, AvDictionaryFlags.DontOverwrite) < 0)
                    throw new ArgumentException("", nameof(key)); // TODO: Real thing here
        }

        public bool ContainsKey(string key)
        {
            fixed (byte* keyData = Encoding.UTF8.GetBytesNullTerminated(key))
                return av_dict_get(_dictPtr, keyData, null, 0) != null;
        }

        public bool Remove(string key)
        {
            fixed (byte* keyData = Encoding.UTF8.GetBytesNullTerminated(key))
                return av_dict_set(ref _dictPtr, keyData, null, 0) >= 0;
        }

        public bool TryGetValue(string key, out string value)
        {
            fixed (byte* keyData = Encoding.UTF8.GetBytesNullTerminated(key))
            {
                var entry = av_dict_get(_dictPtr, keyData, null, 0);
                if (entry == null)
                {
                    value = null;
                    return false;
                }

                value = entry->Value;
                return true;
            }
        }

        public void Add(KeyValuePair<string, string> item) => Add(item.Key, item.Value);

        public void Clear() => throw new NotSupportedException();

        public bool Contains(KeyValuePair<string, string> item) => TryGetValue(item.Key, out var value) && item.Value == value;

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            foreach (var pair in this)
            {
                if (arrayIndex >= array.Length)
                    return;

                array[arrayIndex++] = pair;
            }
        }

        public bool Remove(KeyValuePair<string, string> item) => Remove(item.Key);

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Dispose()
        {
            if (_ownPtr && _dictPtr != null)
                av_freep(ref _dictPtr);
        }

        #region Native

#if WIN32
        [DllImport("avutil-56")]
#else
        [DllImport("avutil")]
#endif  
        private static extern void av_freep(ref AvDictionaryStruct* buffer);

#if WIN32
        [DllImport("avutil-56")]
#else
        [DllImport("avutil")]
#endif
        private static extern AvDictionaryEntry* av_dict_get(AvDictionaryStruct* m,
            byte* key, AvDictionaryEntry* prev, AvDictionaryFlags flags);

#if WIN32
        [DllImport("avutil-56")]
#else
        [DllImport("avutil")]
#endif
        private static extern int av_dict_count(void* m);

#if WIN32
        [DllImport("avutil-56")]
#else
        [DllImport("avutil")]
#endif
        private static extern int av_dict_set(ref AvDictionaryStruct* pm, byte* key,
            byte* value, AvDictionaryFlags flags);

#if WIN32
        [DllImport("avutil-56")]
#else
        [DllImport("avutil")]
#endif
        private static extern void av_dict_copy(ref AvDictionaryStruct* dst, AvDictionaryStruct* src, AvDictionaryFlags flags);

#if WIN32
        [DllImport("avutil-56")]
#else
        [DllImport("avutil")]
#endif
        private static extern void av_dict_free(ref AvDictionaryStruct* m);

        #endregion
    }
}
