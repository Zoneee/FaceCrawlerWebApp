using Helpers;
using Models;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business
{
    /// <summary>
    /// YouTube采集对象
    /// </summary>
    public class YouTubeBusiness : CrawlerBusiness<string, YouTubeResultCsv>
    {
        /// <summary>
        /// YouTube采集对象
        /// </summary>
        /// <param name="keyWord">关键词</param>
        /// <param name="rootPath">Csv保存路径</param>
        /// <param name="sourcePath">NodeJs采集结果保存路径。对于YouTube该字段无意义</param>
        public YouTubeBusiness(string keyWord, DateTime time, int crawlCount, int syncCount, string sourcePath) : base(keyWord, time, crawlCount, syncCount, sourcePath)
        {

        }

        protected override async Task Crawl()
        {
            var playList = await CrawlPlayList();
            Sources = await CrawlMediaInfo(playList);
            if (Sources.Count < 1)
            {
                throw new Exception("有效数据小于1");
            }
        }

        protected override async Task Parse()
        {
            Results = ConvertData();
        }

        protected override async Task Download(YouTubeResultCsv csv)
        {
            var cmdProxy = string.IsNullOrWhiteSpace(Proxy) ? "" : $"--proxy \"{Proxy}\"";
            var r = CmdHelper.ReadAllMessage(VideoPath, $"youtube-dl {cmdProxy} -o {csv.UrlMd5}.%(ext)s {csv.MediaUrl}");
            if (string.IsNullOrWhiteSpace(r.Item1))
                throw new Exception(r.Item2);
            _logger.Log(LogLevel.Debug, r.Item1);
        }

        #region 调用youtube-dl

        async Task<List<YouTubeResultJson>> CrawlPlayList()
        {
            var cmdProxy = string.IsNullOrWhiteSpace(Proxy) ? "" : $"--proxy \"{Proxy}\"";
            var cmdCrawlWord = $"\"ytsearch{CrawlCount}:{KeyWord}\"";
            //获取列表
            //-i, --ignore-errors              Continue on download errors, for example to skip unavailable videos in a playlist
            //-j, --dump-json                  Simulate, quiet but print JSON information.See the "OUTPUT TEMPLATE" for a description of available keys.
            //--flat-playlist                  Do not extract the videos of a playlist,only list them.
            //youtube-dl --proxy "10.93.117.174:8118" -i -j --flat-playlist "ytsearch50:Protest"
            //{"ie_key": "Youtube", "title": "Sunday protests spiral into chaos as Hong Kong police and demonstrators clash", "_type": "url", "id": "dxjqUGtgiSY", "url": "dxjqUGtgiSY"}
            var listCommand = $"youtube-dl {cmdProxy} -i -j --flat-playlist {cmdCrawlWord}";
            var source = CmdHelper.ReadAllMessage(VideoPath, listCommand);
            if (string.IsNullOrWhiteSpace(source.Item1))
                throw new Exception(source.Item2);
            _logger.Log(LogLevel.Debug, $"{listCommand}{Environment.NewLine}{source.Item1}");
            return source.Item1.Split("\n").Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => JsonConvert.DeserializeObject<YouTubeResultJson>(s)).ToList();
        }

        async Task<List<string>> CrawlMediaInfo(List<YouTubeResultJson> sourceLists)
        {
            var cmdProxy = string.IsNullOrWhiteSpace(Proxy) ? "" : $"--proxy \"{Proxy}\"";
            var cmdCrawlInfo = "%(uploader_id)s___%(uploader)s___%(id)s___%(upload_date)s___%(title)s___%(channel_id)s";
            //采集信息
            //-i, --ignore-errors              Continue on download errors, for example to skip unavailable videos in a playlist
            //--get-filename                   Simulate, quiet but print output filename
            //-o, --output TEMPLATE            Output filename template, see the "OUTPUT TEMPLATE" for all the info
            //youtube-dl --proxy "10.93.117.174:8118" -i --get-filename -o '{uploaderId# %(uploader_id)s}' https://www.youtube.com/watch?v=8Nr2OKlKP7M
            var crawlerCommand = sourceLists.Select(s => $"youtube-dl {cmdProxy} -i --get-filename -o {cmdCrawlInfo} {s.FormatUrl}").ToList();
            var crawlerTasks = crawlerCommand.AsParallel().Select(async s =>
            {
                try
                {
                    await _slim.WaitAsync();
                    return CmdHelper.ReadAllMessage(VideoPath, s);
                }
                finally
                {
                    _slim.Release();
                }

            }).ToList();
            var crawlerLists = (await Task.WhenAll(crawlerTasks)).ToList();
            //记录采集日志
            _logger.Log(LogLevel.Debug, $"{JsonConvert.SerializeObject(crawlerCommand)}{Environment.NewLine}{JsonConvert.SerializeObject(crawlerLists)}");
            //结果筛选
            var successList = crawlerLists.Where(s => !string.IsNullOrWhiteSpace(s.Item1)).Select(s => s.Item1).ToList();
            var failedList = crawlerLists.Where(s => !string.IsNullOrWhiteSpace(s.Item2)).Select(s => s.Item2).ToList();
            //判断是否发送邮件
            if (failedList.Count > CrawlCount / 2)
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine($"额定采集数据：{CrawlCount}。缺失：{failedList.Count}<br />");
                builder.AppendLine($"原因：{string.Join(Environment.NewLine, failedList)}");
                var _ = SendMail(builder.ToString());
            }
            return successList;
        }

        #endregion

        #region 第二步：转存youtube-dl采集数据

        List<YouTubeResultCsv> ConvertData()
        {
            var result = Sources.Select(s =>
            {
                var sp = s.Split("___");
                return new YouTubeResultCsv()
                {
                    KeyWord = KeyWord,
                    UserId = sp[0],
                    UserName = sp[1],
                    InfoId = sp[2],
                    ReleaseTime = DateTime.ParseExact(sp[3], "yyyyMMdd", null),
                    Text = sp[4],
                    ChannelId = sp[5],
                };
            }).ToList();

            return result;
        }

        #endregion
    }
}
