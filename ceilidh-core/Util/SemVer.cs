using System;
using System.Linq;

namespace Ceilidh.Core.Util
{
    internal static class SemVer
    {
        [Flags]
        public enum SemVerMatchFlags
        {
            MajorEqual = 1 << 0,
            MajorCompatible = 1 << 1,
            MinorEqual = 1 << 2,
            MinorCompatible = 1 << 3,
            PatchEqual = 1 << 4,
            PatchCompatible = 1 << 5
        }

        public static bool AreEquivalent(this Version one, Version two,
            SemVerMatchFlags flags = SemVerMatchFlags.MajorEqual | SemVerMatchFlags.MinorCompatible |
                                     SemVerMatchFlags.PatchCompatible)
        {
            foreach (var flag in Enum.GetValues(typeof(SemVerMatchFlags)).OfType<SemVerMatchFlags>()
                .Where(x => (flags & x) != 0))
                switch (flag)
                {
                    case SemVerMatchFlags.MajorEqual:
                        if (one.Major != two.Major) return false;
                        break;
                    case SemVerMatchFlags.MajorCompatible:
                        if (one.Major > two.Major) return false;
                        break;
                    case SemVerMatchFlags.MinorEqual:
                        if (one.Major != two.Major && one.Minor != two.Minor) return false;
                        break;
                    case SemVerMatchFlags.MinorCompatible:
                        if (one.Major == two.Major && one.Minor > two.Minor) return false;
                        break;
                    case SemVerMatchFlags.PatchEqual:
                        if (one.Major != two.Major && one.Minor != two.Minor && one.Revision != two.Revision)
                            return false;
                        break;
                    case SemVerMatchFlags.PatchCompatible:
                        if (one.Major == two.Major && one.Minor == two.Minor && one.Revision > two.Revision)
                            return false;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            return true;
        }
    }
}