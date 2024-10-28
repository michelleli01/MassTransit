using Azure.Data.Tables;

namespace MassTransit.Azure.Table.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using TestFramework;


    public class AzureTableInMemoryTestFixture :
        InMemoryTestFixture
    {
        protected readonly string ConnectionString;
        protected readonly TableClient TestCloudTable;
        protected readonly string TestTableName;

        public AzureTableInMemoryTestFixture()
        {
            ConnectionString = Configuration.StorageAccount;
            TestTableName = "azuretabletests";
            var tableServiceClient = new TableServiceClient(ConnectionString);
            var tableClient = tableServiceClient.GetTableClient(TestTableName);
            TestCloudTable = tableClient;
        }

        protected override void ConfigureInMemoryBus(IInMemoryBusFactoryConfigurator configurator)
        {
            configurator.UseDelayedMessageScheduler();

            base.ConfigureInMemoryBus(configurator);
        }

        public IEnumerable<T> GetRecords<T>() where T : class, ITableEntity, new()
        {
            return TestCloudTable.Query<T>().ToList();
        }

        public IEnumerable<TableEntity> GetTableEntities()
        {
            return TestCloudTable.Query<TableEntity>();
        }

        [OneTimeSetUp]
        public async Task Bring_it_up()
        {
            await TestCloudTable.CreateIfNotExistsAsync();

            IEnumerable<TableEntity> entities = GetTableEntities();
            var groupedEntities = entities.GroupBy(e => e.PartitionKey);
            foreach (var group in groupedEntities)
            {
                var batchOperations = group
                    .Select(entity => new TableTransactionAction(TableTransactionActionType.Delete, entity))
                    .ToList();

                // Execute the batch operation
                await TestCloudTable.SubmitTransactionAsync(batchOperations);
            }
        }
    }
}
