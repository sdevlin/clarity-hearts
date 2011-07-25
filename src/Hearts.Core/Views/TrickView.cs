using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hearts.Utility;
using Hearts.Models;
using Hearts.Models.Extensions;

namespace Hearts.Views
{
    public sealed class TrickView
    {
        private List<PlayView> PlayList { get; set; }
        public IList<PlayView> Plays
        {
            get { return PlayList.AsReadOnly(); }
        }
        public bool IsFirstTrick { get; private set; }

        internal TrickView(Trick trick, PlayerBase beholder)
        {
            IsFirstTrick = trick.IsFirstTrick;
            PlayList = trick
                .Select(p => new PlayView(p, beholder))
                .ToList();
        }

        public IEnumerable<Card> Cards
        {
            get { return PlayList.Select(p => p.Card); }
        }

        public PlayView Lead
        {
            get { return PlayList.FirstOrDefault(); }
        }

        public PlayView WinningPlay
        {
            get
            {
                return Lead != null
                    ? PlayList
                        .Where(p => p.Card.Suit == Lead.Card.Suit)
                        .OrderByDescending(p => p.Card.Rank)
                        .First()
                    : null;
            }
        }

        public bool HasPointCards
        {
            get
            {
                return Plays
                    .Select(p => p.Card)
                    .Any(c => c.IsPointCard());
            }
        }
    }
}
