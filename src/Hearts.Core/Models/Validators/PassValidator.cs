using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentValidation;

namespace Hearts.Models.Validators
{
    internal class PassValidator : AbstractValidator<Pass>
    {
        internal PassValidator()
        {
            RuleFor(p => p.Cards)
                .Must((p, cs) =>
                    cs.All(c => p
                        .Giver
                        .Hand
                        .Contains(c)))
                .WithMessage("all cards must be from the player's hand");

            RuleFor(p => p.Cards)
                .Must(cs => cs.Count() == 3)
                .WithMessage("the player must pass exactly three cards");

            RuleFor(p => p.Cards)
                .Must(cs => cs.Distinct().Count() == cs.Count())
                .WithMessage("the cards passed must all be distinct");
        }
    }
}
