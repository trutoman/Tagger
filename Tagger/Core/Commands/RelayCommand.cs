using System;
using System.Windows.Input;

namespace Tagger.Core.Commands
{
    public class RelayCommand : ICommand
    {
        private Action _action;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action action, Func<bool> canExecute)
        {
            this._action = action;
            this._canExecute = canExecute;
        }

        public RelayCommand(Action action)
        {
            this._action = action;
            this._canExecute = () => true;
        }

        public bool CanExecute(object parameter)
        {
            bool result = this._canExecute.Invoke();
            return result;
        }

        public void Execute(object parameter)
        {
            this._action.Invoke();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
