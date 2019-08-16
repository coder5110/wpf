using System;
using System.Windows;
using System.Windows.Input;

namespace Bot.ViewModels
{
    public class DelegateCommand: ICommand
    {
        #region Contructors

        public DelegateCommand(Action<object> executeAction, Func<object, bool> canExecuteAction = null)
        {
            m_executeAction = executeAction;
            if (canExecuteAction == null)
            {
                canExecuteAction = o => true;
            }
            m_canExecuteAction = canExecuteAction;
        }

        #endregion

        #region Methods
        
        public bool CanExecute(object parameter)
        {
            return m_canExecuteAction(parameter);
        }

        public void Execute(object parameter)
        {
            m_executeAction(parameter);

            Application.Current.Dispatcher.Invoke(CommandManager.InvalidateRequerySuggested);
        }

        #endregion

        #region Fields

        private readonly Action<object> m_executeAction;
        private readonly Func<object, bool> m_canExecuteAction;

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
