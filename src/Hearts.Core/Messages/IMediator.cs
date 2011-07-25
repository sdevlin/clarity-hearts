using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hearts.Messages
{
    public interface IMediator
    {
        void Subscribe(MessageType type, Action<object> callback);
        void Publish(MessageType type, object payload);
    }
}
