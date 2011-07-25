using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hearts.Models;
using Hearts.Utility;

namespace UI.Extensions
{
    public static class PlayerExtensions
    {
        public static IEnumerable<PlayerBase> PadWithDrones
            (this IEnumerable<PlayerBase> players)
        {
            return players
                .Shuffle()
                .Concat(Rand
                    .Ints()
                    .Select(i =>
                        new DefaultPlayer(string.Format("drone #{0}", i))));
        }

        public static IEnumerable<PlayerBase> 
            Cull(this IEnumerable<PlayerBase> players)
        {
            return players.Take(4);
        }

        public static IEnumerable<PlayerBase> PadAndCull
            (this IEnumerable<PlayerBase> players)
        {
            return players
                .PadWithDrones()
                .Cull();
        }
    }
}
