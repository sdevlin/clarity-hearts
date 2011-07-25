using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Hearts.Messages;
using Hearts.Models;
using System.Windows.Media.Animation;
using System.Threading;
using Hearts.Utility;
using UI.Animations;
using System.Windows.Threading;

namespace UI.Views
{
    public partial class GameView : ActiveView
    {
        private readonly Dictionary<Card, CardView> _map =
            new Dictionary<Card, CardView>();
        private readonly ManualResetEvent _signal =
            new ManualResetEvent(true);

        public GameView()
        {
            InitializeComponent();
            Loaded += (sender, e) =>
                {
                    var center = new Point(cardsView.ActualWidth / 2,
                        cardsView.ActualHeight / 2);
                    foreach (var card in Card.Deck)
                    {
                        _map[card] = new CardView(card, center);
                        cardsView.Children.Add(_map[card]);
                    }
                };
            Loaded += WireUpSubscriptions;
        }

        public void WireUpSubscriptions(object sender, RoutedEventArgs e)
        {
            Mediator.AddSignal(_signal);

            Mediator.Subscribe(MessageType.GameStarted,
                WrapCallback(Reset));
            //Mediator.Subscribe(MessageType.DealStarted,
            //    AddAnimation<object, Shuffle>(() => new Shuffle(_map)));
            Mediator.Subscribe(MessageType.DealStarted,
                WrapCallback(Shuffle));
            Mediator.Subscribe(MessageType.CardsDealt,
                WrapCallback<Deal>(DealCards));
            Mediator.Subscribe(MessageType.CardsPassed,
                WrapCallback<PassCollection>(PassCards));
            //TrickStarted,
            //TrickFinished,
            Mediator.Subscribe(MessageType.TrickFinished,
                WrapCallback<Trick>(PlayTrick));
            Mediator.Subscribe(MessageType.DealFinished,
                WrapCallback(Reset));
            //GameFinished,
            //ValidationError,
            //UnhandledException
        }

        private Action<object> AddAnimation<T, TAnimation>
            (Func<TAnimation> provider)
            where TAnimation : CardsAnimationBase<T>
        {
            return o => Dispatcher.Invoke((Action)(() =>
            {
                _signal.Reset();
                var animation = provider();
                var sb = animation.Construct((T)o);
                sb.Completed += (sender, e) => _signal.Set();
                sb.Begin();
            }));
        }

        private Action<object> WrapCallback<T>
            (Action<T, Storyboard> callback)
        {
            return o => Dispatcher.Invoke((Action)(() =>
            {
                _signal.Reset();
                var sb = new Storyboard();
                sb.Completed += (sender, e) => _signal.Set();
                callback((T)o, sb);
                sb.Begin();
            }));
        }

        private Action<object> WrapCallback(Action<Storyboard> callback)
        {
            return WrapCallback<object>((_, sb) => callback(sb));
        }

        // a lot of this animation crap should be factored out
        private static readonly PropertyPath _scaleXPath =
            new PropertyPath("(0).(1)[1].(2)",
            new[] 
                { 
                    UIElement.RenderTransformProperty, 
                    TransformGroup.ChildrenProperty, 
                    ScaleTransform.ScaleXProperty
                });
        private static readonly PropertyPath _scaleYPath =
            new PropertyPath("(0).(1)[1].(2)",
            new[] 
                { 
                    UIElement.RenderTransformProperty, 
                    TransformGroup.ChildrenProperty, 
                    ScaleTransform.ScaleYProperty
                });
        private static readonly PropertyPath _pivotAnglePath =
            new PropertyPath("(0).(1)[2].(2)",
            new[] 
                { 
                    UIElement.RenderTransformProperty, 
                    TransformGroup.ChildrenProperty, 
                    RotateTransform.AngleProperty
                });
        private static readonly PropertyPath _rotateAnglePath =
            new PropertyPath("(0).(1)[3].(2)",
            new[] 
                { 
                    UIElement.RenderTransformProperty, 
                    TransformGroup.ChildrenProperty, 
                    RotateTransform.AngleProperty
                });
        private static readonly PropertyPath _translateXPath =
            new PropertyPath("(0).(1)[5].(2)",
            new[] 
                { 
                    UIElement.RenderTransformProperty, 
                    TransformGroup.ChildrenProperty, 
                    TranslateTransform.XProperty 
                });
        private static readonly PropertyPath _translateYPath =
            new PropertyPath("(0).(1)[5].(2)",
            new[] 
                { 
                    UIElement.RenderTransformProperty, 
                    TransformGroup.ChildrenProperty, 
                    TranslateTransform.YProperty 
                });
        private static readonly PropertyPath _zIndexPath =
            new PropertyPath("(0)", new[]  {  Panel.ZIndexProperty });

