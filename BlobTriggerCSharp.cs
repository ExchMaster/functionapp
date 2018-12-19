using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using System.Security.Authentication;
using MongoDB.Bson.Serialization.Serializers;

namespace Company.Function
{
    public static class BlobTriggerCSharp
    {
        [FunctionName("BlobTriggerCSharp")]
        public static void Run([BlobTrigger("samples-workitems/{name}", Connection = "AzureWebJobsStorage")]Stream myBlob, string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
            string strFull = null;


            if (name.Contains(".json"))
            {
                StreamReader myString = new StreamReader(myBlob);
                var connectionString = Environment.GetEnvironmentVariable("CosmosDB_Mongo_ConnString");

                MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(connectionString));
                settings.SslSettings = new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };
                var mongoClient = new MongoClient(settings);
                var database = mongoClient.GetDatabase("BookstoreDb");
                var collection = database.GetCollection<BsonDocument>("Books");

                string s = null;
                string blobBSONType = null;
                while ((s = myString.ReadLine()) != null)
                {
                    strFull += s;
                }

                var jsonReader = new JsonReader(strFull);
                blobBSONType = jsonReader.GetCurrentBsonType().ToString();
                if (jsonReader.CurrentBsonType.ToString() == "Array")
                {
                    var serializer = new BsonArraySerializer();
                    var bsonDocArray = serializer.Deserialize(BsonDeserializationContext.CreateRoot(jsonReader));
                    foreach (var bdoc in bsonDocArray)
                    {
                        collection.InsertOne(bdoc.ToBsonDocument());
                    }
                }
                
                if (jsonReader.CurrentBsonType.ToString() == "Document")
                {
                    var serializer = new BsonDocumentSerializer();
                    var bsonDoc = serializer.Deserialize(BsonDeserializationContext.CreateRoot(jsonReader));
                    collection.InsertOne(bsonDoc);
                }
                var count = collection.CountDocuments(new BsonDocument());
                Console.WriteLine(count);
            }
        }
    }
}
