using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace BankenLive
{
    public class BankenLiveSettings
    {
        static BankenLiveSettings _instance;
        public static BankenLiveSettings Instance
        {
            get
            {
                if (_instance == null)
                    _instance = LoadSettings();

                return _instance;
            }
        }

        private string _token;
        public string Token
        {
            get { return _token; }
            set
            {
                if (value == _token)
                    return;

                _token = value;

                SaveSettings();
            }
        }

        private string _accountId;
        public string AccountId
        {
            get { return _accountId; }
            set
            {
                if (value == _accountId)
                    return;

                _accountId = value;

                SaveSettings();
            }
        }

        public bool HasToken { get { return !String.IsNullOrEmpty(Token); } }

        private bool _toastOnIncreasedBalance { get; set; }
        public bool ToastOnIncreasedBalance
        {
            get { return _toastOnIncreasedBalance; }
            set
            {
                _toastOnIncreasedBalance = value;
                SaveSettings();
            }
        }

        public double _toastOnIncreasedBalanceThreshold { get; set; }
        public double ToastOnIncreasedBalanceThreshold
        {
            get { return _toastOnIncreasedBalanceThreshold; }
            set
            {
                _toastOnIncreasedBalanceThreshold = value;
                SaveSettings();
            }
        }

        private BankenLiveSettings()
        {

        }

        private static BankenLiveSettings LoadSettings()
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            var settings = new BankenLiveSettings();

            settings._token = localSettings.Values["Token"] as string;
            settings._accountId = localSettings.Values["AccountId"] as string;

            if (HasToastConfig(localSettings))
            {
                settings._toastOnIncreasedBalance = (bool)localSettings.Values["ToastOnIncreasedBalance"];
                settings._toastOnIncreasedBalanceThreshold = (double)localSettings.Values["ToastOnIncreasedBalanceThreshold"];
            }
            else
            {
                settings._toastOnIncreasedBalance = false;
                settings._toastOnIncreasedBalanceThreshold = 100.0;
                settings.SaveSettings();
            }

            return settings;
        }

        private static bool HasToastConfig(ApplicationDataContainer localSettings)
        {
            return localSettings.Values.ContainsKey("ToastOnIncreasedBalance") && localSettings.Values.ContainsKey("ToastOnIncreasedBalanceThreshold");
        }

        private void SaveSettings()
        {
            var localSettings = ApplicationData.Current.LocalSettings;

            localSettings.Values["Token"] = Token;
            localSettings.Values["AccountId"] = AccountId;
            localSettings.Values["ToastOnIncreasedBalance"] = ToastOnIncreasedBalance;
            localSettings.Values["ToastOnIncreasedBalanceThreshold"] = ToastOnIncreasedBalanceThreshold;
        }


    }
}