        private void Reset(Storyboard sb)
        {
            foreach (var pair in _map)
            {
                var view = pair.Value;
                var reset = new Storyboard
                {
                    AccelerationRatio = 0.5,
                    DecelerationRatio = 0.5
                };
                Storyboard.SetTarget(reset, view);
                sb.Children.Add(reset);

                var duration = TimeSpan.FromSeconds(0.5);
                var scaleX = new DoubleAnimation(1, duration);
                Storyboard.SetTargetProperty(scaleX, _scaleXPath);
                reset.Children.Add(scaleX);

                var scaleY = new DoubleAnimation(1, duration);
                Storyboard.SetTargetProperty(scaleY, _scaleYPath);
                reset.Children.Add(scaleY);

                var translateX = new DoubleAnimation(0, duration);
                Storyboard.SetTargetProperty(translateX, _translateXPath);
                reset.Children.Add(translateX);

                var translateY = new DoubleAnimation(0, duration);
                Storyboard.SetTargetProperty(translateY, _translateYPath);
                reset.Children.Add(translateY);

                var rotateAngle = new DoubleAnimation(0, duration);
                Storyboard.SetTargetProperty(rotateAngle, _rotateAnglePath);
                reset.Children.Add(rotateAngle);

                var pivotAngle = new DoubleAnimation(0, duration);
                Storyboard.SetTargetProperty(pivotAngle, _pivotAnglePath);
                reset.Children.Add(pivotAngle);

                var zIndex = new Int32AnimationUsingKeyFrames
                {
                    Duration = duration
                };
                zIndex.KeyFrames.Add(new DiscreteInt32KeyFrame(
                    pair.Key.GetHashCode()));
                Storyboard.SetTargetProperty(zIndex, _zIndexPath);
                reset.Children.Add(zIndex);
            }
            sb.Completed += (sender, e) =>
                {
                    foreach (var pair in _map)
                    {
                        Panel.SetZIndex(pair.Value, pair.Key.GetHashCode());
                    }
                };
        }

        private void Shuffle(Storyboard sb)
        {
            foreach (var view in _map.Values)
            {
                var shuffle = new Storyboard();
                Storyboard.SetTarget(shuffle, view);
                sb.Children.Add(shuffle);
                var duration = new Duration(TimeSpan.FromSeconds(0.5 *
                    1d.Inexact(0.2)));

                var translateX = new DoubleAnimation
                {
                    To = Rand.BoundedDouble(cardsView.ActualWidth * 0.8),
                    Duration = duration,
                    AutoReverse = true,
                    AccelerationRatio = 1
                };
                Storyboard.SetTargetProperty(translateX, _translateXPath);
                shuffle.Children.Add(translateX);

                var translateY = new DoubleAnimation
                {
                    To = Rand.BoundedDouble(cardsView.ActualHeight * 0.8),
                    Duration = duration,
                    AutoReverse = true,
                    AccelerationRatio = 1
                };
                Storyboard.SetTargetProperty(translateY, _translateYPath);
                shuffle.Children.Add(translateY);

                var rotateAngle = new DoubleAnimation
                {
                    By = (360d * Rand.Next(1, 3)).MaybeNegate(),
                    Duration = duration.Add(duration),
                    DecelerationRatio = 0.4
                };
                Storyboard.SetTargetProperty(rotateAngle, _rotateAnglePath);
                shuffle.Children.Add(rotateAngle);
            }
        }

