using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Bot.Models;
using Bot.Views;

namespace Bot.ViewModels
{
    public class ReleaseCheckoutProfileManagerViewModel: ViewModelBase
    {
        #region Constructors

        public ReleaseCheckoutProfileManagerViewModel()
        {
            AddNew = new DelegateCommand(parameter =>
            {
                ReleaseCheckoutProfileEditor editor = null;
                ReleaseCheckoutProfile profile = new ReleaseCheckoutProfile();

                while (true)
                {
                    editor = new ReleaseCheckoutProfileEditor()
                    {
                        DataContext = new ReleaseCheckoutProfileEditViewModel()
                        {
                            Model = profile,
                            CheckoutProfiles = CheckoutProfiles,
                            MaxTasksCount = MaxTasksCount - GetCurentTasksCount()
                        }
                    };

                    if (editor.ShowDialog() ?? false)
                    {
                        m_model.Add(profile);

                        break;
                    }
                    else
                    {
                        break;
                    }
                }
            });

            Edit = new DelegateCommand(parameter =>
            {
                ReleaseCheckoutProfileEditor editor = null;
                ReleaseCheckoutProfile originalProfile = SelectedProfiles[0].Model;
                ReleaseCheckoutProfile profile = new ReleaseCheckoutProfile(originalProfile);

                while (true)
                {
                    editor = new ReleaseCheckoutProfileEditor()
                    {
                        DataContext = new ReleaseCheckoutProfileEditViewModel()
                        {
                            Model = profile,
                            CheckoutProfiles = CheckoutProfiles,
                            MaxTasksCount = MaxTasksCount - GetCurentTasksCount() + originalProfile.TasksCount
                        }
                    };

                    if (editor.ShowDialog() ?? false)
                    {
                        m_model[m_model.IndexOf(originalProfile)] = profile;

                        break;
                    }
                    else
                    {
                        break;
                    }
                }
            },
            parameter => SelectedProfiles.Count == 1);

            Duplicate = new DelegateCommand(parameter =>
            {
                ReleaseCheckoutProfileEditor editor = null;
                ReleaseCheckoutProfile profile = new ReleaseCheckoutProfile(SelectedProfiles[0].Model);

                while (true)
                {
                    editor = new ReleaseCheckoutProfileEditor()
                    {
                        DataContext = new ReleaseCheckoutProfileEditViewModel()
                        {
                            Model = profile,
                            CheckoutProfiles = CheckoutProfiles,
                            MaxTasksCount = MaxTasksCount - GetCurentTasksCount()
                        }
                    };

                    if (editor.ShowDialog() ?? false)
                    {
                        m_model.Add(profile);

                        break;
                    }
                    else
                    {
                        break;
                    }
                }
            },
            parameter => SelectedProfiles.Count == 1);

            Delete = new DelegateCommand(parameter =>
            {
                foreach (ReleaseCheckoutProfileRecordViewModel profile in SelectedProfiles.ToList())
                {
                    m_model.Remove(profile.Model);
                }
            },
            parameter => SelectedProfiles.Count > 0);
        }

        #endregion

        #region Properties

        public ObservableCollection<ReleaseCheckoutProfile> Model
        {
            get { return m_model; }

            set
            {
                if (m_model != null)
                {
                    m_model.CollectionChanged -= NotifyCollectionChangedHandler;
                }

                if (SetProperty(ref m_model, value))
                {
                    Update();
                }

                if (m_model != null)
                {
                    m_model.CollectionChanged += NotifyCollectionChangedHandler;
                }
            }
        }

        public ObservableCollection<ReleaseCheckoutProfileRecordViewModel> Profiles { get; } = new ObservableCollection<ReleaseCheckoutProfileRecordViewModel>();
        public ObservableCollection<ReleaseCheckoutProfileRecordViewModel> SelectedProfiles { get; } = new ObservableCollection<ReleaseCheckoutProfileRecordViewModel>();

        public ObservableCollection<CheckoutProfile> CheckoutProfiles
        {
            get { return m_checkoutProfiles; }

            set { SetProperty(ref m_checkoutProfiles, value); }
        }

        public int MaxTasksCount { get; set; } = 0;

        #endregion

        #region Commands

        public ICommand AddNew { get; protected set; }
        public ICommand Edit { get; protected set; }
        public ICommand Duplicate { get; protected set; }
        public ICommand Delete { get; protected set; }

        #endregion

        #region Methods

        protected override void PropertyChangedHandler(object sender, PropertyChangedEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void Update()
        {
            Profiles.Clear();

            foreach (ReleaseCheckoutProfile profile in m_model)
            {
                Profiles.Add(new ReleaseCheckoutProfileRecordViewModel()
                {
                    Model = profile
                });
            }
        }

        private void NotifyCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (ReleaseCheckoutProfile profile in args.NewItems)
                {
                    Profiles.Add(new ReleaseCheckoutProfileRecordViewModel()
                    {
                        Model = profile
                    });
                }
            }
            else if (args.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (ReleaseCheckoutProfile profile in args.OldItems)
                {
                    Profiles.Remove(Profiles.First(p => p.Model == profile));
                }
            }
            else if (args.Action == NotifyCollectionChangedAction.Replace)
            {
                Profiles.First(p => p.Model == args.OldItems[0]).Model = args.NewItems[0] as ReleaseCheckoutProfile;
            }
        }

        private int GetCurentTasksCount()
        {
            int res = 0;

            foreach (ReleaseCheckoutProfile profile in m_model)
            {
                res += profile.TasksCount;
            }

            return res;
        }

        #endregion

        #region Fields

        private ObservableCollection<ReleaseCheckoutProfile> m_model = null;
        private ObservableCollection<CheckoutProfile> m_checkoutProfiles = null;

        #endregion
    }
}
