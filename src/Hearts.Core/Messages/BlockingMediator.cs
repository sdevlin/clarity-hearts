using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Hearts.Messages
{
    public class BlockingMediator : IMediator
    {
        private readonly HashSet<EventWaitHandle>
            _signals = new HashSet<EventWaitHandle>();
        private readonly Dictionary<MessageType, Action<object>>
            _map = new Dictionary<MessageType, Action<object>>();

        public void AddSignal(EventWaitHandle signal)
        {
            _signals.Add(signal);
        }

        public void Subscribe(MessageType type, Action<object> callback)
        {
            if (!_map.ContainsKey(type))
            {
                _map.Add(type, null);
            }
            _map[type] += callback;
        }

        public void Publish(MessageType type, object payload)
        {
            if (!_map.ContainsKey(type))
            {
                return;
            }
            _map[type](payload);
            if (_signals.Any())
            {
                WaitHandle.WaitAll(_signals.ToArray());
            }
        }
    }
}