        private void DealCards(Deal deal, Storyboard sb)
        {
            Func<PlayerBase, Point> getTranslateTo = p =>
                {
                    double magnitude = 250;
                    switch (p.Location)
                    {
                        case AbsoluteLocation.South:
                            return new Point(0, magnitude);
                        case AbsoluteLocation.West:
                            return new Point(-magnitude, 0);
                        case AbsoluteLocation.North:
                            return new Point(0, -magnitude);
                        case AbsoluteLocation.East:
                            return new Point(magnitude, 0);
                        default:
                            throw new ArgumentOutOfRangeException
                                ("p.Location");
                    }
                };
            Func<PlayerBase, double> getRotateAngleTo =
                p => (int)p.Location * 90;
            var records = deal.Players
                .SelectMany((p, i) => p
                    .Hand
                    .OrderBy(c => c)
                    .Select((c, j) => new
                        {
                            View = _map[c],
                            BeginTime = TimeSpan.FromSeconds
                                (i * 0.1 + j * 0.4),
                            TranslateTo = getTranslateTo(p),
                            RotateAngleTo = getRotateAngleTo(p),
                            PivotAngleTo = j * 4.5 - 27
                        }));
            foreach (var record in records)
            {
                var dealCard = new Storyboard();
                dealCard.BeginTime = record.BeginTime;
                Storyboard.SetTarget(dealCard, record.View);
                sb.Children.Add(dealCard);

                var scaleX = new DoubleAnimation
                {
                    To = 2,
                    Duration = new Duration(TimeSpan.FromSeconds(0.5)),
                    DecelerationRatio = 1,
                    AutoReverse = true
                };
                Storyboard.SetTargetProperty(scaleX, _scaleXPath);
                dealCard.Children.Add(scaleX);

                var scaleY = new DoubleAnimation
                {
                    To = 2,
                    Duration = new Duration(TimeSpan.FromSeconds(0.5)),
                    DecelerationRatio = 1,
                    AutoReverse = true
                };
                Storyboard.SetTargetProperty(scaleY, _scaleYPath);
                dealCard.Children.Add(scaleY);

                var translateX = new DoubleAnimation
                {
                    To = record.TranslateTo.X,
                    Duration = new Duration(TimeSpan.FromSeconds(0.5)),
                    DecelerationRatio = 1
                };
                Storyboard.SetTargetProperty(translateX, _translateXPath);
                dealCard.Children.Add(translateX);

                var translateY = new DoubleAnimation
                {
                    To = record.TranslateTo.Y,
                    Duration = new Duration(TimeSpan.FromSeconds(0.5)),
                    DecelerationRatio = 1
                };
                Storyboard.SetTargetProperty(translateY, _translateYPath);
                dealCard.Children.Add(translateY);

                var rotateAngle = new DoubleAnimation
                {
                    To = record.RotateAngleTo,
                    Duration = new Duration(TimeSpan.FromSeconds(0.5)),
                    DecelerationRatio = 1
                };
                Storyboard.SetTargetProperty(rotateAngle, _rotateAnglePath);
                dealCard.Children.Add(rotateAngle);

                var pivotAngle = new DoubleAnimation
                {
                    To = record.PivotAngleTo,
                    BeginTime = TimeSpan.FromSeconds(0.75),
                    Duration = new Duration(TimeSpan.FromSeconds(0.5)),
                    DecelerationRatio = 1
                };
                Storyboard.SetTargetProperty(pivotAngle, _pivotAnglePath);
                dealCard.Children.Add(pivotAngle);
            }
        }

