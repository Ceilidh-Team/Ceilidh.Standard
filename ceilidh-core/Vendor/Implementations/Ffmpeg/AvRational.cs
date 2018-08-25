namespace Ceilidh.Core.Vendor.Implementations.Ffmpeg
{
    internal readonly struct AvRational
    {
        public readonly int Numerator;
        public readonly int Denominator;

        public AvRational(int numerator, int denominator)
        {
            Numerator = numerator;
            Denominator = denominator;
        }

        public override string ToString() => $"{Numerator}/{Denominator}";

        public static explicit operator float(AvRational rational) => rational.Numerator / (float)rational.Denominator;
        public static explicit operator double(AvRational rational) => rational.Numerator / (double)rational.Denominator;
        public static explicit operator decimal(AvRational rational) => rational.Numerator / (decimal)rational.Denominator;
    }
}