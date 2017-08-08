using Microsoft.WindowsAzure.Storage.Table;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace TfsBot.Common.Entities
{
    public class Client: TableEntity
    {
        public Client(string serverId, string userId, string userName) 
            : base(GetPartitionKey(userId), GetRowKey(userId, userName))
        {
            ServerId = serverId;
        }

        public Client()
        {
        }

        public static string GetPartitionKey(string userId)
        {
            return userId.Substring(0, 1);
        }

        public static string GetRowKey(string userId, string userName)
        {
            return $"{userId}:{userName}";
        }

        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonIgnoreIfDefault]
        public string Id { get; set; }
        public string ServerId { get; set; }
    }
}