using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hearts.Messages;
using Hearts.Models;
using System.Threading;
using UI.Controllers;
using UI.Controllers.Interfaces;

namespace UI.ViewModels
{
    public class TourneyViewModel : ActiveViewModel<TourneyViewModel>
    {
        private readonly GameController _controller;
        private readonly Queue<IGameInfo> _schedule =
            new Queue<IGameInfo>();
        private int _playTo;

        internal TourneyViewModel(GameController controller, 
            BlockingMediator mediator)
            : base(mediator)
        {
            _controller = controller;
            mediator.Subscribe(MessageType.GameStarting,
                _ =>
                {
                    if (!IsTourneyMode)
                    {
                        return;
                    }
                    _dispatcher.Invoke((Action)(
                        () =>
                        {
                            ShouldShowOverlay = true;
                            IsGameStarting = true;
                        }));
                    _controller.DelayFor(2500);
                });
            mediator.Subscribe(MessageType.GameStarted,
                _ => _dispatcher.Invoke((Action)(
                        () =>
                        {
                            ShouldShowOverlay = false;
                            IsGameStarting = false;
                        })));
            mediator.Subscribe(MessageType.GameFinishing,
                o =>
                {
                    if (!IsTourneyMode)
                    {
                        return;
                    }
                    _dispatcher.Invoke((Action)(
                        () =>
                        {
                            var game = (Game)o;
                            var playerGroups = game
                                .Players
                                .GroupBy(p => p.Score)
                                .OrderBy(g => g.Key);
                            var scores = new[] { 10d, 6, 3, 1 };
                            int idx = 0;
                            foreach (var group in playerGroups)
                            {
                                int count = group.Count();
                                for (int i = idx; i < idx + count; i++)
                                {
                                    foreach (var player in group)
                                    {
                                        var info = Players
                                            .SingleOrDefault(pi =>
                                                pi.Name == player.Name);
                                        if (info != null)
                                        {
                                            info.Score += scores[i] / count;
                                        }
                                    }
                                }
                                idx += count;
                            }
                            ShouldShowOverlay = true;
                            IsGameFinishing = true;
                        }));
                    _controller.DelayFor(10000);
                });
            mediator.Subscribe(MessageType.GameFinished,
                _ =>
                {
                    _dispatcher.Invoke((Action)(
                        () =>
                        {
                            //ShouldShowOverlay = _schedule.Any();
                            IsGameFinishing = !_schedule.Any();
                        }));
                    ThreadPool.QueueUserWorkItem(__ => PlayNextGame());
                });
        }

        private bool _isTourneyMode;
        public bool IsTourneyMode
        {
            get { return _isTourneyMode; }
            set
            {
                if (_isTourneyMode != value)
                {
                    _isTourneyMode = value;
                    OnPropertyChanged(vm => vm.IsTourneyMode);
                }
            }
        }

        private bool _shouldShowOverlay;
        public bool ShouldShowOverlay
        {
            get { return _shouldShowOverlay; }
            set
            {
                if (_shouldShowOverlay != value)
                {
                    _shouldShowOverlay = value;
                    OnPropertyChanged(vm => vm.ShouldShowOverlay);
                }
            }
        }

        private bool _isGameStarting;
        public bool IsGameStarting
        {
            get { return _isGameStarting; }
            set
            {
                if (_isGameStarting != value)
                {
                    _isGameStarting = value;
                    OnPropertyChanged(vm => vm.IsGameStarting);
                }
            }
        }

        private string _gameName;
        public string GameName
        {
            get { return _gameName; }
            set
            {
                if (_gameName != value)
                {
                    _gameName = value;
                    OnPropertyChanged(vm => vm.GameName);
                }
            }
        }

        private IList<string> _playerNames;
        public IList<string> PlayerNames
        {
            get { return _playerNames; }
            set
            {
                if (_playerNames != value)
                {
                    _playerNames = value;
                    OnPropertyChanged(vm => vm.PlayerNames);
                }
            }
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

        private bool _isGameFinishing;
        public bool IsGameFinishing
        {
            get { return _isGameFinishing; }
            set
            {
                if (_isGameFinishing != value)
                {
                    _isGameFinishing = value;
                    OnPropertyChanged(vm => vm.IsGameFinishing);
                }
            }
        }

        public void Play(IEnumerable<IGameInfo> infos, int playTo)
        {
            IsTourneyMode = true;
            _playTo = playTo;
            foreach (var info in infos)
            {
                _schedule.Enqueue(info);
            }
            Players = infos
                .SelectMany(gi => gi.Players)
                .GroupBy(pi => pi.Name)
                .Select(g => g.First())
                .Select(pi => new PlayerInfo
                {
                    Name = pi.Name,
                    Provider = pi.Provider
                })
                .ToList();
            PlayNextGame();
        }

        private void PlayNextGame()
        {
            if (_schedule.Any())
            {
                var info = _schedule.Dequeue();
                GameName = info.Name;
                PlayerNames = info
                    .Players
                    .Select(pi => pi.Name)
                    .ToList();
                _controller.Start(info, _playTo);
            }
            else
            {
                IsTourneyMode = false;
            }
        }

        public void Stop()
        {
            _controller.Kill();
            IsTourneyMode = false;
            ShouldShowOverlay = false;
            IsGameStarting = false;
            IsGameFinishing = false;
        }

        public class GameInfo : IGameInfo
        {
            public string Name { get; set; }
            public IEnumerable<IPlayerInfo> Players { get; set; }
        }

        public class PlayerInfo : ViewModelBase<PlayerInfo>, IPlayerInfo
        {
            public string Name { get; set; }
            public Func<PlayerBase> Provider { get; set; }

            private double _score;
            public double Score
            {
                get { return _score; }
                set
                {
                    if (_score != value)
                    {
                        _score = value;
                        OnPropertyChanged(vm => vm.Score);
                    }
                }
            }
        }
    }
}
