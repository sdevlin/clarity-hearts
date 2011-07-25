using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hearts.Messages;
using Hearts.Models;
using Hearts.Models.Extensions;
using Hearts.Utility;
using cmd = System.Console;
using System.Configuration;
using System.Reflection;
using System.Collections;

namespace Console
{
    class Program
    {
        private static Thread _gameThread;
        private static readonly Dictionary<string, IList<Card>> _testHands =
            new Dictionary<string, IList<Card>>();

        static void Main(string[] args)
        {
            LoadTestHands();
            var mediator = new BlockingMediator();
            var listener = new Listener(mediator);
            var controller = new Controller(mediator);
            RunGame(mediator);
            cmd.Title = "TC16 - Clarity Hearts (Running)";
            while (true)
            {
                var info = cmd.ReadKey(true);
                switch (info.Key)
                {
                    case ConsoleKey.Q:
                        return;
                    case ConsoleKey.R:
                        _gameThread.Abort();
                        Thread.Sleep(2000);
                        _gameThread.Join();
                        RunGame(mediator);
                        break;
                    case ConsoleKey.T:
                        _gameThread.Abort();
                        Thread.Sleep(2000);
                        _gameThread.Join();
                        cmd.WriteLine("choose a test hand:");
                        int i = 0;
                        foreach (var pair in _testHands)
                        {
                            i += 1;
                            cmd.WriteLine("\t{0}. {1}", i, pair.Key);
                        }
                        int choice = cmd.ReadKey(true).KeyChar - '1';
                        var hand = _testHands
                            .Skip(choice)
                            .First()
                            .Value;
                        RigDeal(hand, mediator);
                        break;
                    case ConsoleKey.Spacebar:
                        controller.Toggle();
                        cmd.Title = string.Format(
                            "TC16 - Clarity Hearts ({0})",
                            controller.IsBlocking
                                ? "Paused"
                                : "Running");
                        break;
                    case ConsoleKey.Enter:
                        controller.Step();
                        break;
                    default:
                        break;
                }
            }
        }

        private static void LoadTestHands()
        {
            dynamic config = ConfigurationManager.GetSection("testHands");
            foreach (string name in config)
            {
                _testHands[name] = Card.ParseHand(config[name]);
            }
        }

        private static void RunGame(BlockingMediator mediator)
        {
            var players = GetPlayers();
            var game = new Game(players, mediator);
            _gameThread = new Thread(() => game.PlayToCompletion());
            _gameThread.Name = "Game Thread";
            _gameThread.IsBackground = true;
            _gameThread.Start();
        }

        private static void RigDeal(IList<Card> hand, BlockingMediator mediator)
        {
            var players = GetPlayers();
            var map = new Dictionary<PlayerBase, IList<Card>>();
            map.Add(players[0], hand);
            var deal = new RiggedDeal(players, PassingMode.Left,
                mediator, map);
            _gameThread = new Thread(() => deal.PlayToCompletion());
            _gameThread.IsBackground = true;
            _gameThread.Start();
        }

        private static PlayerBase[] GetPlayers()
        {
            var hash = (Hashtable)ConfigurationManager.GetSection("playerLibs");
            return hash
                .Values
                .Cast<string>()
                .Select(s => Path.GetFullPath(s))
                .Select(p => Assembly.LoadFile(p))
                .Select(a => a.GetTypes()
                    .Single(t => t.IsSubclassOf(typeof(PlayerBase))))
                .Select(t => Activator.CreateInstance(t))
                .Cast<PlayerBase>()
                .Shuffle()
                .Cycle()
                .Take(4)
                .ToArray();
        }
    }
}
