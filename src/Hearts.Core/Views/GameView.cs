using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hearts.Models;

namespace Hearts.Views
{
    public sealed class GameView
    {
        private List<PlayerView> PlayerList { get; set; }
        public IList<PlayerView> Players
        {
            get { return PlayerList.AsReadOnly(); }
        }
        public bool IsOver { get; private set; }
        internal GameView(Game game, PlayerBase beholder)
        {
            PlayerList = game
                .Players
                .Select(p => new PlayerView(p, beholder))
                .ToList();
            IsOver = game.IsOver;
        }
    }
}
