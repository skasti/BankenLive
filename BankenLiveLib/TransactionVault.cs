using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace BankenLive
{
    public class TransactionVault
    {
        private static TransactionVault _instance;
        public static async Task<TransactionVault> GetInstance()
        {
            if (_instance == null)
                _instance = await Load() ?? new TransactionVault(new List<Transaction>());

            return _instance;
        }

        private ObservableCollection<Transaction> _transactions = new ObservableCollection<Transaction>();
        public ObservableCollection<Transaction> Transactions
        {
            get { return _transactions; }
        }

        public TransactionVault(IEnumerable<Transaction> transactions)
        {
            _transactions = new ObservableCollection<Transaction>(transactions);
        }

        public IEnumerable<Transaction> AddRange(IEnumerable<Transaction> transactions)
        {
            var newTransactions = transactions.Where(t => !_transactions.Contains(t));

            foreach (var transaction in newTransactions)
                _transactions.Add(transaction);

            _transactions.OrderByDescending(t => t.Date);

            return newTransactions;
        }

        public void Reset()
        {
            _transactions.Clear();
        }

        public string createCsv()
        {
            var csvLines = _transactions.Select(t => String.Format("{0};{1};{2};{3}", t.Type, t.Description, t.Amount, t.Incoming));
            return String.Join("\n", csvLines);
        }

        public async void Save()
        {
            var filename = "Vault.json";
            // Get the local folder.
            var local = ApplicationData.Current.LocalFolder;

            // Create a new folder name DataFolder.
            var dataFolder = await local.CreateFolderAsync("Data",
                CreationCollisionOption.OpenIfExists);

            // Create a new file named DataFile.txt.
            var file = await dataFolder.CreateFileAsync(filename,
            CreationCollisionOption.ReplaceExisting);

            await FileIO.WriteTextAsync(file, JsonConvert.SerializeObject(this));
        }

        private static async Task<TransactionVault> Load()
        {
            var filename = "Vault.json";
            // Get the local folder.
            var local = ApplicationData.Current.LocalFolder;

            // Create a new folder name DataFolder.
            var dataFolder = await local.CreateFolderAsync("Data",
                CreationCollisionOption.OpenIfExists);

            try
            {
                // Create a new file named DataFile.txt.
                var file = await dataFolder.GetFileAsync(filename);

                var json = await FileIO.ReadTextAsync(file);

                return JsonConvert.DeserializeObject<TransactionVault>(json);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
