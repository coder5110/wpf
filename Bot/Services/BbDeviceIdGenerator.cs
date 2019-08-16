using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Services
{
    public static class BbDeviceIdGenerator
    {
        #region Methods

        public static string Generate()
        {
            string res = null;

            string str = RandomString(m_length - m_base.Length - 1);
            res = m_base + str + "=";

            return res;
        }

        
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz+-*/";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[m_random.Next(s.Length)]).ToArray());
        }

        #endregion

        #region Fields

        private static string m_base = "0400mTR4Z7+BQcwNf94lis1ztjbaCjtwdIzBnrp/XmcfWoVVgr+Rt2dAZIo7BJIRIWDNf0dFpDbITSTXnfra5uHnD42fRE9PMD+Q3FCX9pS6pa9J27cNnYPSQUyAZ5slTMaJhGc3lDN94AYxBKqzrSkuy0N13n2vmAeykQgdlVeDidwjyx6XR+7WMQo5VNMvaDXfrI3MaSYrgzuOZTYvnKL0JQQ08ROukpTpxifyrThRovESnjwNVGXSgGQ8InPsuf6/kpMgG84gzO5PMQF00uJew9XqxzJ4y+q4D7BvmNEHDAgoB8oZZ0btVCezVvvuF2qVljAlWwXHcT+hgN6tJA4p1nA+k9XRlIy/50xtECsGqYm6aCMfbLbPTy4uvLu1aZomcGdkEVLhDg3+QXmEbxmvGLC8A46ylolhxA9x3qRKbjOwV9GLh0FP4Hyv4hM/AnmN6EuQOlw0fYZ9/L+MCT1tQv6H4rWKKGSWv9eOvsyvTBVVGK66EXEuaD0YLXuOWeFzfdEHluIab5zvUSbJ+Ltz3R3+1lLYqHZEj+IcoGnbUH7waDNJScJcz+9geVfa3Q4gKULPxBs5mS8Xqi+HJDD4AWXDdE2CEbJsOfSle6y1MKoEMUscz7kbcnG9B+SIx5AnCkrOClWCgeJDTj/tf6S8ijexjDDuBwYFRw6NL2GAwt9ss5KMs0PsoG2s0fO33l4+9bLxQk+jr+3NB7f9PDxiL9ebRcG2+TZFbazR87feXj4jmR3L2eiAEnMnEYUvBMAKTEVdGJlxBwCc8acbcKqAuWf7gouzBPJaEMCy0s3hRLlX3uHnT/mMq6Sxn6AVxzxp/ED2FVz6zK96DgNxszbLkaDl1FsZY8jwFaXKqt5H5j+zoEkNBc35XGtVsYmdWtM91YYITyzRwv8PXXpOa/Fh/Q7T6v45cktLSp+CCNJ+Oiiusa2jjh0Vn3enZWPFEyBIiSkiHcDvTuWf/Cqw+IYFx+FybVq+UV9nSDycnts0swvqLgGIeGqjIilb4tK0h/8u5xVfFj40LHiGT7PU7po4i6NCI+25jHS1f5IgLE4vrHWx2a9qirpXpL5B/YfYEkP1K3mjpPnycynnVb7uHgXQGX7ojYa45C7OGMW1OJGB+ysUG6605icneA86Jms/ZKI5bPJst9a2pmVJJjzrJUU/BlEOdKf8J5Q2RUWqyLgukUlzhn8sAAPURYzyVekwhI5/tPvXfQEg+KLRVsDNUznM+4IlRQO86SD0OIS7LH591kg7Q92FGKueuxRQDekNcfNFHQnRncqsMG12Hj2lPPDtzNhejcFMJfaGGPF5Yq8RTSueun9eZx9ahVWCv5G3Z0BkSYe0W7SAdaEPm6W/nvCegCF+8VUjaVDzIG2/CXX4tnqvW18iZWhKAZVPfVKB9sZJjw85/2I16rbjPQA7SJ+APDnQbDT4GnU5z1+B+B/GF2TlAhy3sJeNlM6Tn5qdCRSa5ml7UpdlJpo2r+smR3B9yYc+acKU0y9xlgtfKpX9/nuMAAsUNaqqo4LExXlvyVFLb3rPMuETX9hlGL/SvMcvROqtBy+L6BpT4vA8ObKJ6/";
        private static int m_length = 1680;
        private static Random m_random = new Random();

        #endregion
    }
}
