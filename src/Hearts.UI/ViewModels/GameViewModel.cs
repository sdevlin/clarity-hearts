using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hearts.Models;
using Hearts.Messages;

namespace UI.ViewModels
{
    public class GameViewModel : ActiveViewModel<GameViewModel>
    {
        public GameViewModel(BlockingMediator mediator)
            : base(mediator)
        {
            PlayerOne = new PlayerViewModel(g => g.Players[0], mediator);
            PlayerTwo = new PlayerViewModel(g => g.Players[1], mediator);
            PlayerThree = new PlayerViewModel(g => g.Players[2], mediator);
            PlayerFour = new PlayerViewModel(g => g.Players[3], mediator);
        }

        private PlayerViewModel _playerOne;
        public PlayerViewModel PlayerOne
        {
            get { return _playerOne; }
            set
            {
                if (_playerOne != value)
                {
                    _playerOne = value;
                    OnPropertyChanged(vm => vm.PlayerOne);
                }
            }
        }

        private PlayerViewModel _playerTwo;
        public PlayerViewModel PlayerTwo
        {
            get { return _playerTwo; }
            set
            {
                if (_playerTwo != value)
                {
                    _playerTwo = value;
                    OnPropertyChanged(vm => vm.PlayerTwo);
                }
            }
        }

        private PlayerViewModel _playerThree;
        public PlayerViewModel PlayerThree
        {
            get { return _playerThree; }
            set
            {
                if (_playerThree != value)
                {
                    _playerThree = value;
                    OnPropertyChanged(vm => vm.PlayerThree);
                }
            }
        }

        private PlayerViewModel _playerFour;
        public PlayerViewModel PlayerFour
        {
            get { return _playerFour; }
            set
            {
                if (_playerFour != value)
                {
                    _playerFour = value;
                    OnPropertyChanged(vm => vm.PlayerFour);
                }
            }
        }
    }
}
