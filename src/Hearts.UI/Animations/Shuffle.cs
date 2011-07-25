using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Animation;
using Hearts.Models;
using UI.Views;
using System.Windows;
using Hearts.Utility;

namespace UI.Animations
{
    public class Shuffle : CardsAnimationBase<object>
    {
        public Shuffle(Dictionary<Card, CardView> cardMap)
            : base(cardMap)
        { }

        public override Storyboard Construct(object ignore)
        {
            var sb = new Storyboard();
            foreach (var view in _cardMap.Values)
            {
                var shuffle = new Storyboard();
                Storyboard.SetTarget(shuffle, view);
                sb.Children.Add(shuffle);
                var duration = new Duration(TimeSpan.FromSeconds(0.5 *
                    Rand.NextDouble(0.8, 1.2)));

                var translateX = new DoubleAnimation
                {
                    //To = Rand.BoundedDouble(cardsView.ActualWidth * 0.8),
                    Duration = duration,
                    AutoReverse = true,
                    AccelerationRatio = 1
                };
                Storyboard.SetTargetProperty(translateX, _translateXPath);
                shuffle.Children.Add(translateX);

                var translateY = new DoubleAnimation
                {
                    //To = Rand.BoundedDouble(cardsView.ActualHeight * 0.8),
                    Duration = duration,
                    AutoReverse = true,
                    AccelerationRatio = 1
                };
                Storyboard.SetTargetProperty(translateY, _translateYPath);
                shuffle.Children.Add(translateY);

                var rotateAngle = new DoubleAnimation
                {
                    By = 360 * Rand.Next(1, 3) * (Rand.NextBool() ? 1 : -1),
                    Duration = duration.Add(duration),
                    DecelerationRatio = 0.4
                };
                Storyboard.SetTargetProperty(rotateAngle, _rotateAnglePath);
                shuffle.Children.Add(rotateAngle);
            }
            return sb;
        }
    }
}
