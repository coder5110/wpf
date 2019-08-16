using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Bot
{
    public class BindableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName]string name = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        protected bool SetProperty<T>(ref T field, T value, object lockObj = null, bool notify = true, [CallerMemberName] string name = null)
        {
            bool changed = false;

            if (lockObj != null)
            {
                lock (lockObj)
                {
                    changed = DoSetProperty(ref field, value);
                }
            }
            else
            {
                changed = DoSetProperty(ref field, value);
            }

            if (changed && notify)
            {
                OnPropertyChanged(name);
            }

            return changed;
        }

        private bool DoSetProperty<T>(ref T field, T value)
        {
            bool changed = !EqualityComparer<T>.Default.Equals(field, value);

            if (changed)
            {
                field = value;
            }

            return changed;
        }
    }
}