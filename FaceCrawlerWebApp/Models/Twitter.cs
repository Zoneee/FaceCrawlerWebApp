using CsvHelper.Configuration.Attributes;
using Helpers;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace Models
{
    /// <summary>
    /// Twitter查询格式
    /// </summary>
    public class TwitterSearch
    {
        /// <summary>
        /// 唯一标识
        /// </summary>
        public string token { get; set; }
        /// <summary>
        /// 关键字
        /// </summary>
        public string keyWord { get; set; }
        /// <summary>
        /// 精准短语
        /// </summary>
        public string phraseWord { get; set; }
        /// <summary>
        /// 任何一词
        /// </summary>
        public string anyWord { get; set; }
        /// <summary>
        /// 排除词语
        /// </summary>
        public string excludeWord { get; set; }
        /// <summary>
        /// 话题
        /// </summary>
        public string hasTitle { get; set; }
        /// <summary>
        /// 来自帐号
        /// </summary>
        public string fromAccount { get; set; }
        /// <summary>
        /// 发送账号
        /// </summary>
        public string toAccount { get; set; }
        /// <summary>
        /// 提及账号
        /// </summary>
        public string mentionAccount { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public string sinceTime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public string untilTime { get; set; }
        /// <summary>
        /// 条数
        /// </summary>
        public int count { get; set; }

        /// <summary>
        /// 代理
        /// </summary>
        public string proxy { get; set; }
    }


    /// <summary>
    /// Twitter Json格式
    /// </summary>
    public class TwitterResultJson
    {
        /// <summary>
        /// 关键词
        /// </summary>
        public string keyWord { get; set; }
        /// <summary>
        /// Post用户ID
        /// </summary>
        public string userId { get; set; }
        /// <summary>
        /// Post用户可读名
        /// </summary>
        public string userName { get; set; }
        /// <summary>
        /// Post用户名ID
        /// </summary>
        public string userAccount { get; set; }
        /// <summary>
        /// 引用Twitter ID
        /// </summary>
        public string quotedId { get; set; }
        /// <summary>
        /// Twitter ID
        /// </summary>
        public string twitterId { get; set; }
        /// <summary>
        /// 发布时间
        /// </summary>
        public string releaseTime { get; set; }
        /// <summary>
        /// 正文 （纯文字）
        /// </summary>
        public string text { get; set; }
        /// <summary>
        /// 媒体类型 'video'or'image'
        /// </summary>
        public string mediaType { get; set; }
        /// <summary>
        /// 媒体url
        /// </summary>
        public string[] mediaInfo { get; set; }
    }

    /// <summary>
    /// Twitter Csv格式
    /// </summary>
    public class TwitterResultCsv : CrawlStatistics, IEqualityComparer<CrawlStatistics>, IEqualityComparer<TwitterResultCsv>
    {
        /// <summary>
        /// Twitter用户id
        /// </summary>
        [BsonElement("userAccount")]
        public string UserAccount { get; set; }
        /// <summary>
        /// 引用Twitter ID
        /// 拼接某条推特url用
        /// </summary>
        [BsonElement("quotedId"), Ignore]
        public string QuotedId { get; set; }
        public override MediaTypeEnum MediaType { get => base.MediaType; set => base.MediaType = value; }
        public override CrawlTypeEnum CrawlType => CrawlTypeEnum.Twitter;

        public bool Equals(TwitterResultCsv x, TwitterResultCsv y)
        {
            return x.UrlMd5 == y.UrlMd5;
        }

        public int GetHashCode(TwitterResultCsv obj)
        {
            return obj.UrlMd5.GetHashCode();
        }
    }
}
