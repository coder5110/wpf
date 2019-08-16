using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Bot.ViewModels
{
    public class AsyncCommand<TResult>: AsyncCommandBase, INotifyPropertyChanged
    {

        #region Constructors

        public AsyncCommand(Func<object, CancellationToken, Task<TResult>> actionFunc)
        {
            action = actionFunc;
            m_cancelCommand = new CancelAsyncCommand();
        }
        
        #endregion

        #region Properties

        public TaskCompletionNotifier<TResult> Execution
        {
            get
            {
                return m_execution;
            }

            private set
            {
                m_execution = value;
                OnPropertyChanged();
            }
        }

        public ICommand CancelCommand => m_cancelCommand;


        #endregion

        #region Methods

        public override bool CanExecute(object parameter)
        {
            return Execution == null || Execution.IsCompleted || m_cancelCommand.CancelToken.IsCancellationRequested;
        }

        public override async Task ExecuteAsync(object parameter)
        {
            await m_semaphore.WaitAsync();

            m_cancelCommand.NotifyCommandStarting();
            Execution = new TaskCompletionNotifier<TResult>(action(parameter, m_cancelCommand.CancelToken));
            Execution.Run();
            TaskCompletionNotifier<TResult> me = Execution;
            RaiseCanExecuteChanged();

            m_semaphore.Release();

            await Execution.TaskCompletion;

            await m_semaphore.WaitAsync();

            if (me == Execution)
            {
                m_cancelCommand.NotifyCommandCompleted();
                RaiseCanExecuteChanged();
            }

            m_semaphore.Release();
        }

        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion

        #region Fields

        private TaskCompletionNotifier<TResult> m_execution = null;
        private readonly Func<object, CancellationToken, Task<TResult>> action = null;
        private readonly CancelAsyncCommand m_cancelCommand;
        private readonly SemaphoreSlim m_semaphore = new SemaphoreSlim(1, 1);

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }

    public class CancelAsyncCommand : ICommand
    {
        #region Constructors



        #endregion

        #region Properties

        public CancellationToken CancelToken => m_cts.Token;

        #endregion

        #region Methods

        public bool CanExecute(object parameter)
        {
            return m_isCommandExecuting && !m_cts.IsCancellationRequested;
        }

        public void Execute(object parameter)
        {
            m_cts.Cancel();
            RaiseCanExecuteChanged();
        }

        public void NotifyCommandStarting()
        {
            m_isCommandExecuting = true;
            if (m_cts.IsCancellationRequested)
            {
                m_cts = new CancellationTokenSource();
                RaiseCanExecuteChanged();
            }
        }

        public void NotifyCommandCompleted()
        {
            m_isCommandExecuting = false;
            RaiseCanExecuteChanged();
        }

        private void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        #endregion

        #region Fields

        private CancellationTokenSource m_cts = new CancellationTokenSource();
        private bool m_isCommandExecuting = false;

        #endregion

        #region Events

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        #endregion
    }

    public static class AsyncCommand
    {
        #region Methods

        //public static AsyncCommand<object> Create(Func<Task> action)
        //{
        //    return new AsyncCommand<object>(async _ => { await action(); return null; });
        //}

        //public static AsyncCommand<TResult> Create<TResult>(Func<Task<TResult>> action)
        //{
        //    return new AsyncCommand<TResult>(_ => action());
        //}

        public static AsyncCommand<object> Create(Func<object, CancellationToken, Task> action)
        {
            return new AsyncCommand<object>(async (parameter, token) => { await action(parameter, token); return null; });
        }

        public static AsyncCommand<TResult> Create<TResult>(Func<object, CancellationToken, Task<TResult>> action)
        {
            return new AsyncCommand<TResult>(action);
        }

        #endregion
    }
}