        private void PassCards(PassCollection info, Storyboard sb)
        {
            double innerMagnitude = 250;
            double outerMagnitude = 500;
            Func<double, double> getOffAxisMagnitude = m =>
                info.Mode == PassingMode.Across ? m : 0;
            Func<double, double> getZero = _ => 0;
            Func<PlayerBase, double, Func<double, double>, Point> 
                getTranslateTo = (p, m, f) =>
                {
                    switch (p.Location)
                    {
                        case AbsoluteLocation.South:
                            return new Point(f(m), m);
                        case AbsoluteLocation.West:
                            return new Point(-m, f(m));
                        case AbsoluteLocation.North:
                            return new Point(-f(m), -m);
                        case AbsoluteLocation.East:
                            return new Point(m, -f(m));
                        default:
                            throw new ArgumentOutOfRangeException
                                ("p.Location");
                    };
                };
            Func<PlayerBase, EasingMode> getTranslateXEasingMode = p =>
                {
                    switch (p.Location)
                    {
                        case AbsoluteLocation.South:
                        case AbsoluteLocation.North:
                            return EasingMode.EaseOut;
                        case AbsoluteLocation.West:
                        case AbsoluteLocation.East:
                            return info.Mode == PassingMode.Across
                                ? EasingMode.EaseInOut
                                : EasingMode.EaseIn;
                        default:
                            throw new ArgumentOutOfRangeException
                                ("p.Location");
                    }
                };
            Func<PlayerBase, EasingMode> getTranslateYEasingMode = p =>
                {
                    switch (p.Location)
                    {
                        case AbsoluteLocation.South:
                        case AbsoluteLocation.North:
                            return info.Mode == PassingMode.Across
                                ? EasingMode.EaseInOut
                                : EasingMode.EaseIn;
                        case AbsoluteLocation.West:
                        case AbsoluteLocation.East:
                            return EasingMode.EaseOut;
                        default:
                            throw new ArgumentOutOfRangeException
                                ("p.Location");
                    }
                };
            double tickSeconds = 0.4;
            var halfTick = TimeSpan.FromSeconds(tickSeconds * 0.5);
            var tick = TimeSpan.FromSeconds(tickSeconds);
            var threeTicks = TimeSpan.FromSeconds(tickSeconds * 3);
            Func<PlayerBase, Duration> getTranslateXDuration = p =>
                {
                    switch (p.Location)
                    {
                        case AbsoluteLocation.South:
                        case AbsoluteLocation.North:
                            return new Duration(threeTicks);
                        case AbsoluteLocation.West:
                        case AbsoluteLocation.East:
                            return info.Mode == PassingMode.Across
                                ? new Duration(threeTicks.Add(threeTicks))
                                : new Duration(threeTicks);
                        default:
                            throw new ArgumentOutOfRangeException
                                ("p.Location");
                    }
                };
            Func<PlayerBase, Duration> getTranslateYDuration = p =>
                {
                    switch (p.Location)
                    {
                        case AbsoluteLocation.South:
                        case AbsoluteLocation.North:
                            return info.Mode == PassingMode.Across
                                ? new Duration(threeTicks.Add(threeTicks))
                                : new Duration(threeTicks);
                        case AbsoluteLocation.West:
                        case AbsoluteLocation.East:
                            return new Duration(threeTicks);
                        default:
                            throw new ArgumentOutOfRangeException
                                ("p.Location");
                    }
                };
            Func<Card, PlayerBase, double> getPivotAngle = (c, p) =>
                {
                    return p
                        .Hand
                        .OrderBy(c1 => c1)
                        .ToList()
                        .IndexOf(c) * 4.5 - 27;
                };
            double rotateAngleBy = 0;
            switch (info.Mode)
            {
                case PassingMode.Left:
                    rotateAngleBy = 90;
                    break;
                case PassingMode.Across:
                    rotateAngleBy = 180;
                    break;
                case PassingMode.Right:
                    rotateAngleBy = -90;
                    break;
                default:
                    throw new ArgumentOutOfRangeException
                        ("info.Mode");
            }
            var passRecords = info
                .Passes
                .SelectMany((p, i) => p
                    .Cards
                    .OrderBy(c => c)
                    .Select((c, j) => new
                        {
                            View = _map[c],
                            BeginTime = TimeSpan.FromSeconds(tickSeconds * j),
                            TranslateToSetup = 
                                getTranslateTo(p.Giver, outerMagnitude, 
                                    getZero),
                            TranslateTo =
                                getTranslateTo(p.Receiver, outerMagnitude, 
                                    getOffAxisMagnitude),
                            TranslateXDuration =
                                getTranslateXDuration(p.Giver),
                            TranslateYDuration =
                                getTranslateYDuration(p.Giver),
                            TranslateXEasing =
                                getTranslateXEasingMode(p.Giver),
                            TranslateYEasing =
                                getTranslateYEasingMode(p.Giver),
                            TranslateXAutoReverse =
                                info.Mode == PassingMode.Across &&
                                (p.Giver.Location == AbsoluteLocation.South ||
                                p.Giver.Location == AbsoluteLocation.North),
                            TranslateYAutoReverse =
                                info.Mode == PassingMode.Across &&
                                (p.Giver.Location == AbsoluteLocation.West ||
                                p.Giver.Location == AbsoluteLocation.East),
                            TranslateToTeardown =
                                getTranslateTo(p.Receiver, innerMagnitude, 
                                    getZero),
                            PivotAngleReceiveTo =
                                getPivotAngle(c, p.Receiver)
                        }));
            foreach (var record in passRecords)
            {
                var passCard = new Storyboard();
                Storyboard.SetTarget(passCard, record.View);
                sb.Children.Add(passCard);

                var translateXSetup = new DoubleAnimation
                {
                    To = record.TranslateToSetup.X,
                    BeginTime = record.BeginTime,
                    Duration = new Duration(tick.Add(halfTick)),
                    EasingFunction = 
                        new SineEase { EasingMode = EasingMode.EaseOut }
                };
                Storyboard.SetTargetProperty(translateXSetup, _translateXPath);
                passCard.Children.Add(translateXSetup);

                var translateYSetup = new DoubleAnimation
                {
                    To = record.TranslateToSetup.Y,
                    BeginTime = record.BeginTime,
                    Duration = new Duration(tick.Add(halfTick)),
                    EasingFunction = 
                        new SineEase { EasingMode = EasingMode.EaseOut }
                };
                Storyboard.SetTargetProperty(translateYSetup, _translateYPath);
                passCard.Children.Add(translateYSetup);

                var scaleXUp = new DoubleAnimation
                {
                    To = 1.5,
                    BeginTime = record.BeginTime,
                    Duration = new Duration(tick),
                    DecelerationRatio = 1
                };
                Storyboard.SetTargetProperty(scaleXUp, _scaleXPath);
                passCard.Children.Add(scaleXUp);

                var scaleYUp = new DoubleAnimation
                {
                    To = 1.5,
                    BeginTime = record.BeginTime,
                    Duration = new Duration(tick),
                    DecelerationRatio = 1
                };
                Storyboard.SetTargetProperty(scaleYUp, _scaleYPath);
                passCard.Children.Add(scaleYUp);

                var pivotAngleGive = new DoubleAnimation
                {
                    To = 0,
                    BeginTime = record.BeginTime,
                    Duration = new Duration(tick.Add(halfTick)),
                    DecelerationRatio = 1
                };
                Storyboard.SetTargetProperty(pivotAngleGive, _pivotAnglePath);
                passCard.Children.Add(pivotAngleGive);

                var translateX = new DoubleAnimation
                {
                    To = record.TranslateTo.X,
                    BeginTime = record.BeginTime.Add(tick),
                    Duration = record.TranslateXDuration,
                    EasingFunction = new SineEase
                    {
                        EasingMode = record.TranslateXEasing
                    },
                    AutoReverse = record.TranslateXAutoReverse
                };
                Storyboard.SetTargetProperty(translateX, _translateXPath);
                passCard.Children.Add(translateX);

                var translateY = new DoubleAnimation
                {
                    To = record.TranslateTo.Y,
                    BeginTime = record.BeginTime.Add(tick),
                    Duration = record.TranslateYDuration,
                    EasingFunction = new SineEase
                    {
                        EasingMode = record.TranslateYEasing
                    },
                    AutoReverse = record.TranslateYAutoReverse
                };
                Storyboard.SetTargetProperty(translateY, _translateYPath);
                passCard.Children.Add(translateY);

                var rotateAngle = new DoubleAnimation
                {
                    By = rotateAngleBy,
                    BeginTime = record.BeginTime.Add(tick),
                    Duration = info.Mode == PassingMode.Across
                        ? new Duration(threeTicks.Add(threeTicks))
                        : new Duration(threeTicks)
                };
                Storyboard.SetTargetProperty(rotateAngle, _rotateAnglePath);
                passCard.Children.Add(rotateAngle);

                var translateXTeardown = new DoubleAnimation
                {
                    To = record.TranslateToTeardown.X,
                    BeginTime = record.BeginTime
                        .Add(TimeSpan.FromSeconds(tickSeconds *
                            (info.Mode == PassingMode.Across
                                ? 7
                                : 4))),
                    Duration = new Duration(tick),
                    DecelerationRatio = 1
                };
                Storyboard.SetTargetProperty(translateXTeardown, _translateXPath);
                passCard.Children.Add(translateXTeardown);

                var translateYTeardown = new DoubleAnimation
                {
                    To = record.TranslateToTeardown.Y,
                    BeginTime = record.BeginTime
                        .Add(TimeSpan.FromSeconds(tickSeconds *
                            (info.Mode == PassingMode.Across
                                ? 7
                                : 4))),
                    Duration = new Duration(tick),
                    DecelerationRatio = 1
                };
                Storyboard.SetTargetProperty(translateYTeardown, _translateYPath);
                passCard.Children.Add(translateYTeardown);

                var scaleXDown = new DoubleAnimation
                {
                    To = 1,
                    BeginTime = record.BeginTime
                        .Add(TimeSpan.FromSeconds(tickSeconds *
                            (info.Mode == PassingMode.Across
                                ? 7
                                : 4))),
                    Duration = new Duration(tick),
                    DecelerationRatio = 1
                };
                Storyboard.SetTargetProperty(scaleXDown, _scaleXPath);
                passCard.Children.Add(scaleXDown);

                var scaleYDown = new DoubleAnimation
                {
                    To = 1,
                    BeginTime = record.BeginTime
                        .Add(TimeSpan.FromSeconds(tickSeconds *
                            (info.Mode == PassingMode.Across
                                ? 7
                                : 4))),
                    Duration = new Duration(tick),
                    DecelerationRatio = 1
                };
                Storyboard.SetTargetProperty(scaleYDown, _scaleYPath);
                passCard.Children.Add(scaleYDown);

                var pivotAngleReceive = new DoubleAnimation
                {
                    To = record.PivotAngleReceiveTo,
                    BeginTime = record.BeginTime
                        .Add(TimeSpan.FromSeconds(tickSeconds *
                            (info.Mode == PassingMode.Across
                                ? 7
                                : 4))),
                    Duration = new Duration(tick),
                    DecelerationRatio = 1
                };
                Storyboard.SetTargetProperty
                    (pivotAngleReceive, _pivotAnglePath);
                passCard.Children.Add(pivotAngleReceive);
            }
            var handRecords = info
                .Passes
                .SelectMany(p => p
                    .Receiver
                    .Hand
                    .OrderBy(c => c)
                    .Select(c => new
                        {
                            Card = c,
                            View = _map[c],
                            PivotAngleTo = getPivotAngle(c, p.Receiver)
                        })
                    .Where(x => !p
                        .Cards
                        .Contains(x.Card)));
            foreach (var record in handRecords)
            {
                var shiftCard = new Storyboard();
                Storyboard.SetTarget(shiftCard, record.View);
                sb.Children.Add(shiftCard);

                var pivotAngle = new DoubleAnimation
                {
                    To = record.PivotAngleTo,
                    BeginTime = TimeSpan.FromSeconds(tickSeconds *
                        (info.Mode == PassingMode.Across
                            ? 7
                            : 4)),
                    Duration = new Duration(threeTicks),
                    DecelerationRatio = 1
                };
                Storyboard.SetTargetProperty
                    (pivotAngle, _pivotAnglePath);
                shiftCard.Children.Add(pivotAngle);
            }
        }

