using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hearts.Models.Extensions
{
    internal static class TrickExtensions
    {
        internal static IEnumerable<Card> Cards
            (this IEnumerable<Trick> tricks)
        {
            return tricks.SelectMany(t => t.Cards);
        }
    }
}
