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
       
        public String LastError { get; set; }

        private Account _account;
        public Account Account
        {
            get
            {
                return _account;
            }
            private set
            {
                if (value == _account)
                    return;

                _account = value;
            }
        }

        public bool AccountLoaded { get; private set; }

        private Session()
        {
            Client = new HttpClient();
        }

        public async Task<Account> LoadAccount()
        {
            if (AccountLoaded) return Account;
            _account = await Account.Load();
            AccountLoaded = true;
            return Account;
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

            BankenLiveSettings.Instance.Token = (string)token["token"];

            return true;
        }

        public async Task<Boolean> Login()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, CommonData.BaseUri + "/session");

            request.Headers.Add("X-SB1-Rest-Version", "1.0.0");
            request.Headers.Authorization = new AuthenticationHeaderValue("SB1-TOKEN");

            request.Content = new StringContent(
                JsonConvert.SerializeObject(new { token = BankenLiveSettings.Instance.Token }),
                Encoding.UTF8, "application/json");

            var response = await Client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                LastError = "Kunne ikke logge inn med token, prøv å logge inn på nytt";
                return false;
            }

            var session = JsonConvert.DeserializeObject<JObject>(await response.Content.ReadAsStringAsync());

            BankenLiveSettings.Instance.AccountId = (string)session["user"]["defaultBalanceAccountNumber"];

            return true;
        }

        public async Task<AccountSnapshot> GetInfo()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, CommonData.BaseUri + "/accounts/" + BankenLiveSettings.Instance.AccountId);

            request.Headers.Add("X-SB1-Rest-Version", "1.0.0");

            var response = await Client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                LastError = "Kunne ikke hente kontoinformasjon";
                return null;
            }

            var accountJson = await response.Content.ReadAsStringAsync();

            var snapshot = JsonConvert.DeserializeObject<AccountSnapshot>(accountJson);

            if (Account == null)
            {
                Account = new Account(snapshot.Id, snapshot.Name, new List<AccountSnapshot>());
            }

            Account.AddSnapshot(snapshot);
            await Account.Save();

            return snapshot;
        }

        public async Task<IEnumerable<Transaction>> GetTransactions()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, CommonData.BaseUri + "/accounts/" + BankenLiveSettings.Instance.AccountId + "/transactions");

            request.Headers.Add("X-SB1-Rest-Version", "1.0.0");

            var response = await Client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                LastError = "Kunne ikke hente transaksjoner";
                return null;
            }

            var transactionsJson = await response.Content.ReadAsStringAsync();

            var transactionsWrapper = JsonConvert.DeserializeObject<TransactionsWrapper>(transactionsJson);

            return transactionsWrapper.Transactions;
        }
    }
}
