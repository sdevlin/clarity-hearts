using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hearts.Messages;
using Hearts.Models;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using Hearts.Models.Extensions;
using System.Reflection;

namespace UI.ViewModels
{
    public class PlayerViewModel : ActiveViewModel<PlayerViewModel>
    {
        internal PlayerViewModel(Func<Game, PlayerBase> playerSelector, 
            BlockingMediator mediator)
            : base(mediator)
        {
            PlayerBase player = null;
            Assembly asm = null;
            string[] resourceNames = null;
            string oldAvatar = null;
            PointsTaken = new ObservableCollection<object>();
            Action update = () =>
                {
                    Name = player.Name;
                    Score = player.Score;
                    if (resourceNames.Contains(player.Avatar) &&
                        oldAvatar != player.Avatar)
                    {
                        var stream = asm
                            .GetManifestResourceStream(player.Avatar);
                        _dispatcher.Invoke((Action)(() =>
                        {
                            var bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.StreamSource = stream;
                            bitmap.EndInit();
                            Avatar = bitmap;
                        }));
                        oldAvatar = player.Avatar;
                    }
                    else if (string.IsNullOrEmpty(player.Avatar))
                    {
                        Avatar = null;
                    }
                };
            _mediator.Subscribe(MessageType.GameStarted,
                o => 
                {
                    player = playerSelector((Game)o);
                    asm = player
                        .GetType()
                        .Assembly;
                    resourceNames = asm
                        .GetManifestResourceNames();
                    update();
                });
            _mediator.Subscribe(MessageType.DealStarted,
                _ => _dispatcher.BeginInvoke((Action)
                    (() => PointsTaken.Clear())));
            _mediator.Subscribe(MessageType.TrickFinished,
                _ => _dispatcher.BeginInvoke((Action)
                    (() =>
                    {
                        int pointsTaken = player
                            .TrickList
                            .SelectMany(t => t.Cards)
                            .Value();
                        while (PointsTaken.Count < pointsTaken)
                        {
                            PointsTaken.Add(new object());
                        }
                    })));
            _mediator.Subscribe(MessageType.DealFinished,
                _ => update());
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(vm => vm.Name);
                }
            }
        }

        private BitmapImage _avatar;
        public BitmapImage Avatar
        {
            get { return _avatar; }
            set
            {
                if (_avatar != value)
                {
                    _avatar = value;
                    OnPropertyChanged(vm => vm.Avatar);
                }
            }
        }

        private int? _score;
        public int? Score
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

        public ObservableCollection<object> PointsTaken { get; private set; }
    }
}
