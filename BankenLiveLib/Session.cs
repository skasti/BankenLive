using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System.UserProfile;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BankenLive
{
    public class Session
    {
        private static Session _current;
        public static Session Current
        {
            get
            {
                if (_current == null)
                    _current = new Session();

                return _current;
            }
        }

        private HttpClient Client { get; set; }
        private String Token { get; set; }
        private String AccountId { get; set; }

        public Boolean HasToken { get { return !String.IsNullOrEmpty(Token); } }
        public String LastError { get; set; }


        private Account _previousAccount = null;
        public Account PreviousAccount
        {
            get
            {
                if (_previousAccount == null)
                {
                    var localSettings = ApplicationData.Current.LocalSettings;
                    var json = localSettings.Values["PreviousAccount"] as string;

                    if (!String.IsNullOrEmpty(json))
                        _previousAccount = JsonConvert.DeserializeObject<Account>(json);
                }

                return _previousAccount;
            }
            private set
            {
                var localSettings = ApplicationData.Current.LocalSettings;
                localSettings.Values["PreviousAccount"] = JsonConvert.SerializeObject(value);
                
                _previousAccount = value;
            }
        }

        private Account _currentAccount = null;
        public Account CurrentAccount
        {
            get
            {
                if (_currentAccount == null)
                {
                    var localSettings = ApplicationData.Current.LocalSettings;
                    var json = localSettings.Values["CurrentAccount"] as string;

                    if (!String.IsNullOrEmpty(json))
                        _currentAccount = JsonConvert.DeserializeObject<Account>(json);
                }

                return _currentAccount;
            }
            private set
            {
                var localSettings = ApplicationData.Current.LocalSettings;
                localSettings.Values["CurrentAccount"] = JsonConvert.SerializeObject(value);

                _currentAccount = value;
            }
        }

        private Session()
        {
            Client = new HttpClient();

            LoadSettings();
        }

        private void LoadSettings()
        {
            var localSettings = ApplicationData.Current.LocalSettings;

            Token = localSettings.Values["Token"] as string;
            AccountId = localSettings.Values["AccountId"] as string;
        }

        private void SaveSettings()
        {
            var localSettings = ApplicationData.Current.LocalSettings;

            localSettings.Values["Token"] = Token;
            localSettings.Values["AccountId"] = AccountId;
        }

        public async Task<Boolean> Level2Login(string bank, string ssn, string password, string oneTimePassword)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, CommonData.BaseUri + "/session");

            request.Headers.Add("X-SB1-Rest-Version","1.0.0");
            request.Headers.Authorization = new AuthenticationHeaderValue("SB1-TODOS");
            
            request.Content = new StringContent(
                JsonConvert.SerializeObject(new { ssn, bank, password, oneTimePassword }), 
                Encoding.UTF8, "application/json");

            var response = await Client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                LastError = "Nivå2 innlogging mislyktes";
                return false;
            }

            var csrfToken = response.Headers.GetValues("X-CSRFToken").First();

            return await CreateToken(csrfToken);
        }

        private async Task<Boolean> CreateToken(string csrfToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, CommonData.BaseUri + "/session/token");

            request.Headers.Add("X-SB1-Rest-Version", "1.0.0");
            request.Headers.Add("X-CSRFToken", csrfToken);

            request.Content = new StringContent(
                JsonConvert.SerializeObject(new { description = "klokkebank" }), 
                Encoding.UTF8, "application/json");

            var response = await Client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                LastError = "Kunne ikke opprette token";
                return false;
            }

            var token = JsonConvert.DeserializeObject<JObject>(await response.Content.ReadAsStringAsync());

            Token = (string)token["token"];

            SaveSettings();

            return true;
        }

        public async Task<Boolean> Login()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, CommonData.BaseUri + "/session");

            request.Headers.Add("X-SB1-Rest-Version", "1.0.0");
            request.Headers.Authorization = new AuthenticationHeaderValue("SB1-TOKEN");

            request.Content = new StringContent(
                JsonConvert.SerializeObject(new { token = Token }),
                Encoding.UTF8, "application/json");

            var response = await Client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                LastError = "Kunne ikke logge inn med token, prøv å logge inn på nytt";
                return false;
            }

            var session = JsonConvert.DeserializeObject<JObject>(await response.Content.ReadAsStringAsync());

            AccountId = (string)session["user"]["defaultBalanceAccountNumber"];

            SaveSettings();

            return true;
        }

        public async Task<Account> GetInfo()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, CommonData.BaseUri + "/accounts/" + AccountId);

            request.Headers.Add("X-SB1-Rest-Version", "1.0.0");

            var response = await Client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                LastError = "Kunne ikke hente kontoinformasjon";
                return null;
            }

            var account = JsonConvert.DeserializeObject<Account>(await response.Content.ReadAsStringAsync());

            if (CurrentAccount != null)
                PreviousAccount = CurrentAccount;

            CurrentAccount = account;

            return account;
        }
    }

    public class Account
    {
        public String Id { get; set; }
        public String Name { get; set; }
        public String FormattedNumber { get; set; }
        public int DisposableAmountInteger { get; set; }
        public int DisposableAmountFraction { get; set; }

        public override string ToString()
        {
            return String.Format("{0}: {1},{2}", Name, DisposableAmountInteger,
                    DisposableAmountFraction > 0 ? DisposableAmountFraction.ToString() : "-");
        }
    }
}
