using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hearts.Messages;
using System.Windows;
using System.Windows.Threading;

namespace UI.ViewModels
{
    public abstract class ActiveViewModel<TViewModel> :
        ViewModelBase<TViewModel>,
        IMediatorProvider<BlockingMediator>
            where TViewModel : ViewModelBase<TViewModel>
    {
        protected readonly Dispatcher _dispatcher;
        protected readonly BlockingMediator _mediator;
        public ActiveViewModel(BlockingMediator mediator)
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            _mediator = mediator;
        }

        BlockingMediator IMediatorProvider<BlockingMediator>.Mediator
        {
            get { return _mediator; }
        }
    }
}
