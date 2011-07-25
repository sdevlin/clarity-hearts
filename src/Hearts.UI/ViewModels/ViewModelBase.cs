using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Linq.Expressions;
using Hearts.Messages;
using System.Windows.Threading;

namespace UI.ViewModels
{
    public abstract class ViewModelBase<TViewModel> : 
        INotifyPropertyChanged
            where TViewModel : ViewModelBase<TViewModel>
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected void OnPropertyChanged<TProperty>
            (Expression<Func<TViewModel, TProperty>> expr)
        {
            var body = expr.Body as MemberExpression;
            if (body == null)
            {
                throw new ArgumentException
                    ("expr.Body must be a MemberExpression");
            }
            OnPropertyChanged(body.Member.Name);
        }
    }
}
