using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hearts.Models
{
    internal class Table
    {
        private readonly IList<PlayerBase> _players;
        internal Table(IList<PlayerBase> players)
        {
            _players = players;
        }

        internal AbsoluteLocation GetAbsoluteLocation(PlayerBase player)
        {
            switch (_players.IndexOf(player))
            {
                case 0:
                    return AbsoluteLocation.South;
                case 1:
                    return AbsoluteLocation.West;
                case 2:
                    return AbsoluteLocation.North;
                case 3:
                    return AbsoluteLocation.East;
                default:
                    throw new ArgumentOutOfRangeException("player");
            }
        }

        internal RelativeLocation GetRelativeLocation
            (PlayerBase first, PlayerBase second)
        {
            int i = _players.IndexOf(first);
            int j = _players.IndexOf(second);
            int location = j >= i
                ? j - i
                : j - i + _players.Count;
            return (RelativeLocation)location;
        }
    }
}
