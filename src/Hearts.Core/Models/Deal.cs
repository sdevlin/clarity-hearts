using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hearts.Models.Extensions;
using System.Collections;
using Hearts.Messages;
using Hearts.Models.Validators;
using Hearts.Views;

namespace Hearts.Models
{
    internal class Deal : GameUnitBase, IEnumerable<Trick>
    {
        private List<PlayerBase> PlayerList { get; set; }
        internal IList<PlayerBase> Players
        {
            get { return PlayerList.AsReadOnly(); }
        }

        internal bool AreHeartsBroken
        {
            get
            {
                return Tricks
                    .SelectMany(t => t.Cards)
                    .Any(c => c.Suit == Suit.Hearts);
            }
        }

        public override bool IsOver
        {
            get
            {
                return Tricks.Count == Card.Deck.Count() / Players.Count;
            }
        }

        private List<Trick> TrickList { get; set; }
        internal IList<Trick> Tricks
        {
            get { return TrickList.AsReadOnly(); }
        }

        internal Dictionary<PlayerBase, int> ScoreDeltas { get; private set; }

        private readonly int _playTo;

        internal PassingMode Mode { get; private set; }

        internal Deal(IList<PlayerBase> players, int playTo, 
            PassingMode mode, IMediator mediator)
            : base(players, mediator)
        {
            PlayerList = players.ToList();
            _playTo = playTo;
            TrickList = new List<Trick>();
            ScoreDeltas = players.ToDictionary(p => p, _ => 0);
            Mode = mode;
        }

        protected virtual void DealCards()
        {
            Card.Deck.DealTo(Players);
        }

        private void ExchangeCards()
        {
            if (Mode == PassingMode.Hold)
            {
                return;
            }
            int offset = GetOffset(Mode);
            Func<int, int> getOffsetIndex =
                i => (i + offset) % Players.Count;
            var passes = new List<Pass>();
            var validator = new PassValidator();
            for (int i = 0; i < Players.Count; i += 1)
            {
                var giver = Players[i];
                var receiver = Players[getOffsetIndex(i)];
                Func<Pass> resultProvider = () =>
                {
                    var cards = giver.GetCardsToPass
                        (new PlayerView(receiver, giver));
                    return new Pass(cards, giver, receiver);
                };
                Func<Pass> defaultProvider = () =>
                {
                    var cards = giver
                        .Hand
                        .OrderBy(c => c.Suit)
                        .ThenBy(c => c.Rank)
                        .Take(3);
                    return new Pass(cards, giver, receiver);
                };
                var pass = GetSafeResult
                    (resultProvider, validator, defaultProvider);
                passes.Add(pass);
            }
            foreach (var pass in passes)
            {
                var cards = pass.Cards;
                var giver = pass.Giver;
                var receiver = pass.Receiver;
                foreach (var card in cards)
                {
                    giver.CardList.Remove(card);
                    receiver.CardList.Add(card);
                }
                SendSafeNotification(() => receiver
                    .NotifyPassedCards(cards,
                        new PlayerView(giver, receiver)));
            }
            _mediator.Publish(MessageType.CardsPassed, 
                new PassCollection(Mode, passes));
        }

        private int GetOffset(PassingMode mode)
        {
            switch (mode)
            {
                case PassingMode.Left:
                    return 1;
                case PassingMode.Right:
                    return 3;
                case PassingMode.Across:
                    return 2;
                case PassingMode.Hold:
                    return 0;
            }
            throw new ArgumentOutOfRangeException("mode");
        }

        public override void PlayToCompletion()
        {
            _mediator.Publish(MessageType.DealStarted, this);
            DealCards();
            _mediator.Publish(MessageType.CardsDealt, this);
            ExchangeCards();
            while (!IsOver)
            {
                var trick = new Trick(this, _mediator);
                AddTrick(trick);
                trick.PlayToCompletion();
                foreach (var player in Players)
                {
                    SendSafeNotification(() => player
                        .NotifyTrickResults(new TrickView(trick, player)));
                }
            }
            TallyScores();
            RetrieveCards();
            _mediator.Publish(MessageType.DealFinished, this);
        }

        private void TallyScores()
        {
            int total = Card
                .Deck
                .Value();
            var shooter = Players
                .SingleOrDefault(p => p
                    .TrickList
                    .Cards()
                    .Value() == total);
            if (shooter != null)
            {
                var others = Players.Where(p => p != shooter);
                if (others.Any(p => p.Score + total >= _playTo) &&
                    others.Any(p => p.Score + total < shooter.Score))
                {
                    ScoreDeltas[shooter] = -total;
                    shooter.Score -= total;
                }
                else
                {
                    foreach (var player in others)
                    {
                        ScoreDeltas[player] = total;
                        player.Score += total;
                    }
                }
            }
            else
            {
                foreach (var player in Players)
                {
                    int delta = player
                        .TrickList
                        .Cards()
                        .Value();
                    ScoreDeltas[player] = delta;
                    player.Score += delta;
                }
            }
        }

        private void RetrieveCards()
        {
            foreach (var player in Players)
            {
                player.CardList.Clear();
                player.TrickList.Clear();
            }
        }

        public void AddTrick(Trick trick)
        {
            TrickList.Add(trick);
        }

        public IEnumerator<Trick> GetEnumerator()
        {
            return Tricks.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
