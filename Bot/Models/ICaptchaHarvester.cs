using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Models
{
    public interface ICaptchaHarvester
    {
        #region Properties

        int TasksInQueue { get; }
        ObservableCollection<ICaptchaSolutionSource> SolutionSources { get; }
        bool IsEnabled { get; set; }

        #endregion

        #region Methods

        void GetSolution(ICaptchaHarvesterTask task);

        #endregion
    }

    public interface ICaptchaHarvesterTask
    {
        #region Properties

        ManualResetEvent SolutionReadyEvent { get; }
        object Parameter { get; }
        object Solution { get; set; }

        #endregion
    }

    public interface ICaptchaSolutionSource
    {
        #region Properties

        bool IsEnabled { get; }
        CancellationToken CancelToken { get; set; }

        #endregion

        #region Methods

        ICaptchaSolutionSourceTask GetSolution(object parameter);


        #endregion
    }

    public interface ICaptchaSolutionSourceTask
    {
        #region Properties

        object Solution { get; }

        #endregion

        #region Events

        event CaptchaSolutionReleasedEvent SolutionReleased;

        #endregion
    }

    public delegate void CaptchaSolutionReleasedEvent(object sender, EventArgs args);
}
