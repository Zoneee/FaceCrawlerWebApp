using Helpers;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Helpers
{
    public class MongoHelper
    {
        public static MongoClient Client = new MongoClient(ConfigHelper.GetConnectionString("SaveDataMongo"));
        public static IMongoDatabase Db = Client.GetDatabase("facex_test");
    }

    public class CollectionName
    {
        public const string crawl_statistics = nameof(crawl_statistics);
        public const string youtube = nameof(youtube);
        public const string twitter = nameof(twitter);
    }
}
