using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hearts.Utility
{
    public static class Double
    {
        public static double Inexact(this double d, double tolerance)
        {
            if (Math.Abs(tolerance) > 1)
            {
                throw new ArgumentOutOfRangeException("tolerance");
            }
            return d * Rand.NextDouble(1 - tolerance, 1 + tolerance);
        }

        public static double MaybeNegate(this double d)
        {
            return Rand.NextBool() ? d : -d;
        }
    }
}
