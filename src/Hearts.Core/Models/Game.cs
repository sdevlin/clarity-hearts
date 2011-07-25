using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections;
using Hearts.Messages;
using Hearts.Models.Extensions;
using Hearts.Views;

namespace Hearts.Models
{
    internal class Game : GameUnitBase, IEnumerable<Deal>
    {
        internal List<PlayerBase> Players { get; private set; }

        private readonly int _playTo;
        public override bool IsOver
        {
            get { return Players.Any(p => p.Score >= _playTo); }
        }

        public PlayerBase Winner
        {
            get 
            { 
                return IsOver 
                    ? Players
                        .OrderBy(p => p.Score)
                        .First() 
                    : null; 
            }
        }

        private List<Deal> DealList { get; set; }
        internal IList<Deal> Deals
        {
            get { return DealList.AsReadOnly(); }
        }

        public Game(IList<PlayerBase> players, int playTo, IMediator mediator)
            : base(players, mediator)
        {
            Players = players.ToList();
            _playTo = playTo;
            DealList = new List<Deal>();
        }

        public Game(IList<PlayerBase> players, IMediator mediator)
            : this(players, 100, mediator)
        { }

        public override void PlayToCompletion()
        {
            _mediator.Publish(MessageType.GameStarting, this);
            _mediator.Publish(MessageType.GameStarted, this);
            while (!IsOver)
            {
                var mode = (PassingMode)(Deals.Count %
                    Utility.Enum.GetValues<PassingMode>().Count());
                var deal = new Deal(Players, _playTo, mode, _mediator);
                AddDeal(deal);
                deal.PlayToCompletion();
                foreach (var player in deal.Players)
                {
                    SendSafeNotification(() => player
                        .NotifyDealResults(new DealView(deal, player)));
                }
            }
            _mediator.Publish(MessageType.GameFinishing, this);
            _mediator.Publish(MessageType.GameFinished, this);
        }

        public void AddDeal(Deal deal)
        {
            DealList.Add(deal);
        }

        public IEnumerator<Deal> GetEnumerator()
        {
            return Deals.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
