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
using UI.ViewModels;
using Hearts.Models;
using Hearts.Utility;
using System.Windows.Media.Animation;

namespace UI.Views
{
    public partial class CardView : UserControl
    {
        public CardView(Card card, Point initialLocation)
        {
            InitializeComponent();
            DataContext = card;
            var path = new Uri(
                string.Format("/resources/images/cards/{0}_{1}.png",
                    card.Rank == Rank.Ace
                        ? 1
                        : (int)card.Rank + 2, 
                    card.Suit), 
                    UriKind.Relative);
            img.Source = new BitmapImage(path);
            Loaded += (sender, e) =>
            {
                center.X = initialLocation.X - ActualWidth / 2;
                center.Y = initialLocation.Y - ActualHeight / 2;
            };
        }
    }
}
