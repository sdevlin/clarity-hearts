using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Hearts.Messages;
using UI.Commands;
using System.Collections.ObjectModel;
using Hearts.Models;
using System.Configuration;
using System.IO;
using System.Reflection;
using Hearts.Utility;
using System.Threading;
using System.Xml.Linq;
using System.Threading.Tasks;
using UI.Extensions;
using UI.Controllers;
using UI.Controllers.Interfaces;

namespace UI.ViewModels
{
    public class MainViewModel : ActiveViewModel<MainViewModel>
    {
        private Thread _gameThread;
        private readonly ManualResetEvent _signal = 
            new ManualResetEvent(true);
        private bool _isPaused;
        private readonly GameController _controller;

        public MainViewModel(BlockingMediator mediator)
            : base(mediator)
        {
            _controller = new GameController(mediator);
            mediator.AddSignal(_signal);
            Title = "TC16 - Clarity Hearts";
            Game = new GameViewModel(mediator);
            Batch = new BatchViewModel();
            Tourney = new TourneyViewModel(_controller, mediator);
            var referencedAssemblies = AppDomain
                .CurrentDomain
                .GetAssemblies();
            var playerProviders = Directory
                .GetFiles(Directory.GetCurrentDirectory(), "*.dll")
                .Select(s => Assembly.LoadFile(s))
                .Where(a => !referencedAssemblies.Contains(a))
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsSubclassOf(typeof(PlayerBase)))
                .ToDictionary(t => t
                    .Assembly
                    .ManifestModule
                    .Name
                    .Replace(".dll", string.Empty),
                    t => (Func<PlayerBase>)
                        (() => (PlayerBase)Activator.CreateInstance(t)));
            Players = playerProviders
                .Values
                .Select(f => new PlayerInfo
                {
                    Name = f().Name,
                    Provider = f
                })
                .ToList();
            ConfigGames = XElement.Load("config/games.xml")
                .Elements("game")
                .Select(g => new GameInfo
                {
                    Name = g.Attribute("name").Value,
                    Players = g.Elements("player")
                        .Select(p => p.Attribute("lib").Value)
                        .Select(s => playerProviders[s])
                        .Select(f => Players.Single(p => p.Provider == f))
                })
                .ToList();
            Scores = new List<PlayToInfo>();
            Scores.Add(new PlayToInfo { Value = 75 });
            Scores.Add(new PlayToInfo { Value = 150 });
        }

        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged(vm => vm.Title);
                }
            }
        }

        public GameViewModel Game { get; private set; }

        public BatchViewModel Batch { get; private set; }

        public TourneyViewModel Tourney { get; private set; }

        public IList<PlayerInfo> Players { get; private set; }

        public IList<GameInfo> ConfigGames { get; private set; }

        public IList<PlayToInfo> Scores { get; private set; }

        private int PlayTo
        {
            get
            {
                return Scores
                    .Where(i => i.IsSelected)
                    .Select(i => (int?)i.Value)
                    .FirstOrDefault() ?? 0;
            }
        }

        private DelegateCommand _aboutCommand;
        public ICommand AboutCommand
        {
            get
            {
                if (_aboutCommand == null)
                {
                    _aboutCommand = new DelegateCommand(About);
                }
                return _aboutCommand;
            }
        }

        private DelegateCommand _exitCommand;
        public ICommand ExitCommand
        {
            get
            {
                if (_exitCommand == null)
                {
                    _exitCommand = new DelegateCommand(Exit);
                }
                return _exitCommand;
            }
        }

        private DelegateCommand _pauseCommand;
        public ICommand PauseCommand
        {
            get
            {
                if (_pauseCommand == null)
                {
                    _pauseCommand = new DelegateCommand(Pause);
                }
                return _pauseCommand;
            }
        }

        private DelegateCommand _killCommand;
        public ICommand KillCommand
        {
            get
            {
                if (_killCommand == null)
                {
                    _killCommand = new DelegateCommand(KillGame);
                }
                return _killCommand;
            }
        }

        private DelegateCommand _newCommand;
        public ICommand NewCommand
        {
            get
            {
                if (_newCommand == null)
                {
                    _newCommand = new DelegateCommand(NewGame);
                }
                return _newCommand;
            }
        }

        private DelegateCommand<GameInfo> _loadCommand;
        public ICommand LoadCommand
        {
            get
            {
                if (_loadCommand == null)
                {
                    _loadCommand =
                        new DelegateCommand<GameInfo>(LoadGame);
                }
                return _loadCommand;
            }
        }

        private DelegateCommand _tourneyCommand;
        public ICommand TourneyCommand
        {
            get
            {
                if (_tourneyCommand == null)
                {
                    _tourneyCommand = new DelegateCommand(PlayTourney);
                }
                return _tourneyCommand;
            }
        }

        private DelegateCommand _batchCommand;
        public ICommand BatchCommand
        {
            get
            {
                if (_batchCommand == null)
                {
                    _batchCommand = new DelegateCommand(BatchRun);
                }
                return _batchCommand;
            }
        }

        private void About()
        {
            MessageBox.Show("by sean devlin");
        }

        private void Exit()
        {
            Application.Current.Shutdown();
        }

        private void Pause()
        {
            _controller.Pause();
        }

        private void KillGame()
        {
            _controller.Kill();
            Batch.Stop();
            Tourney.Stop();
        }

        private void LoadGame(GameInfo info)
        {
            _controller.Start(info, PlayTo);
        }

        private void NewGame()
        {
            LoadGame(new GameInfo
            {
                Name = "ad hoc game",
                Players = Players
                    .Where(p => p.IsSelected)
                    .Choose(4)
            });
        }

        private void PlayTourney()
        {
            Tourney.Play(ConfigGames, PlayTo);
        }

        private void BatchRun()
        {
            if (!Batch.IsBatchMode)
            {
                Batch.Run(Players.Where(p => p.IsSelected));
            }
        }

        public class GameInfo : IGameInfo
        {
            public string Name { get; set; }
            public IEnumerable<IPlayerInfo> Players { get; set; }
        }

        public class PlayerInfo : IPlayerInfo
        {
            public string Name { get; set; }
            public Func<PlayerBase> Provider { get; set; }
            public bool IsSelected { get; set; }
        }

        public class PlayToInfo
        {
            public int Value { get; set; }
            public bool IsSelected { get; set; }
        }
    }
}
