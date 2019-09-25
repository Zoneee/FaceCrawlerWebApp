using CsvHelper.Configuration.Attributes;
using Helpers;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Models
{
    public abstract class CrawlStatistics : IEqualityComparer<CrawlStatistics>
    {
        /// <summary>
        /// MongoId
        /// </summary>
        [Ignore]
        public ObjectId Id { get; set; }
        /// <summary>
        /// 关键词
        /// </summary>
        [BsonElement("keyWord"), Name("keyWord")]
        public string KeyWord { get; set; }
        /// <summary>
        /// YouTube用户/频道Id、Twitter用户被“@”名称
        /// </summary>
        [BsonElement("userId"), Name("userId")]
        public string UserId { get; set; }
        /// <summary>
        /// YouTube用户可读名、Twitter用户可读名
        /// </summary>
        [BsonElement("userName"), Name("userName")]
        public string UserName { get; set; }
        /// <summary>
        /// YouTube视频Id、Twitter推文Id
        /// YouTube视频Id拼接某个视频url用
        /// Twitter推文Id拼接某条推特url用
        /// </summary>
        [BsonElement("infoId"), Ignore]
        public string InfoId { get; set; }
        /// <summary>
        /// 采集类型
        /// </summary>
        [BsonElement("crawlType"), BsonRepresentation(BsonType.String), Ignore]
        public abstract CrawlTypeEnum CrawlType { get; }
        /// <summary>
        /// 发布时间
        /// </summary>
        [BsonElement("releaseTime"), Ignore]
        public DateTime ReleaseTime { get; set; }
        /// <summary>
        /// 媒体类型
        /// </summary>
        [BsonElement("mediaType"), BsonRepresentation(BsonType.String), Name("mediaType")]
        public virtual MediaTypeEnum MediaType { get; set; }
        /// <summary>
        /// 媒体url
        /// </summary>
        [BsonElement("mediaUrl"), Name("mediaUrl")]
        public virtual string MediaUrl { get; set; }
        /// <summary>
        /// 媒体url md5签名
        /// 做文件名
        /// </summary>
        [BsonElement("urlMd5"), Name("urlMd5")]
        public string UrlMd5 => MD5Helper.Encrypt(MediaUrl);
        /// <summary>
        /// 下载状态
        /// </summary>
        [BsonElement("downloadStatus"), BsonRepresentation(BsonType.String), Ignore]
        public DownloadStatusEnum DownloadStatus { get; set; }
        /// <summary>
        /// YouTube视频标题、Twitter推文正文
        /// YouTube视频标题
        /// Twitter推文正文 （文字+emoji表情）
        /// </summary>
        [BsonElement("text"), Ignore]
        public string Text { get; set; }
        public bool Equals(CrawlStatistics x, CrawlStatistics y)
        {
            return x.UrlMd5 == y.UrlMd5;
        }

        public int GetHashCode(CrawlStatistics obj)
        {
            return obj.UrlMd5.GetHashCode();
        }

        /// <summary>
        /// MongoDb集合
        /// </summary>
        IMongoCollection<CrawlStatistics> _collection = MongoHelper.Db.GetCollection<CrawlStatistics>(CollectionName.crawl_statistics);
        /// <summary>
        /// 添加单项
        /// </summary>
        /// <returns></returns>
        public async Task Add()
        {
            await _collection.InsertOneAsync(this);
        }

        /// <summary>
        /// 更新下载状态
        /// </summary>
        /// <param name="success">成功/失败 true/false</param>
        /// <returns></returns>
        public async Task UpdateSuccess(DownloadStatusEnum downloadStatus)
        {
            var filter = Builders<CrawlStatistics>.Filter.Eq(s => s.Id, Id);
            var update = Builders<CrawlStatistics>.Update.Set(s => s.DownloadStatus, downloadStatus);
            await _collection.UpdateOneAsync(filter, update);

        }

        /// <summary>
        /// 获取相同md5对象
        /// </summary>
        /// <returns></returns>
        public async Task<CrawlStatistics> FindHistorySameByMd5()
        {
            var filter = Builders<CrawlStatistics>.Filter.And(
                        Builders<CrawlStatistics>.Filter.Eq(s => s.UrlMd5, UrlMd5),
                        Builders<CrawlStatistics>.Filter.Eq(s => s.DownloadStatus, DownloadStatus),
                        Builders<CrawlStatistics>.Filter.Ne(s => s.Id, Id),
                        Builders<CrawlStatistics>.Filter.Gte(s => s.ReleaseTime, ReleaseTime.AddDays(-2)),
                        Builders<CrawlStatistics>.Filter.Lte(s => s.ReleaseTime, ReleaseTime));
            var sort = Builders<CrawlStatistics>.Sort.Descending(s => s.ReleaseTime);
            var options = new FindOptions<CrawlStatistics>()
            {
                Sort = sort,
                Limit = 1
            };
            using (var cursor = await _collection.FindAsync(filter, options))
            {
                return await cursor.SingleOrDefaultAsync();
            }
        }
    }

    public enum MediaTypeEnum
    {
        image,
        video
    }

    public enum CrawlTypeEnum
    {
        Youtube,
        Twitter
    }

    public enum DownloadStatusEnum
    {
        Success,
        Failed,
        Skip
    }
}