        private void PlayTrick(Trick trick, Storyboard sb)
        {
            Func<PlayerBase, double, Point> getTranslateTo = (p, m) =>
                {
                    switch (p.Location)
                    {
                        case AbsoluteLocation.South:
                            return new Point(0, m);
                        case AbsoluteLocation.West:
                            return new Point(-m, 0);
                        case AbsoluteLocation.North:
                            return new Point(0, -m);
                        case AbsoluteLocation.East:
                            return new Point(m, 0);
                        default:
                            throw new ArgumentOutOfRangeException
                                ("p.Location");
                    }
                };
            Point translateTakeTo = new Point();
            switch (trick.WinningPlay.Player.Location)
            {
                case AbsoluteLocation.South:
                    translateTakeTo = new Point(240, 450);
                    break;
                case AbsoluteLocation.West:
                    translateTakeTo = new Point(-550, 170);
                    break;
                case AbsoluteLocation.North:
                    translateTakeTo = new Point(-240, -450);
                    break;
                case AbsoluteLocation.East:
                    translateTakeTo = new Point(550, -170);
                    break;
                default:
                    throw new ArgumentOutOfRangeException
                        ("trick.WinningPlay.Player.Location");
            }
            var playRecords = trick
                .Plays
                .Select((p, i) => new
                    {
                        View = _map[p.Card],
                        BeginTime = TimeSpan.FromSeconds(i * 0.2),
                        PivotAngleDrawBy = 
                            p.Player.Hand.Any(c => p.Card.CompareTo(c) < 0)
                            ? -13.5
                            : 0,
                        TranslatePlayTo = getTranslateTo(p.Player, 230),
                        ScaleSwellBy = p == trick.WinningPlay ? 0.8 : 0,
                        ZIndex = Card.Deck.Count + 
                            2 * (13 - p.Player.Hand.Count) +
                            (p == trick.WinningPlay ? 1 : 0)
                    });
            foreach (var record in playRecords)
            {
                var playCard = new Storyboard();
                playCard.BeginTime = record.BeginTime;
                Storyboard.SetTarget(playCard, record.View);
                sb.Children.Add(playCard);

                var pivotAngleDraw = new DoubleAnimation
                {
                    By = record.PivotAngleDrawBy,
                    Duration = new Duration(TimeSpan.FromSeconds(0.4)),
                    DecelerationRatio = 1
                };
                Storyboard.SetTargetProperty(pivotAngleDraw, _pivotAnglePath);
                playCard.Children.Add(pivotAngleDraw);

                var zIndex = new Int32AnimationUsingKeyFrames();
                zIndex.KeyFrames.Add(new DiscreteInt32KeyFrame(
                    Panel.GetZIndex(record.View)));
                zIndex.KeyFrames.Add(new DiscreteInt32KeyFrame(
                    record.ZIndex,
                    KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.4))));
                Storyboard.SetTargetProperty(zIndex, _zIndexPath);
                playCard.Children.Add(zIndex);

