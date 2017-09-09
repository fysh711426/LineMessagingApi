﻿using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Threading.Tasks;

namespace FunctionAppSample
{
    class LineBotEntry : TableEntity
    {
        public string Location { get; set; }
        public LineBotEntry()
        { }

        public LineBotEntry(string entryType, string entryId)
        {
            PartitionKey = entryType;
            RowKey = entryId;
        }
    }

    class LineBotTableStorage
    {
        private CloudTableClient _tableClient;
        private CloudTable _entries;

        protected LineBotTableStorage(string connectionString)
        {
            var strageAccount = CloudStorageAccount.Parse(connectionString);
            _tableClient = strageAccount.CreateCloudTableClient();
        }

        protected async Task InitializeAsync()
        {
            _entries = _tableClient.GetTableReference("Entries");
            await _entries.CreateIfNotExistsAsync();
        }

        public static async Task<LineBotTableStorage> CreateAsync(string connectionString)
        {
            var result = new LineBotTableStorage(connectionString);
            await result.InitializeAsync();
            return result;
        }

        public async Task AddEntryAsync(string entryType, string entryId)
        {
            if ((await FindEntryAsync(entryType, entryId)) != null) { return; }

            var newItem = new LineBotEntry(entryType, entryId);
            await _entries.ExecuteAsync(TableOperation.Insert(newItem));
        }

        public async Task UpdateEntryAsync(LineBotEntry entry)
        {
            await _entries.ExecuteAsync(TableOperation.InsertOrReplace(entry));
        }

        public async Task<LineBotEntry> FindEntryAsync(string entryType, string entryId)
        {
            var ope = TableOperation.Retrieve<LineBotEntry>(entryType, entryId);
            var retrieveResult = await _entries.ExecuteAsync(ope);
            if (retrieveResult.Result == null) { return null; }
            return (LineBotEntry)(retrieveResult.Result);
        }

        public async Task DeleteEntryAsync(string entryType, string entryId)
        {
            var entry = await FindEntryAsync(entryType, entryId);
            if (entry == null) { return; }

            var ope = TableOperation.Delete(entry);
            await _entries.ExecuteAsync(ope);
        }
    }
}
