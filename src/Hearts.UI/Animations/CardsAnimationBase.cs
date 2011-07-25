using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Hearts.Models;
using UI.Views;

namespace UI.Animations
{
    public abstract class CardsAnimationBase<T>
    {
        protected readonly Dictionary<Card, CardView> _cardMap;

        public CardsAnimationBase(Dictionary<Card, CardView> cardMap)
        {
            _cardMap = cardMap;
        }

        public abstract Storyboard Construct(T info);

        protected static readonly PropertyPath _scaleXPath;
        protected static readonly PropertyPath _scaleYPath;
        protected static readonly PropertyPath _pivotAnglePath;
        protected static readonly PropertyPath _rotateAnglePath;
        protected static readonly PropertyPath _translateXPath;
        protected static readonly PropertyPath _translateYPath;
        static CardsAnimationBase()
        {
            Func<int, DependencyProperty, PropertyPath> initPath = (i, prop) =>
                new PropertyPath(string.Format("(0).(1)[{0}].(2)", i),
                    new[]
                    {
                        UIElement.RenderTransformProperty, 
                        TransformGroup.ChildrenProperty, 
                        prop
                    });
            _scaleXPath = initPath(0, ScaleTransform.ScaleXProperty);
            _scaleYPath = initPath(0, ScaleTransform.ScaleYProperty);
            _pivotAnglePath = initPath(1, RotateTransform.AngleProperty);
            _rotateAnglePath = initPath(2, RotateTransform.AngleProperty);
            _translateXPath = initPath(4, TranslateTransform.XProperty);
            _translateYPath = initPath(4, TranslateTransform.YProperty);
        }
    }
}
