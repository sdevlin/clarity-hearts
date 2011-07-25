using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using UI.Views;
using UI.ViewModels;
using Hearts.Messages;

namespace UI
{
    public partial class App : Application
    {
        private void OnStartup(object sender, StartupEventArgs e)
        {
            var view = new MainView();
            view.DataContext = new MainViewModel(new BlockingMediator());
            view.Show();
        }
    }
}
