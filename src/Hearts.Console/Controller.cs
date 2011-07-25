using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hearts.Messages;
using System.Threading;
using Hearts.Utility;
using System.Configuration;

namespace Console
{
    public class Controller
    {
        private readonly EventWaitHandle _signal;
        public bool IsBlocking { get; private set; }

        public Controller(BlockingMediator mediator)
        {
            _signal = new AutoResetEvent(false);
            mediator.AddSignal(_signal);
            dynamic config = ConfigurationManager.GetSection("delays");
            var delays = new Dictionary<MessageType, int>();
            foreach (string name in config)
            {
                delays[Hearts.Utility.Enum.Parse<MessageType>(name)] =
                    int.Parse(config[name]);
            }
            foreach (var type in Hearts.Utility.Enum.GetValues<MessageType>())
            {
                var key = type;
                mediator.Subscribe(type, ignore =>
                    {
                        if (delays.ContainsKey(key))
                        {
                            Thread.Sleep(delays[key]);
                        }
                        _signal.Reset();
                        if (!IsBlocking)
                        {
                            ThreadPool.QueueUserWorkItem(_ => _signal.Set());
                        }
                    });
            }
        }

        public void Toggle()
        {
            IsBlocking = !IsBlocking;
            if (!IsBlocking)
            {
                _signal.Set();
            }
        }

        public void Step()
        {
            if (IsBlocking)
            {
                _signal.Set();
            }
        }
    }
}
