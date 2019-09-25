using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Models
{
    public class CrawlConfig
    {
        public CrawlTaskInfo Twitter { get; set; }
        public CrawlTaskInfo YouTube { get; set; }
        /// <summary>
        /// Nodejs爬虫服务地址
        /// </summary>
        public string NodejsServer { get; set; }
        /// <summary>
        /// 定时压缩
        /// </summary>
        public bool TimingCompress { get; set; }
        /// <summary>
        /// 定时压缩运行状态
        /// </summary>
        public string TimingCompressStatus { get; set; }
    }

    public class CrawlTaskInfo
    {
        /// <summary>
        /// 单次采集数量
        /// </summary>
        public int CrawlCount { get; set; }
        /// <summary>
        /// 并行采集数量
        /// </summary>
        public int SyncCount { get; set; }
        /// <summary>
        /// 原始数据文件路径
        /// </summary>
        public string SourcePath { get; set; }
        /// <summary>
        /// 关键词文件路径
        /// </summary>
        public string KeyWordPath { get; set; }
    }
}
