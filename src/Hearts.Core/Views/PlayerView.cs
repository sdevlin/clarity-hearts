using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hearts.Models.Extensions;
using Hearts.Models;

namespace Hearts.Views
{
    public sealed class PlayerView
    {
        public int Score { get; private set; }
        public RelativeLocation Location { get; private set; }
        internal PlayerView(PlayerBase player, PlayerBase beholder)
        {
            Score = player.Score;
            Location = player.LocationRelativeTo(beholder);
        }
    }
}
