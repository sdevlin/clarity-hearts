using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hearts.Utility
{
    public static class Enumerable
    {
        /// <summary>
        /// Orders a sequence randomly.
        /// </summary>
        public static IEnumerable<TSource> Shuffle<TSource>
            (this IEnumerable<TSource> source)
        {
            return source.OrderBy(_ => Rand.Next());
        }

        /// <summary>
        /// Returns a random group of elements from a sequence.
        /// </summary>
        /// <param name="count">
        /// The number of elements to return.
        /// </param>
        public static IEnumerable<TSource> Choose<TSource>
            (this IEnumerable<TSource> source, int count)
        {
            return source
                .Shuffle()
                .Take(count);
        }

        /// <summary>
        /// Returns a random element from a sequence.
        /// </summary>
        public static TSource Choose<TSource>
            (this IEnumerable<TSource> source)
        {
            return source
                .Choose(1)
                .Single();
        }

        /// <summary>
        /// Yields the elements of a sequence in order.
        /// </summary>
        /// <param name="element">
        /// The element to start the sequence with.
        /// </param>
        public static IEnumerable<TSource> StartingWith<TSource>
            (this IEnumerable<TSource> source, TSource element)
        {
            var list = source.ToList();
            int i = list.IndexOf(element);
            do
            {
                yield return list[i];
                i += 1;
                i %= list.Count;
            } while (!list[i].Equals(element));
        }

        /// <summary>
        /// Repeats an enumeration endlessly.
        /// </summary>
        public static IEnumerable<TSource> Cycle<TSource>
            (this IEnumerable<TSource> source)
        {
            while (true)
            {
                foreach (var elem in source)
                {
                    yield return elem;
                }
            }
        }
    }
}
