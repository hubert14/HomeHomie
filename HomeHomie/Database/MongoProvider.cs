using HomeHomie.Database.Entities;
using MongoDB.Driver;

namespace HomeHomie.Database
{
    static class MongoProvider
    {
        static string connectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING");
        static string dbName = Environment.GetEnvironmentVariable("MONGO_DATABASE");

        public static async Task<ElectricityGraphic?> GetDataFromMongoAsync(string? date = null)
        {
            var collection = GetCollection();
            var dateToCheck = date ?? DateTime.Now.ToString("dd.MM.yyyy");
            return await collection.Find(document => document.Date == dateToCheck).FirstOrDefaultAsync();
        }

        public static async Task UpdateDataInMongoAsync(ElectricityGraphic graphic)
        {
            var collection = GetCollection();
            await collection.ReplaceOneAsync(document => document.Date == graphic.Date, graphic);
            Console.WriteLine("Updated graphic for date:" + graphic.Date);
        }

        public static async Task AddDataInMongoAsync(ElectricityGraphic graphic)
        {
            var collection = GetCollection();
            await collection.InsertOneAsync(graphic);
            Console.WriteLine("Inserted graphic for date:" + graphic.Date);
        }

        private static IMongoCollection<ElectricityGraphic> GetCollection() => new MongoClient(connectionString).GetDatabase(dbName).GetCollection<ElectricityGraphic>(ElectricityGraphic.CollectionName);
    }
}
