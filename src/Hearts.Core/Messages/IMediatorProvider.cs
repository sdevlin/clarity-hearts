using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hearts.Messages
{
    public interface IMediatorProvider<TMediator> 
        where TMediator : IMediator
    {
        TMediator Mediator { get; }
    }
}
