using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hearts.Models;

namespace Hearts.Views
{
    public sealed class PlayView
    {
        public Card Card { get; private set; }
        public PlayerView Player { get; private set; }
        internal PlayView(Play play, PlayerBase beholder)
        {
            Card = play.Card;
            Player = new PlayerView(play.Player, beholder);
        }
    }
}
