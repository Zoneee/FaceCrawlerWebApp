using CsvHelper.Configuration.Attributes;
using Helpers;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace Models
{

    public class YouTubeResultJson
    {
        /// <summary>
        /// 采集网站
        /// </summary>
        public string ie_key { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// 采集类型
        /// </summary>
        public string _type { get; set; }
        /// <summary>
        /// 视频id
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// 视频url片段
        /// 同视频id
        /// </summary>
        public string url { get; set; }
        /// <summary>
        /// 完整视频url
        /// </summary>
        public string FormatUrl => $"https://www.youtube.com/watch?v={url}";
    }

    public class YouTubeResultCsv : CrawlStatistics, IEqualityComparer<CrawlStatistics>, IEqualityComparer<YouTubeResultCsv>
    {
        /// <summary>
        /// 用户类型
        /// 频道/用户
        /// </summary>
        [BsonElement("userType"), Ignore]
        public UserTypeEnum UserType => UserId == ChannelId ? UserTypeEnum.Channel : UserTypeEnum.User;
        /// <summary>
        /// 频道ID
        /// </summary>
        [BsonIgnore, Ignore]
        public string ChannelId { get; set; }
        /// <summary>
        /// 媒体类型
        /// </summary>
        public override MediaTypeEnum MediaType => MediaTypeEnum.video;
        /// <summary>
        /// 媒体url
        /// </summary>
        public override string MediaUrl => $"https://www.youtube.com/watch?v={InfoId}";
        public override CrawlTypeEnum CrawlType => CrawlTypeEnum.Youtube;
        public bool Equals(YouTubeResultCsv x, YouTubeResultCsv y)
        {
            return x.UrlMd5 == y.UrlMd5;
        }

        public int GetHashCode(YouTubeResultCsv obj)
        {
            return obj.UrlMd5.GetHashCode();
        }
    }

    public enum UserTypeEnum
    {
        User,
        Channel
    }
}