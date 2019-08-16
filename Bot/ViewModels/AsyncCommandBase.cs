using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Bot.ViewModels
{
    public abstract class AsyncCommandBase: IAsyncCommand
    {
        #region Methods

        public abstract bool CanExecute(object parameter);

        public abstract Task ExecuteAsync(object parameter);

        public async void Execute(object parameter)
        {
            await ExecuteAsync(parameter);
        }

        protected void RaiseCanExecuteChanged()
        {
            Application.Current.Dispatcher.Invoke(CommandManager.InvalidateRequerySuggested);
        }

        #endregion

        #region Events

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        #endregion
    }
}
