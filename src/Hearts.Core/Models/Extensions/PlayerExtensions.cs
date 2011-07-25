using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hearts.Views;
using Hearts.Models.Validators;
using Hearts.Models;

namespace Hearts.Models.Extensions
{
    public static class PlayerExtensions
    {
        internal static RelativeLocation LocationRelativeTo
            (this PlayerBase player, PlayerBase other)
        {
            return player.Table.GetRelativeLocation(other, player);
        }

        public static IEnumerable<Card> GetLeadableCards
            (this PlayerBase player, DealView dealInProgress)
        {
            var validator = new LeadValidator(dealInProgress);
            return player
                .CardList
                .Where(c => validator
                    .Validate(new Play(c, player))
                    .IsValid)
                .OrderBy(c => c.Suit)
                .ThenBy(c => c.Rank);
        }

        public static IEnumerable<Card> GetPlayableCards
            (this PlayerBase player, TrickView trickInProgress)
        {
            var validator = new PlayValidator(trickInProgress);
            return player
                .CardList
                .Where(c => validator
                    .Validate(new Play(c, player))
                    .IsValid)
                .OrderBy(c => c.Suit)
                .ThenBy(c => c.Rank);
        }
    }
}
