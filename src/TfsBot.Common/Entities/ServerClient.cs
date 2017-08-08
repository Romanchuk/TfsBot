using Microsoft.WindowsAzure.Storage.Table;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace TfsBot.Common.Entities
{
    public class ServerClient : TableEntity
    {
        public ServerClient(string serverId, string userId) : base(serverId, userId)
        {
        }

        public ServerClient()
        {
        }

        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonIgnoreIfDefault]
        public string Id { get; set; }

        public string ServiceId => PartitionKey;

        public string UserId => RowKey;
        public string UserName { get; set; }

        public string BotServiceUrl { get; set; }        
        public string BotId { get; set; }
        public string BotName { get; set; }
        public string ReplaceFrom { get; set; }
        public string ReplaceTo { get; set; }
    }
}
