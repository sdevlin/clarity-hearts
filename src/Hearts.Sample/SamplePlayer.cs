using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hearts.Models;
using Hearts.Models.Extensions;
using Hearts.Utility;
using Hearts.Views;

namespace Sample
{
    public class SamplePlayer : PlayerBase
    {
        private static int _count = 0;
        private readonly string _name;
        public SamplePlayer()
        {
            _count += 1;
            _name = "Sample" + _count.ToString();
        }

        public override string Name
        {
            get { return _name; }
        }

        public override IEnumerable<Card> GetCardsToPass(PlayerView toPlayer)
        {
            // try to pass off our high spades
            var highSpades = Hand
                .OfSuit(Suit.Spades)
                .Outranking(new Card(Suit.Spades, Rank.Jack))
                .OrderByDescending(c => c.Rank)
                .Take(3); // this will take three *if possible*
            var others = Hand
                .Except(highSpades)
                .Shuffle();
            return highSpades
                .Concat(others)
                .Take(3);
        }

        public override void NotifyPassedCards
            (IEnumerable<Card> cards, PlayerView fromPlayer)
        {

        }

        public override Card GetLead(DealView dealInProgress)
        {
            // who cares, just lead whatever
            return this
                .GetLeadableCards(dealInProgress)
                .Choose();
        }

        public override Card GetPlay
            (DealView dealInProgress, TrickView trickInProgress)
        {
            var cards = this.GetPlayableCards(trickInProgress);
            // if there are point cards, attempt to duck the trick
            var highCard = trickInProgress.WinningPlay.Card;
            if (trickInProgress.HasPointCards &&
                cards.Any(c => c.IsOutrankedBy(highCard)))
            {
                cards = cards.OutrankedBy(highCard);
            }
            return cards.First();
        }

        public override void NotifyTrickResults(TrickView completedTrick)
        {
            
        }

        public override void NotifyDealResults(DealView completedDeal)
        {
            
        }
    }
}
