using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hearts.Messages
{
    public class NopMediator : IMediator
    {
        public void Subscribe(MessageType type, Action<object> callback)
        { }

        public void Publish(MessageType type, object payload)
        { }
    }
}