                var scaleXUp = new DoubleAnimation
                {
                    To = 2,
                    BeginTime = TimeSpan.FromSeconds(0.5),
                    Duration = new Duration(TimeSpan.FromSeconds(0.5)),
                    DecelerationRatio = 1
                };
                Storyboard.SetTargetProperty(scaleXUp, _scaleXPath);
                playCard.Children.Add(scaleXUp);

                var scaleYUp = new DoubleAnimation
                {
                    To = 2,
                    BeginTime = TimeSpan.FromSeconds(0.5),
                    Duration = new Duration(TimeSpan.FromSeconds(0.5)),
                    DecelerationRatio = 1
                };
                Storyboard.SetTargetProperty(scaleYUp, _scaleYPath);
                playCard.Children.Add(scaleYUp);

                var pivotAnglePlay = new DoubleAnimation
                {
                    To = 0,
                    BeginTime = TimeSpan.FromSeconds(0.5),
                    Duration = new Duration(TimeSpan.FromSeconds(0.5)),
                    DecelerationRatio = 1
                };
                Storyboard.SetTargetProperty(pivotAnglePlay, _pivotAnglePath);
                playCard.Children.Add(pivotAnglePlay);

                var translateXPlay = new DoubleAnimation
                {
                    To = record.TranslatePlayTo.X,
                    BeginTime = TimeSpan.FromSeconds(0.5),
                    Duration = new Duration(TimeSpan.FromSeconds(0.3)),
                    AccelerationRatio = 0.3
                };
                Storyboard.SetTargetProperty(translateXPlay, _translateXPath);
                playCard.Children.Add(translateXPlay);

