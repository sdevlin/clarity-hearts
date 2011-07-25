using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Hearts.Messages;
using Hearts.Models;
using UI.Controllers.Interfaces;
using UI.Extensions;
using Hearts.Utility;

namespace UI.Controllers
{
    internal class GameController
    {
        private Thread _gameThread;
        private readonly ManualResetEvent _signal = 
            new ManualResetEvent(true);
        private readonly BlockingMediator _mediator;

        public GameController(BlockingMediator mediator)
        {
            _mediator = mediator;
            _mediator.AddSignal(_signal);
        }

        private static PlayerBase[] GetPlayers(IGameInfo info)
        {
            return info
                .Players
                .Select(p => p.Provider)
                .Select(f => f())
                .PadAndCull()
                .Shuffle()
                .ToArray();
        }

        public bool IsPaused { get; private set; }

        public Game Start(IGameInfo info)
        {
            return Start(info, 100);
        }

        public Game Start(IGameInfo info, int playTo)
        {
            Kill();
            var game = new Game(GetPlayers(info), playTo, _mediator);
            _gameThread = new Thread(() => game.PlayToCompletion());
            _gameThread.Name = string.Format("{0} Thread", info.Name);
            _gameThread.IsBackground = true;
            _gameThread.Start();
            return game;
        }

        public void Pause()
        {
            IsPaused = !IsPaused;
            if (IsPaused)
            {
                _signal.Reset();
            }
            else
            {
                _signal.Set();
            }
        }

        public void DelayFor(int milliseconds)
        {
            if (!IsPaused)
            {
                Pause();
            }
            ThreadPool.QueueUserWorkItem(_ =>
                {
                    Thread.Sleep(milliseconds);
                    if (IsPaused)
                    {
                        Pause();
                    }
                });
        }

        public void Kill()
        {
            if (_gameThread != null &&
                _gameThread.IsAlive)
            {
                _gameThread.Abort();
                _gameThread.Join();
            }
        }
    }
}
