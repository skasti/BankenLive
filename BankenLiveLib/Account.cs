using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace BankenLive
{
    public class Account
    {
        public string Id { get; private set; }
        public string Name { get; set; }

        private List<AccountSnapshot> _snapshots = new List<AccountSnapshot>();
        public List<AccountSnapshot> Snapshots
        {
            get
            {
                return _snapshots.ToList();
            }
        }

        public AccountSnapshot PreviousSnapshot { get; private set; }
        private AccountSnapshot _latestSnapshot;
        public AccountSnapshot LatestSnapshot
        {
            get { return _latestSnapshot; }
            private set
            {
                if (_latestSnapshot != null)
                    PreviousSnapshot = _latestSnapshot;

                _latestSnapshot = value;
            }
        }

        public double Balance
        {
            get
            {
                return LatestSnapshot ?? 0.0;
            }
        }

        public double BalanceChange
        {
            get
            {
                return PreviousSnapshot != null ? LatestSnapshot.Balance - PreviousSnapshot.Balance : 0.0;
            }
        }

        public Account(string id, string name, List<AccountSnapshot> snapshots, AccountSnapshot latestSnapshot = null, AccountSnapshot previousSnapshot = null)
        {
            Id = id;
            Name = name;
            _snapshots = snapshots;
            _latestSnapshot = latestSnapshot;
            PreviousSnapshot = previousSnapshot;
        }

        public void AddSnapshot(AccountSnapshot snapshot)
        {
            _snapshots.Add(snapshot);
            LatestSnapshot = snapshot;
        }

        public void ClearSnapshotList()
        {
            _snapshots.Clear();
        }

        public override string ToString()
        {
            return string.Format("{0}: {1:C}", Name, Balance);
        }

        public async Task<Account> Save()
        {
            var filename = "Account.json";
            // Get the local folder.
            var local = ApplicationData.Current.LocalFolder;

            // Create a new folder name DataFolder.
            var dataFolder = await local.CreateFolderAsync("Data",
                CreationCollisionOption.OpenIfExists);

            // Create a new file named DataFile.txt.
            var file = await dataFolder.CreateFileAsync(filename,
            CreationCollisionOption.ReplaceExisting);

            await FileIO.WriteTextAsync(file, JsonConvert.SerializeObject(this));

            return this;
        }

        public static async Task<Account> Load()
        {
            var filename = "Account.json";
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

                return JsonConvert.DeserializeObject<Account>(json);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
