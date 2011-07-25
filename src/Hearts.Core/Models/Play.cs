using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hearts.Models
{
    internal class Play
    {
        public Card Card { get; private set; }
        internal PlayerBase Player { get; private set; }
        internal Play(Card card, PlayerBase player)
        {
            Card = card;
            Player = player;
        }
    }
}
