using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Hearts.Utility;
using System.Collections;
using Hearts.Messages;
using Hearts.Models.Extensions;
using Hearts.Models.Validators;
using Hearts.Views;

namespace Hearts.Models
{
    internal class Trick : GameUnitBase, IEnumerable<Play>
    {
        internal Deal Deal { get; private set; }

        private List<PlayerBase> PlayerList { get; set; }
        public IList<PlayerBase> PlayersStartingWithLead
        {
            get { return PlayerList.AsReadOnly(); }
        }

        private List<Play> PlayList { get; set; }
        public IList<Play> Plays
        {
            get { return PlayList.AsReadOnly(); }
        }

        public bool IsFirstTrick { get; private set; }

        public override bool IsOver
        {
            get { return PlayList.Count == PlayerList.Count; }
        }

        public IEnumerable<Card> Cards
        {
            get { return Plays.Select(p => p.Card); }
        }

        public Play Lead
        {
            get { return Plays.First(); }
        }

        public Play WinningPlay
        {
            get
            {
                return Plays
                    .Where(p => p.Card.Suit == Lead.Card.Suit)
                    .OrderByDescending(p => p.Card.Rank)
                    .First();
            }
        }

        public bool HasPointCards
        {
            get
            {
                return Plays
                    .Select(p => p.Card)
                    .Any(c => c.Suit == Suit.Hearts || 
                        c == Card.QueenOfSpades);
            }
        }

        internal Trick(Deal deal, IMediator mediator)
            : base(deal.Players, mediator)
        {
            Deal = deal;
            IsFirstTrick = !deal.Tricks.Any();
            PlayList = new List<Play>();
            var players = Deal.Players;
            var leader = IsFirstTrick
                ? players
                    .Single(p => p
                        .Hand
                        .Contains(Card.TwoOfClubs))
                : players
                    .Single(p => p == Deal
                        .Tricks
                        .Last(t => t.IsOver)
                        .WinningPlay
                        .Player);
            PlayerList = players
                .StartingWith(leader)
                .ToList();
            AddPlay(GetLead(deal, leader));
        }

        private Play GetLead(Deal deal, PlayerBase leader)
        {
            var dealView = new DealView(deal, leader);
            var validator = new LeadValidator(dealView);
            Func<Play> resultProvider = () =>
            {
                var card = leader.GetLead(dealView);
                return new Play(card, leader);
            };
            Func<Play> defaultProvider = () =>
            {
                var card = leader
                    .GetLeadableCards(dealView)
                    .First();
                return new Play(card, leader);
            };
            var play = GetSafeResult
                (resultProvider, validator, defaultProvider);
            return play;
        }

        public override void PlayToCompletion()
        {
            var players = PlayersStartingWithLead.Skip(1);
            foreach (var player in players)
            {
                var trickView = new TrickView(this, player);
                var validator = new PlayValidator(trickView);
                Func<Play> resultProvider = () =>
                {
                    var card = player.GetPlay(
                        new DealView(Deal, player),
                        new TrickView(this, player));
                    return new Play(card, player);
                };
                Func<Play> defaultProvider = () =>
                {
                    var card = player
                        .GetPlayableCards(trickView)
                        .First();
                    return new Play(card, player);
                };
                var play = GetSafeResult
                    (resultProvider, validator, defaultProvider);
                AddPlay(play);
            }
            var winner = WinningPlay.Player;
            winner.AddTrick(this);
            _mediator.Publish(MessageType.TrickFinished, this);
        }

        public void AddPlay(Play play)
        {
            play.Player.CardList.Remove(play.Card);
            PlayList.Add(play);
        }

        public IEnumerator<Play> GetEnumerator()
        {
            return Plays.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
