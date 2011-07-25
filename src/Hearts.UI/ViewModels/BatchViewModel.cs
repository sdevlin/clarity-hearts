using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hearts.Messages;
using System.Collections.ObjectModel;
using Hearts.Models;
using UI.Extensions;
using Hearts.Utility;
using System.Threading.Tasks;
using System.Threading;
using UI.Controllers.Interfaces;

namespace UI.ViewModels
{
    public class BatchViewModel : ActiveViewModel<BatchViewModel>
    {
        public BatchViewModel()
            : base(new BlockingMediator())
        { }

        private CancellationTokenSource _cts;

        public void Run(IEnumerable<IPlayerInfo> info)
        {
            Players = info
                .Select(p => new PlayerInfo
                {
                    Name = p.Name,
                    Provider = p.Provider
                })
                .ToList();
            // note that execution of players query is deferred
            var players = Players
                .Select(p => p.Provider())
                .PadAndCull()
                .Shuffle();
            var mediator = new NopMediator();
            // ...until it is called in the games query
            var games = System.Linq.Enumerable
                .Range(0, 1000)
                .Select(_ => new Game(players.ToArray(), mediator));
            _cts = new CancellationTokenSource();
            var options = new ParallelOptions();
            options.CancellationToken = _cts.Token;
            GamesPlayed = 0;
            IsBatchMode = true;
            IsRunning = true;
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    Parallel.ForEach(games, options,
                        g =>
                        {
                            options.CancellationToken
                                .ThrowIfCancellationRequested();
                            g.PlayToCompletion();
                            lock (this)
                            {
                                _dispatcher.Invoke((Action)
                                    (() => GamesPlayed++));
                            }
                            var winner = Players
                                .SingleOrDefault(p => 
                                    p.Name == g.Winner.Name);
                            if (winner != null)
                            {
                                lock (winner)
                                {
                                    _dispatcher.Invoke((Action)
                                        (() => winner.Wins++));
                                }
                            }
                        });
                }
                catch (OperationCanceledException)
                { }
                finally
                {
                    _cts.Dispose();
                    _cts = null;
                    IsRunning = false;
                }
            });
        }

        public void Stop()
        {
            if (_cts != null)
            {
                _cts.Cancel();
            }
            IsBatchMode = false;
        }

        private IList<PlayerInfo> _players;
        public IList<PlayerInfo> Players
        {
            get { return _players; }
            set
            {
                if (_players != value)
                {
                    _players = value;
                    OnPropertyChanged(vm => vm.Players);
                }
            }
        }

        private bool _isBatchMode;
        public bool IsBatchMode
        {
            get { return _isBatchMode; }
            set
            {
                if (_isBatchMode != value)
                {
                    _isBatchMode = value;
                    OnPropertyChanged(vm => vm.IsBatchMode);
                }
            }
        }

        private bool _isRunning;
        public bool IsRunning
        {
            get { return _isRunning; }
            set
            {
                if (_isRunning != value)
                {
                    _isRunning = value;
                    OnPropertyChanged(vm => vm.IsRunning);
                }
            }
        }

        private int _gamesPlayed;
        public int GamesPlayed
        {
            get { return _gamesPlayed; }
            set
            {
                if (_gamesPlayed != value)
                {
                    _gamesPlayed = value;
                    OnPropertyChanged(vm => vm.GamesPlayed);
                }
            }
        }

        public class PlayerInfo : ViewModelBase<PlayerInfo>, IPlayerInfo
        {
            public string Name { get; set; }
            public Func<PlayerBase> Provider { get; set; }

            private int _wins;
            public int Wins
            {
                get { return _wins; }
                set
                {
                    if (_wins != value)
                    {
                        _wins = value;
                        OnPropertyChanged(vm => vm.Wins);
                    }
                }
            }
        }
    }
}