                var translateYPlay = new DoubleAnimation
                {
                    To = record.TranslatePlayTo.Y,
                    BeginTime = TimeSpan.FromSeconds(0.5),
                    Duration = new Duration(TimeSpan.FromSeconds(0.3)),
                    AccelerationRatio = 0.3
                };
                Storyboard.SetTargetProperty(translateYPlay, _translateYPath);
                playCard.Children.Add(translateYPlay);

                var scaleXSwell = new DoubleAnimation
                {
                    By = record.ScaleSwellBy,
                    BeginTime = TimeSpan.FromSeconds(1.6)
                        .Subtract(record.BeginTime),
                    Duration = new Duration(TimeSpan.FromSeconds(0.3)),
                    EasingFunction = 
                        new SineEase { EasingMode = EasingMode.EaseOut }
                };
                Storyboard.SetTargetProperty(scaleXSwell, _scaleXPath);
                playCard.Children.Add(scaleXSwell);

                var scaleYSwell = new DoubleAnimation
                {
                    By = record.ScaleSwellBy,
                    BeginTime = TimeSpan.FromSeconds(1.6)
                        .Subtract(record.BeginTime),
                    Duration = new Duration(TimeSpan.FromSeconds(0.3)),
                    EasingFunction =
                        new SineEase { EasingMode = EasingMode.EaseOut }
                };
                Storyboard.SetTargetProperty(scaleYSwell, _scaleYPath);
                playCard.Children.Add(scaleYSwell);

