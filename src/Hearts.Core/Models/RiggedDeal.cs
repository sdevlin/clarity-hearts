using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hearts.Messages;
using Hearts.Models.Extensions;

namespace Hearts.Models
{
    internal class RiggedDeal : Deal
    {
        private readonly Dictionary<PlayerBase, IList<Card>> _map;

        public RiggedDeal(IList<PlayerBase> players, PassingMode mode,
            IMediator mediator,
            Dictionary<PlayerBase, IList<Card>> map)
            : base(players, 100, mode, mediator)
        {
            _map = map;
        }

        protected override void DealCards()
        {
            foreach (var pair in _map)
            {
                var player = pair.Key;
                var cards = pair.Value;
                player.CardList.AddRange(cards);
            }
            var otherPlayers = Players
                .Except(_map.Keys)
                .ToList();
            var otherCards = Card
                .Deck
                .Except(_map.SelectMany(p => p.Value));
            otherCards.DealTo(otherPlayers);
        }
    }
}
