using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using TfsBot.Common.Entities;

namespace TfsBot.Common.Db
{
    public class MongoRepository : IRepository
    {
        private readonly IMongoCollection<ServerClient> _serviceClientsCollection;
        private readonly IMongoCollection<Client> _clientsCollection;


        public MongoRepository(string connectionString, string dbName = null)
        {
            var client = new MongoClient(connectionString);
            var db = client.GetDatabase(dbName ?? "tfs_bot");

            const string serviceClientsCollectionName = "serviceclients";
            if (!CollectionExists(db, serviceClientsCollectionName))
                db.CreateCollection(serviceClientsCollectionName);
            _serviceClientsCollection = db.GetCollection<ServerClient>(serviceClientsCollectionName);
            
            const string clientsCollectionName = "clients";
            if (!CollectionExists(db, clientsCollectionName))
                db.CreateCollection(clientsCollectionName);
            _clientsCollection = db.GetCollection<Client>(clientsCollectionName);
        }

        public async Task SaveServiceClient(ServerClient serverClient)
        {
            var filterPartitionKey = Builders<ServerClient>.Filter.Eq(c => c.PartitionKey, serverClient.PartitionKey);
            var filterRowKey = Builders<ServerClient>.Filter.Eq(c => c.RowKey, serverClient.RowKey);
            var filter = Builders<ServerClient>.Filter.And(filterPartitionKey, filterRowKey);
            
            await _serviceClientsCollection.ReplaceOneAsync(
                filter,
                serverClient,
                new UpdateOptions { IsUpsert = true });
        }

        public List<ServerClient> GetServerClients(string serverId)
        {
            var filterServiceId = Builders<ServerClient>.Filter.Eq(c => c.ServiceId, serverId);

            var result =
                _serviceClientsCollection.FindSync(filterServiceId).ToList();
            return result;
        }

        public async Task SaveClient(Client client)
        {
            var filterPartitionKey = Builders<Client>.Filter.Eq(c => c.PartitionKey, client.PartitionKey);
            var filterRowKey = Builders<Client>.Filter.Eq(c => c.RowKey, client.RowKey);
            var filter = Builders<Client>.Filter.And(filterPartitionKey, filterRowKey);

            await _clientsCollection.ReplaceOneAsync(
                filter,
                client,
                new UpdateOptions { IsUpsert = true });
        }

        public async Task<Client> GetClientAsync(string userId, string userName)
        {
            var partitionKey = Client.GetPartitionKey(userId);
            var rowKey = Client.GetRowKey(userId, userName);

            var filterPartitionKey = Builders<Client>.Filter.Eq(c => c.PartitionKey, partitionKey);
            var filterRowKey = Builders<Client>.Filter.Eq(c => c.RowKey, rowKey);
            var filter = Builders<Client>.Filter.And(filterPartitionKey, filterRowKey);

            return await _clientsCollection
                .FindAsync(
                    filter, 
                    new FindOptions<Client>()
                    {
                        Limit = 1
                    })
                    .Result.FirstOrDefaultAsync();
        }

        public async Task RemoveServerClientAsync(ServerClient client)
        {
            var filterPartitionKey = Builders<ServerClient>.Filter.Eq(c => c.ServiceId, client.ServiceId);
            var filterRowKey = Builders<ServerClient>.Filter.Eq(c => c.UserId, client.UserId);
            var filter = Builders<ServerClient>.Filter.And(filterPartitionKey, filterRowKey);

            await _serviceClientsCollection.DeleteOneAsync(filter);
        }


        private bool CollectionExists(IMongoDatabase db, string collectionName)
        {
            var filter = new BsonDocument("name", collectionName);
            //filter by collection name
            var collections = db.ListCollections(new ListCollectionsOptions { Filter = filter });
            //check for existence
            return collections.Any();
        }
    }
}