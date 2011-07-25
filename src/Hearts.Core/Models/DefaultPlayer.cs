using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hearts.Models.Extensions;
using Hearts.Utility;
using Hearts.Views;

namespace Hearts.Models
{
    internal class DefaultPlayer : PlayerBase
    {
        private readonly string _name;
        public override string Name
        {
            get { return _name; }
        }

        public DefaultPlayer(string name)
        {
            _name = name;
        }

        public override IEnumerable<Card> GetCardsToPass
            (PlayerView toPlayer)
        {
            return this
                .Hand
                .Choose(3);
        }

        public override Card GetLead(DealView dealInProgress)
        {
            return this
                .GetLeadableCards(dealInProgress)
                .Choose();
        }

        public override Card GetPlay
            (DealView dealInProgress, TrickView trickInProgress)
        {
            return this
                .GetPlayableCards(trickInProgress)
                .Choose();
        }
    }
}
