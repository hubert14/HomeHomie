using HomeHomie.Core;
using HomeHomie.Core.Providers;
using MongoDB.Driver;
using System.Text.RegularExpressions;

namespace HomeHomie.DatabaseModule
{
    internal class MongoProvider : IDatabaseProvider
    {
        private IMongoDatabase _db;

        public MongoProvider(IDatabaseSettings settings)
        {
            _db = new MongoClient(settings.ConnectionString).GetDatabase(settings.DatabaseName);
        }

        //public static async Task<T> GetSettingsFromMongoAsync<T>(string title) where T : Settings
        //{
        //    var collection = GetCollection<T>("settings");
        //    return await collection.Find(document => document.Title == title).FirstOrDefaultAsync();
        //}

        //public static async Task<ElectricityGraphic?> GetDataFromMongoAsync(string? date = null)
        //{
        //    var collection = GetCollection();
        //    var dateToCheck = date ?? DateTime.Now.ToString("dd.MM.yyyy");
        //    return await collection.Find(document => document.Date == dateToCheck).FirstOrDefaultAsync();
        //}

        //public static async Task UpdateDataInMongoAsync(ElectricityGraphic graphic)
        //{
        //    var collection = GetCollection();

        //    var existed = collection.Find(document => document.Date == graphic.Date).FirstOrDefault();
        //    if (existed == null)
        //    {
        //        await AddDataInMongoAsync(graphic);
        //    }
        //    else
        //    {
        //        var updateDef = Builders<ElectricityGraphic>.Update.Set(x => x.UpdatedAt, DateTime.Now);

        //        Console.WriteLine("Updated graphic for date:" + graphic.Date);

        //        if (graphic.OnHours.Any())
        //        {
        //            updateDef = updateDef.Set(x => x.OnHours, graphic.OnHours);
        //        }

        //        if (!string.IsNullOrWhiteSpace(graphic.ImageLink))
        //        {
        //            updateDef = updateDef.Set(x => x.ImageLink, graphic.ImageLink);
        //        }

        //        if (graphic.Messages.Any())
        //        {
        //            updateDef = updateDef.Set(x => x.Messages, graphic.Messages);
        //        }

        //        if (graphic.NotifiedHours.Any())
        //        {
        //            updateDef = updateDef.Set(x => x.NotifiedHours, graphic.NotifiedHours);
        //        }

        //        await collection.FindOneAndUpdateAsync(g => g.Date == graphic.Date, updateDef);
        //    }
        //}

        //public async Task AddDataInMongoAsync(ElectricityGraphic graphic)
        //{
        //    var collection = GetCollection();

        //    var existed = collection.Find(document => document.Date == graphic.Date).FirstOrDefault();
        //    if (existed != null)
        //    {
        //        await UpdateDataInMongoAsync(graphic);
        //        return;
        //    }

        //    graphic.CreatedAt = DateTime.Now;
        //    await collection.InsertOneAsync(graphic);
        //    Console.WriteLine("Inserted graphic for date:" + graphic.Date);
        //}



        public IQueryable<T> Get<T>() where T : BaseEntity
        {
            var col = GetCollection<T>();
            return col.AsQueryable();
        }

        public async Task<T> GetAsync<T>(Guid id) where T : BaseEntity
        {
            var col = GetCollection<T>();
            var res = await col.FindAsync(f => f.Id == id);
            return await res.SingleOrDefaultAsync();
        }

        public async Task InsertAsync<T>(T item) where T : BaseEntity
        {
            var col = GetCollection<T>();
            await col.InsertOneAsync(item);
        }

        public async Task ReplaceAsync<T>(T item) where T : BaseEntity
        {
            var col = GetCollection<T>();
            await col.ReplaceOneAsync(f => f.Id == item.Id, item);
        }

        public async Task DeleteAsync<T>(Guid id) where T : BaseEntity
        {
            var col = GetCollection<T>();
            await col.DeleteOneAsync(f => f.Id == id);
        }


        private IMongoCollection<T> GetCollection<T>()
        {
            return _db.GetCollection<T>(ToCollectionName<T>());
        }
        private static string ToCollectionName<T>()
        {
            var x = typeof(T).Name.Replace("_", "");
            if (x.Length == 0) return "null";
            x = Regex.Replace(x, "([A-Z])([A-Z]+)($|[A-Z])",
                m => m.Groups[1].Value + m.Groups[2].Value.ToLower() + m.Groups[3].Value);
            return char.ToLower(x[0]) + x.Substring(1);
        }
    }
}
