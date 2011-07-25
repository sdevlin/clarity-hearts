using System.Windows.Controls;
using Hearts.Messages;

namespace UI.Views
{
    public class ActiveView : UserControl
    {
        protected BlockingMediator Mediator { get; private set; }

        public ActiveView()
        {
            // intentionally fails hard if dc is not a provider
            Loaded += (sender, e) => Mediator =
                ((IMediatorProvider<BlockingMediator>)DataContext).Mediator;
        }
    }
}
