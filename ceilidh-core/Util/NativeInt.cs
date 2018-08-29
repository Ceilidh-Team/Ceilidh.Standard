using System;

namespace Ceilidh.Core.Util
{
    internal struct NativeInt
    {
        private readonly IntPtr _value;

        public NativeInt(IntPtr value)
        {
            _value = value;
        }

        public static implicit operator IntPtr(NativeInt value) => value._value;
        public static implicit operator NativeInt(int value) => new NativeInt(new IntPtr(value));
        public static implicit operator NativeInt(long value) => new NativeInt(new IntPtr(value));
        public static implicit operator NativeInt(IntPtr value) => new NativeInt(value);
        public static unsafe implicit operator NativeInt(void* value) => new NativeInt(new IntPtr(value));
    }
}
