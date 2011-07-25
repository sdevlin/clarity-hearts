using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hearts.Messages;
using FluentValidation;

namespace Hearts.Models
{
    internal abstract class GameUnitBase
    {
        protected readonly IMediator _mediator;

        public GameUnitBase(IList<PlayerBase> players, IMediator mediator)
        {
            if (!players.Any(p => p.IsSeated))
            {
                var table = new Table(players);
                foreach (var player in players)
                {
                    player.Table = table;
                }
            }
            _mediator = mediator;
        }

        public abstract void PlayToCompletion();

        public abstract bool IsOver { get; }

        protected void SendSafeNotification(Action notifier)
        {
            try
            {
                notifier();
            }
            catch (Exception ex)
            {
                _mediator.Publish(MessageType.UnhandledException, ex);
            }
        }

        protected TResult GetSafeResult<TResult>(Func<TResult> resultProvider,
            IValidator<TResult> validator,
            Func<TResult> defaultProvider)
        {
            var result = default(TResult);
            try
            {
                result = resultProvider();
                validator.ValidateAndThrow(result);
            }
            catch (ValidationException ex)
            {
                _mediator.Publish(MessageType.ValidationError,
                    ex.Errors.Select(e => e.ErrorMessage));
                result = defaultProvider();
            }
            catch (Exception ex)
            {
                _mediator.Publish(MessageType.UnhandledException, ex);
                result = defaultProvider();
            }
            return result;
        }
    }
}
