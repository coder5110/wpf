using System;
using System.ComponentModel;

namespace Bot.ViewModels
{
    public abstract class ViewModelBase: BindableObject
    {
        #region Constructors



        #endregion

        #region Properties



        #endregion

        #region Methods

        protected abstract void PropertyChangedHandler(object sender, PropertyChangedEventArgs args);

        protected void AttachModel(object model)
        {
            if (model == null)
            {
                throw new ArgumentNullException("Model is null");
            }

            INotifyPropertyChanged o = model as INotifyPropertyChanged;

            if (o != null)
            {
                o.PropertyChanged += PropertyChangedHandler;
            }
            else
            {
                throw new ArgumentException("The model doesn't implement INotifyPropertyChanged interface");
            }
        }

        protected void DeattachModel(object model)
        {
            if (model == null)
            {
                throw new ArgumentNullException("Model is null");
            }

            INotifyPropertyChanged o = model as INotifyPropertyChanged;

            if (o != null)
            {
                o.PropertyChanged -= PropertyChangedHandler;
            }
            else
            {
                throw new ArgumentException("The model doesn't implement INotifyPropertyChanged interface");
            }
        }

        #endregion

        #region Fields



        #endregion
    }
}
