using System;
using System.Diagnostics.Contracts;

namespace ProjectCeilidh.Ceilidh.Standard.Filter
{
    public readonly struct Decibel
    {
        public double Value { get; }

        public Decibel(double value) => Value = value;

        [Pure]
        public double GetAmplitudeRatio() => Math.Pow(10, Value / 20);

        public static Decibel FromAmplitudeRatio(double ratio)
        {
            return new Decibel(Math.Log10(ratio) * 20);
        }
        
        public static Decibel operator +(Decibel one, Decibel two)
        {
            return new Decibel(10 * Math.Log10(Math.Pow(10, one.Value / 10) + Math.Pow(10, two.Value / 10)));
        }

        public static Decibel operator -(Decibel one, Decibel two)
        {
            return new Decibel(10 * Math.Log10(Math.Pow(10, one.Value / 10) - Math.Pow(10, two.Value / 10)));
        }
    }
}