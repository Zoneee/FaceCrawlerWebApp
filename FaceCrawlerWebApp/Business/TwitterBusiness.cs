using Common.Message;
using Helpers;
using Models;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Business
{
    /// <summary>
    /// Twitter采集对象
    /// </summary>
    public class TwitterBusiness : CrawlerBusiness<TwitterResultJson, TwitterResultCsv>
    {
        public TwitterSearch Search { get; private set; }
        public string NodejsServer => ConfigHelper.GetConfigtion("CrawlConfig:NodejsServer");

        /// <summary>
        /// Twitter采集对象
        /// </summary>
        /// <param name="keyWord">关键词</param>
        /// <param name="rootPath">Csv保存路径</param>
        /// <param name="sourcePath">NodeJs采集结果保存路径</param>
        public TwitterBusiness(string keyWord, DateTime time, int crawlCount, int syncCount, string sourcePath) : base(keyWord, time, crawlCount, syncCount, sourcePath)
        {
        }

        protected override async Task Crawl()
        {
            Search = await GetTwitterSearch();
            await NodeJsCrawler();
            await SendMail("");
            Sources = ReadSource();
            if (Sources?.Count <= 0)
            {
                throw new Exception("无采集数据");
            }
        }

        protected override async Task Parse()
        {
            Results = ConvertData();
        }

        protected override async Task Download(TwitterResultCsv csv)
        {
            using (HttpClientHandler handler = new HttpClientHandler() { Proxy = new WebProxy(Proxy) })
            {
                using (HttpClient client = new HttpClient(handler))
                {
                    var response = await client.GetAsync(csv.MediaUrl, HttpCompletionOption.ResponseHeadersRead);
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        _logger.Log(LogLevel.Error, $"关键词：{csv.KeyWord} Url：{csv.MediaUrl} Md5：{csv.UrlMd5} Respopse状态：({(int)response.StatusCode}){response.StatusCode}");
                        return;
                    }
                    string savePath = csv.MediaType == MediaTypeEnum.image ? ImagePath : VideoPath;
                    string saveType = csv.MediaType == MediaTypeEnum.image ? ".png" : ".mp4";
                    string file = Path.Combine(savePath, $"{csv.UrlMd5}{saveType}");
                    if (File.Exists(file))
                    {
                        _logger.Log(LogLevel.Warn, $"文件已存在，跳过下载！关键词：{csv.KeyWord} Url：{csv.MediaUrl} Md5：{csv.UrlMd5} ");
                        return;
                    }
                    using (var fStream = File.Create(file))
                        await response.Content.CopyToAsync(fStream);
                }
            }
        }

        protected override async Task SendMail(string message)
        {
            var files = Directory.GetFiles(SourcePath, $"*log*", SearchOption.TopDirectoryOnly).FirstOrDefault();
            var r = (await File.ReadAllLinesAsync(files)).LastOrDefault();
            if (r.Contains("被封了"))
            {
                var _ = base.SendMail(r);
            }
        }

        #region 第一步：调用NodeJs采集数据

        /// <summary>
        /// 关键词转Twitter高级查询对象
        /// </summary>
        /// <returns></returns>
        async Task<TwitterSearch> GetTwitterSearch()
        {
            var data = new TwitterSearch();
            //data.proxy = "localhost:7777";
            await BorrowProxy();
            data.proxy = Proxy;
            data.token = KeyWord.Trim();
            data.count = CrawlCount;
            data.sinceTime = DateTime.Now.AddMonths(-1).ToString("yyyy-MM-01");
            data.untilTime = DateTime.Now.ToString("yyyy-MM-dd");
            if (KeyWord.Contains("#"))
            {
                data.hasTitle = KeyWord;
            }
            else
            {
                data.keyWord = KeyWord;
            }
            return data;
        }

        /// <summary>
        /// 调用NodeJs爬虫
        /// </summary>
        /// <returns></returns>
        async Task NodeJsCrawler()
        {
            try
            {
                using (HttpClient client = new HttpClient() { Timeout = TimeSpan.FromMinutes(5) })
                {
                    var resp = await client.PostAsync($"{NodejsServer}/twitter/search/"
                        , new StringContent(JsonConvert.SerializeObject(Search)
                        , Encoding.UTF8, "application/json"));
                    var r = await resp.Content.ReadAsStringAsync();
                }
            }
            catch
            {
                //超时后NodeJs爬虫会继续运行，所以不抛出异常可以在B轮处理A轮的数据，如果B轮失败的情况
            }
        }

        #endregion

        #region 第二步：转存NodeJs采集数据

        /// <summary>
        /// 读取NodeJs采集结果
        /// </summary>
        List<TwitterResultJson> ReadSource()
        {
            var files = Directory.GetFiles(SourcePath, $"*format*", SearchOption.TopDirectoryOnly);
            List<TwitterResultJson> jsons = new List<TwitterResultJson>();
            foreach (var file in files)
            {
                var r = File.ReadAllText(file);
                var j = JsonConvert.DeserializeObject<List<TwitterResultJson>>(r);
                jsons.AddRange(j);
            }
            return jsons;
        }

        /// <summary>
        /// Json转Csv
        /// </summary>
        List<TwitterResultCsv> ConvertData()
        {
            var csvs = new List<TwitterResultCsv>();
            foreach (var json in Sources)
            {
                if (json.mediaInfo == null)
                {
                    continue;
                }

                var item = json.mediaInfo.Select(url =>
                {
                    var mediaType = (MediaTypeEnum)Enum.Parse(typeof(MediaTypeEnum), json.mediaType);
                    var mediaUrl = Regex.Match(url, ".*?/media/[\\w|-]+").Value;
                    var imgUrl = Regex.Match(url, ".*?/img/[\\w|-]+").Value;
                    if (mediaType == MediaTypeEnum.image)
                    {
                        url = $"{(!string.IsNullOrWhiteSpace(mediaUrl) ? mediaUrl : imgUrl)}.png?name=large";
                    }
                    return new TwitterResultCsv()
                    {
                        ReleaseTime = DateTime.Parse(json.releaseTime),
                        MediaUrl = url,
                        MediaType = mediaType,
                        Text = json.text,
                        InfoId = json.twitterId,
                        UserAccount = json.userId,
                        UserId = json.userAccount,
                        UserName = json.userName,
                        KeyWord = json.keyWord,
                        QuotedId = json.quotedId,
                    };
                }).ToList();
                csvs.AddRange(item);
            }
            return csvs;
        }

        #endregion

    }
}
