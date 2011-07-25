using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hearts.Messages;
using Hearts.Models;
using Hearts.Models.Extensions;
using System.Threading;

namespace Console
{
    public class Listener
    {
        public Listener(BlockingMediator mediator)
        {
            mediator.Subscribe(MessageType.GameStarted,
                WrapCallback<Game>(g =>
                {
                    Printer.PrintInfo(MessageType.GameStarted);
                    foreach (var player in g.Players)
                    {
                        var type = player.GetType();
                        Printer.PrintInfo(
                            "\t{0} from type {1} in assembly {2}",
                            player.Name,
                            type,
                            type.Module);
                    }
                }));

            mediator.Subscribe(MessageType.CardsDealt,
                WrapCallback<Deal>(d =>
                {
                    Printer.PrintInfo(MessageType.CardsDealt);
                    foreach (var player in d.Players)
                    {
                        Printer.PrintInfo("\t{0}: {1}", player.Name,
                            player.Hand.AsFormattedString());
                    }
                }));

            mediator.Subscribe(MessageType.CardsPassed,
                WrapCallback<PassCollection>(pc =>
                {
                    Printer.PrintInfo(MessageType.CardsPassed);
                    foreach (var pass in pc.Passes)
                    {
                        Printer.PrintInfo("\t{0}: {1} from ({2})",
                            pass.Receiver.Name,
                            pass.Cards.AsFormattedString(),
                            pass.Giver.Name);
                    }
                }));

            mediator.Subscribe(MessageType.TrickFinished,
                WrapCallback<Trick>(t =>
                {
                    Printer.PrintInfo(MessageType.TrickFinished);
                    foreach (var play in t)
                    {
                        string info = string.Format("\t{0}: {1}",
                            play.Player.Name, play.Card);
                        if (play == t.WinningPlay)
                        {
                            info += " (Winner)";
                        }
                        Printer.PrintInfo(info);
                    }
                }));

            mediator.Subscribe(MessageType.DealFinished,
                WrapCallback<Deal>(d =>
                {
                    Printer.PrintInfo(MessageType.DealFinished);
                    foreach (var player in d.Players)
                    {
                        Printer.PrintInfo("\t{0}: {1} ({2})", player.Name,
                            player.Score, d.ScoreDeltas[player]);
                    }
                }));

            mediator.Subscribe(MessageType.GameFinished,
                WrapCallback<Game>(g =>
                {
                    Printer.PrintInfo(MessageType.GameFinished);
                    foreach (var player in g.Players)
                    {
                        string info = string.Format("\t{0}: {1}",
                            player.Name, player.Score);
                        if (player == g.Winner)
                        {
                            info += " (Winner)";
                        }
                        Printer.PrintInfo(info);
                    }
                }));

            mediator.Subscribe(MessageType.ValidationError,
                WrapCallback<IEnumerable<string>>(ems =>
                {
                    foreach (var errorMsg in ems)
                    {
                        Printer.PrintError(errorMsg);
                    }
                }));

            mediator.Subscribe(MessageType.UnhandledException,
                WrapCallback<Exception>(e =>
                {
                    Printer.PrintError(e);
                }));
        }

        private Action<object> WrapCallback<T>
            (Action<T> callback)
        {
            return o => callback((T)o);
        }
    }
}
