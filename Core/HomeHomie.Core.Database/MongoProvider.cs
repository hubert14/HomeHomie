using HomeHomie.Core.Providers;
using MongoDB.Driver;
using System.Text.RegularExpressions;

namespace HomeHomie.Core.Database
{
    internal class MongoProvider : IDatabaseProvider
    {
        private IMongoDatabase _db;

        public MongoProvider(IDatabaseSettings settings)
        {
            _db = new MongoClient(settings.ConnectionString).GetDatabase(settings.DatabaseName);
        }

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
