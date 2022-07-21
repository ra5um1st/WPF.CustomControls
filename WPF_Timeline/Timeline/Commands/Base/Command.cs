using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace WPF_Timeline.Timeline.Commands
{
    abstract class Command : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public virtual bool CanExecute(object parameter)
        {
            return true;
        }

        public abstract void Execute(object parameter);

        protected void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, new EventArgs());
        }
    }
}
