using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Win32;

namespace Bot.ViewModels
{
    public class DotTrickGeneratorViewModel: ViewModelBase
    {
        #region Constructors

        public DotTrickGeneratorViewModel()
        {
            Generate = new DelegateCommand(parameter =>
                {
                    GenerateEmails();
                },
            parameter => Email != null);

            Clear = new DelegateCommand(parameter =>
            {
                GeneratedEmails.Clear();
            },
            parameter => GeneratedEmails.Count > 0);

            Export = new DelegateCommand(parameter =>
            {
                SaveFileDialog dialog = new SaveFileDialog()
                {
                    DefaultExt = ".csv",
                    Filter = "CSV files (.csv)|*.csv"
                };

                if (dialog.ShowDialog() ?? false)
                {
                    using (StreamWriter writer = new StreamWriter(dialog.FileName))
                    {
                        foreach (string email in GeneratedEmails)
                        {
                            writer.WriteLine(email);
                        }

                        writer.Close();
                    }
                }
            },
            parameter => GeneratedEmails.Count > 0);
        }

        #endregion

        #region Properties

        public string Email
        {
            get { return m_email; }

            set { SetProperty(ref m_email, value); }
        }

        public int Permutations
        {
            get { return m_permutations; }

            set { SetProperty(ref m_permutations, value); }
        }

        public ObservableCollection<string> GeneratedEmails { get; } = new ObservableCollection<string>();

        public static ObservableCollection<int> PermutationsCollection
        {
            get
            {
                if (m_permutationsCollection == null)
                {
                    m_permutationsCollection = new ObservableCollection<int>();

                    for (int i = 0; i < 256; i++)
                    {
                        m_permutationsCollection.Add(i);
                    }
                }

                return m_permutationsCollection;
            }
        }

        #endregion

        #region Commands

        public ICommand Generate { get; protected set; }
        public ICommand Clear { get; protected set; }
        public ICommand Export { get; protected set; }

        #endregion

        #region Methods

        protected override void PropertyChangedHandler(object sender, PropertyChangedEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void GenerateEmails()
        {
            string[] arr = Email.Split(new char[] {'@'});
            string name = arr[0];
            string server = arr[1];
            int currentPosition = 1;
            int lastPosition = name.Length - 1;
            int generatedCount = 0;

            while (true)
            {
                int firstPosition = -1;
                for (int i = currentPosition; i <= lastPosition; i++)
                {
                    if (name[i - 1] != '.' && name[i] != '.')
                    {
                        if (firstPosition == -1)
                        {
                            firstPosition = i;
                        }

                        string email = $"{name.Insert(i, ".")}@{server}";
                        if (!GeneratedEmails.Contains(email) && (Permutations == 0 || generatedCount < Permutations))
                        {
                            GeneratedEmails.Add(email);
                            generatedCount++;
                        }
                    }
                }

                if (firstPosition != -1)
                {
                    name = name.Insert(firstPosition, ".");
                    currentPosition = firstPosition + 1;
                    lastPosition++;
                }
                else
                {
                    break;
                }
            }


        }

        #endregion

        #region Fields

        private string m_email = null;
        private int m_permutations = PermutationsCollection.First();
        private static ObservableCollection<int> m_permutationsCollection = null;

        #endregion
    }
}
