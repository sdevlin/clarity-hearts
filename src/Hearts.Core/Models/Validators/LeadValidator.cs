using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentValidation;
using Hearts.Models.Extensions;
using Hearts.Views;

namespace Hearts.Models.Validators
{
    internal class LeadValidator : AbstractValidator<Play>
    {
        public LeadValidator(DealView dealInProgress)
        {
            RuleFor(p => p.Card)
                .Must(c => c == Card.TwoOfClubs)
                .When(p => p
                    .Player
                    .Hand
                    .Contains(Card.TwoOfClubs))
                .WithMessage("must lead two of clubs on first trick");

            RuleFor(p => p.Card)
                .Must(c => c.Suit != Suit.Hearts)
                .Unless(_ => dealInProgress.AreHeartsBroken)
                .Unless(p => p
                    .Player
                    .Hand
                    .All(c => c.Suit == Suit.Hearts))
                .WithMessage("cannot lead hearts until hearts are broken");

            RuleFor(p => p.Card)
                .Must((p, c) => p
                    .Player
                    .Hand
                    .Contains(c))
                .WithMessage("card must come from player's hand");
        }
    }
}
