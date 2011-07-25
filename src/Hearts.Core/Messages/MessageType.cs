using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hearts.Messages
{
    public enum MessageType
    {
        GameStarting,
        GameStarted,
        DealStarted,
        CardsDealt,
        CardsPassed,
        TrickStarted,
        TrickFinished,
        DealFinished,
        GameFinishing,
        GameFinished,
        ValidationError,
        UnhandledException
    }
}
