using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bot.Models;

namespace Bot.ViewModels
{
    public class PayCardEditViewModel: ViewModelBase
    {
        #region Constructors



        #endregion

        #region Properties

        public PayCard Model
        {
            get { return m_model; }

            set
            {
                if (m_model != null)
                {
                    DeattachModel(m_model);
                }

                if (SetProperty(ref m_model, value))
                {

                }

                if (m_model != null)
                {
                    AttachModel(m_model);
                }
            }
        }

        public string Number
        {
            get { return m_model.Number; }

            set { m_model.Number = value; }
        }

        public string Holder
        {
            get { return m_model.Holder; }

            set { m_model.Holder = value; }
        }

        public DateTime? ExpirationDate
        {
            get { return m_model.ExpirationDate; }

            set { m_model.ExpirationDate = value; }
        }

        public string CVS
        {
            get { return m_model.CVS; }

            set { m_model.CVS = value; }
        }

        public PayCardType Type
        {
            get { return m_model.Type; }

            set { m_model.Type = value; }
        }

        #endregion

        #region MyRegion

        protected override void PropertyChangedHandler(object sender, PropertyChangedEventArgs args)
        {
            
        }

        #endregion

        #region Fields

        private PayCard m_model = null;

        #endregion
    }
}
