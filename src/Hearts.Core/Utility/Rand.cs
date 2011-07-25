using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hearts.Utility
{
    internal static class Rand
    {
        private static readonly Random _rand = new Random();

        public static IEnumerable<bool> Bools()
        {
            while (true)
            {
                yield return _rand.Next() % 2 == 0;
            }
        }

        public static bool NextBool()
        {
            return Bools().First();
        }

        public static IEnumerable<int> Ints()
        {
            while (true)
            {
                yield return _rand.Next();
            }
        }

        public static int Next(int minValue, int maxValue)
        {
            return _rand.Next(minValue, maxValue);
        }

        public static int Next(int maxValue)
        {
            return Next(0, maxValue);
        }

        public static int Next()
        {
            return Next(int.MaxValue);
        }

        public static IEnumerable<double> Doubles()
        {
            while (true)
            {
                yield return _rand.NextDouble();
            }
        }

        public static double NextDouble(double minValue, double maxValue)
        {
            return minValue + Doubles().First() * (maxValue - minValue);
        }

        public static double NextDouble(double maxValue)
        {
            return NextDouble(0d, maxValue);
        }

        public static double NextDouble()
        {
            return NextDouble(0d, 1d);
        }

        public static double BoundedDouble(double bound)
        {
            return NextDouble(-bound, bound);
        }
    }
}
