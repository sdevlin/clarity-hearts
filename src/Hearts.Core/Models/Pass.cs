using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hearts.Models
{
    internal class Pass
    {
        internal IList<Card> Cards { get; private set; }
        internal PlayerBase Giver { get; private set; }
        internal PlayerBase Receiver { get; private set; }
        internal Pass(IEnumerable<Card> cards, PlayerBase giver, 
            PlayerBase receiver)
        {
            Cards = cards.ToList();
            Giver = giver;
            Receiver = receiver;
        }
    }
}
