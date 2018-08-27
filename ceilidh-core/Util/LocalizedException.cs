using System;
using Ceilidh.Core.Vendor.Contracts;

namespace Ceilidh.Core.Util
{
    public class LocalizedException : Exception
    {
        internal static ILocalizationController LocalizationController;

        public LocalizedException(Exception innerException, params object[] args) : base(LocalizationController.Translate(innerException.Message, args), innerException) {}

        public override string ToString() => InnerException.ToString();
    }
}
