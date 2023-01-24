using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace WebApplication3.Models
{
    public class GridFSServiceAndDataBaseService
    {
        private readonly IMongoCollection<BsonDocument> table;
        private readonly GridFSBucket bucket;
        public GridFSServiceAndDataBaseService(IOptions<UsersDatabaseSetting> options)
        {
            var client = new MongoClient(options.Value.ConnectionString);
            var database = client.GetDatabase(options.Value.DatabaseName);
            /*Console.WriteLine(options.Value.DatabaseName);
            Console.WriteLine(options.Value.UsersCollectionName);*/
            table = database.GetCollection<BsonDocument>(options.Value.UsersCollectionName);
            var cond = new GridFSBucketOptions
            {
                BucketName = "Image",
                ChunkSizeBytes = 255 //255 KB is the default value
            };
            bucket = new(database, cond);
        }
        public async Task Create(BsonDocument userUploadFile)
        {
            await table.InsertOneAsync(userUploadFile);
        }
        public async Task<List<BsonDocument>> Get(string ip)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("IP",ip);
            return await table.Find(filter).ToListAsync();
        }
        public async Task<string> UploadAsync(IFormFile file)
        {
            var type = file.ContentType.ToString();
            var fileName = file.FileName;

            var options = new GridFSUploadOptions
            {
                Metadata = new BsonDocument { { "FileName", fileName }, { "Type", type } }
            };

            using var stream = await bucket.OpenUploadStreamAsync(fileName, options); // Open the output stream
            var id = stream.Id; // Unique Id of the file
            //Console.WriteLine(id);
            file.CopyTo(stream); // Copy the contents to the stream
            await stream.CloseAsync();
            return id.ToString();
        }
        //public async 
        public async Task<byte[]> GetFileByIdAsync(ObjectId Id)
        {
            return await bucket.DownloadAsBytesAsync(Id);
            //return await bucket.DownloadAsBytesAsync();

        }

    }
}
