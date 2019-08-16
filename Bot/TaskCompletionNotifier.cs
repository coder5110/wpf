using System;
using System.Threading.Tasks;

namespace Bot
{
    public class TaskCompletionNotifier<TResult>: BindableObject
    {
        #region Constructors

        public TaskCompletionNotifier(Task<TResult> task)
        {
            Task = task;
        }

        #endregion

        #region Properties

        public readonly Task<TResult> Task;
        public Task TaskCompletion { get; protected set; }

        public TResult Result => (Task.Status == TaskStatus.RanToCompletion) ? Task.Result : default(TResult);

        public TaskStatus Status => Task.Status;

        public bool IsCompleted => Task.IsCompleted;

        public bool IsNotCompleted => !Task.IsCompleted;

        public bool IsSuccessfullyCompleted => Task.Status == TaskStatus.RanToCompletion;

        public bool IsCanceled => Task.IsCanceled;

        public bool IsFaulted => Task.IsFaulted;

        public AggregateException Exception => Task.Exception;

        public Exception InnerException => Exception?.InnerException;

        public string ErrorMessage => InnerException?.Message;

        #endregion

        #region Methods

        public void Run()
        {
            TaskCompletion = Watch(Task);
        }

        private async Task Watch(Task<TResult> task)
        {
            try
            {
                await task;
            }
            catch (Exception e)
            {
                ;
            }

            OnPropertyChanged("Status");
            OnPropertyChanged("IsCompleted");
            OnPropertyChanged("IsNotCompleted");

            if (task.IsCanceled)
            {
                OnPropertyChanged("IsCanceled");
            }
            else if (task.IsFaulted)
            {
                OnPropertyChanged("IsFaulted");
                OnPropertyChanged("Exception");
                OnPropertyChanged("InnerException");
                OnPropertyChanged("ErrorMessage");
            }
            else
            {
                OnPropertyChanged("IsSuccessfullyCompleted");
                OnPropertyChanged("Result");
            }
        }

        #endregion

        #region Fields



        #endregion
    }
}
