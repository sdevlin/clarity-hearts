using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentValidation;
using Hearts.Models.Extensions;
using Hearts.Views;

namespace Hearts.Models.Validators
{
    internal class PlayValidator : AbstractValidator<Play>
    {
        public PlayValidator(TrickView trickInProgress)
        {
            RuleFor(p => p.Card)
                .Must(c => c.Suit == trickInProgress.Lead.Card.Suit)
                .Unless(p => !p.Player
                    .CardList
                    .OfSuit(trickInProgress.Lead.Card.Suit)
                    .Any())
                .WithMessage("must follow the lead suit when possible");

            RuleFor(p => p.Card)
                .Must(c => !c.IsPointCard())
                .When(_ => trickInProgress.IsFirstTrick)
                .Unless(p => p.Player
                    .CardList
                    .All(c => c.IsPointCard()))
                .WithMessage("point cards cannot be played on first trick");
            
            RuleFor(p => p.Card)
                .Must((p, c) => p.Player.Hand.Contains(c))
                .WithMessage("card must come from player's hand");
        }
    }
}
