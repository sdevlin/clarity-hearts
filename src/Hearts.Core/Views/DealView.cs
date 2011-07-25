using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hearts.Models;

namespace Hearts.Views
{
    public sealed class DealView
    {
        private List<PlayerView> PlayerList { get; set; }
        public IList<PlayerView> Players
        {
            get { return PlayerList.AsReadOnly(); }
        }
        public bool AreHeartsBroken { get; private set; }
        public bool IsOver { get; private set; }
        internal DealView(Deal deal, PlayerBase beholder)
        {
            PlayerList = deal
                .Players
                .Select(p => new PlayerView(p, beholder))
                .ToList();
            AreHeartsBroken = deal.AreHeartsBroken;
            IsOver = deal.IsOver;
        }
    }
}