                var translateXTake = new DoubleAnimation
                {
                    To = translateTakeTo.X.Inexact(0.15),
                    BeginTime = TimeSpan.FromSeconds(2.2)
                        .Subtract(record.BeginTime),
                    Duration = new Duration(TimeSpan.FromSeconds(0.5)),
                    DecelerationRatio = 0.6
                };
                Storyboard.SetTargetProperty(translateXTake, _translateXPath);
                playCard.Children.Add(translateXTake);

                var translateYTake = new DoubleAnimation
                {
                    To = translateTakeTo.Y.Inexact(0.15),
                    BeginTime = TimeSpan.FromSeconds(2.2)
                        .Subtract(record.BeginTime),
                    Duration = new Duration(TimeSpan.FromSeconds(0.5)),
                    DecelerationRatio = 0.6
                };
                Storyboard.SetTargetProperty(translateYTake, _translateYPath);
                playCard.Children.Add(translateYTake);

                var scaleXDown = new DoubleAnimation
                {
                    To = 1,
                    BeginTime = TimeSpan.FromSeconds(2.2)
                        .Subtract(record.BeginTime),
                    Duration = new Duration(TimeSpan.FromSeconds(0.2)),
                    AccelerationRatio = 1
                };
                Storyboard.SetTargetProperty(scaleXDown, _scaleXPath);
                playCard.Children.Add(scaleXDown);

                var scaleYDown = new DoubleAnimation
                {
                    To = 1,
                    BeginTime = TimeSpan.FromSeconds(2.2)
                        .Subtract(record.BeginTime),
                    Duration = new Duration(TimeSpan.FromSeconds(0.2)),
                    AccelerationRatio = 1
                };
                Storyboard.SetTargetProperty(scaleYDown, _scaleYPath);
                playCard.Children.Add(scaleYDown);

                var rotateAngle = new DoubleAnimation
                {
                    By = Rand.BoundedDouble(540).MaybeNegate(),
                    BeginTime = TimeSpan.FromSeconds(2.2)
                        .Subtract(record.BeginTime),
                    Duration = 
                        new Duration(TimeSpan.FromSeconds(0.7.Inexact(0.2))),
                    DecelerationRatio = 0.85
                };
                Storyboard.SetTargetProperty(rotateAngle, _rotateAnglePath);
                playCard.Children.Add(rotateAngle);
            }
            var handRecords = trick
                .PlayersStartingWithLead
                .SelectMany(p => p
                    .Hand
                    .OrderBy(c => c)
                    .Select((c, i) => new
                        {
                            View = _map[c],
                            PivotAngleTo = i * 4.5 - 27 + 
                                2.25 * (13 - p.Hand.Count)
                        }));
            foreach (var record in handRecords)
            {
                var shiftCard = new Storyboard();
                shiftCard.BeginTime = TimeSpan.FromSeconds(2.2);
                Storyboard.SetTarget(shiftCard, record.View);
                sb.Children.Add(shiftCard);

                var pivotAngle = new DoubleAnimation
                {
                    To = record.PivotAngleTo,
                    Duration = new Duration(TimeSpan.FromSeconds(0.4)),
                    DecelerationRatio = 1
                };
                Storyboard.SetTargetProperty(pivotAngle, _pivotAnglePath);
                shiftCard.Children.Add(pivotAngle);
            }
        }
    }
}
