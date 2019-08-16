using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Models
{
    public enum CaptchaType
    {
        ReCaptchaV2,
        Undefined
    }

    public class CaptchaDescription
    {
        #region Properties

        public CaptchaType Code { get; set; }
        public object DefaultSettings { get; set; }

        #endregion
    }

    public class RecaptchaSettings
    {
        #region Properties

        public string SiteKey { get; set; }
        public string SitePath { get; set; }

        #endregion
    }

    public static class CaptchaHarvestersCollection
    {

        #region Properties

        public static Dictionary<CaptchaType, Type> Harvesters { get; } = new Dictionary<CaptchaType, Type>()
        {
            { CaptchaType.ReCaptchaV2, typeof(ReCaptchaHarvester) }
        };

        #endregion

    }
}
