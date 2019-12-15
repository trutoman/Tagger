using System;
using System.Windows.Input;

namespace Tagger.Core.Commands
{
    public class ParamCommand : ICommand
    {
        private Action<object> _action;
        private readonly Func<bool> _canExecute;

        public ParamCommand(Action<object> action)
        {
            _action = action;
            _canExecute = () => true;
        }

        public ParamCommand(Action<object> action, Func<bool> canExecute)
        {
            _action = action;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            if (this._canExecute == null)
            {
                return true;
            }
            else
            {
                bool result = this._canExecute.Invoke();
                return result;
            }
        }

        public void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                if (parameter != null)
                {
                    _action(parameter);
                }
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

    }
}
