using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using Business;
using CsvHelper;
using Helpers;
using Microsoft.AspNetCore.Mvc;
using Models;
using NLog;
using System.Text.RegularExpressions;

namespace FaceCrawlerWebApp.Controllers
{
    public class ReloadController : Controller
    {
        [HttpPost("/Reload/Reload")]
        public async Task<string> Reload()
        {
            LoggerHelper logger = LoggerHelper.CreateLoggerHelper(@".\Reload");
            SemaphoreSlim slim = new SemaphoreSlim(1);
            /**
             * 读csv
             * 下载媒体
             * 存csv
             */
            var rootPath = @"C:\Users\www\Desktop\reload";
            var files = Directory.GetFiles(rootPath, "*.csv", SearchOption.AllDirectories);
            var tasks = files.AsParallel().Select(async file =>
            {
                logger.Log(LogLevel.Info, $"开始：{file}");
                List<TwitterResultCsv> news;
                //读csv
                using (var reader = new StreamReader(file))
                using (var csv = new CsvReader(reader))
                {
                    news = csv.GetRecords<TwitterResultCsv>().Select(n =>
                    {
                        var url = n.MediaUrl;
                        if (!url.Contains("video.twimg.com"))
                        {
                            var mediaUrl = Regex.Match(url, ".*?/media/[\\w|-]+").Value;
                            var imgUrl = Regex.Match(url, ".*?/img/[\\w|-]+").Value;
                            n.MediaUrl = $"{(string.IsNullOrWhiteSpace(mediaUrl) ? imgUrl : mediaUrl)}.png?name=large";
                            n.MediaType = MediaTypeEnum.image;
                        }
                        else
                        {
                            n.MediaType = MediaTypeEnum.video;
                        }
                        logger.Log(LogLevel.Debug, $"文件：{file} Type：{n.MediaType.ToString()} Url1：{url} Url2：{n.MediaUrl}");
                        return n;
                    }).ToList();
                }
                //保存路径
                string RootPath = Path.Combine(@"C:\Users\www\Desktop\reload", DateTime.Now.ToString("yyyy-MM-dd"), "Twitter");
                string CsvPath = Path.Combine(RootPath, Path.GetFileName(Path.GetDirectoryName(file)).Trim());
                string ImagePath = Path.Combine(RootPath, Path.GetFileName(Path.GetDirectoryName(file)).Trim(), "images");
                string VideoPath = Path.Combine(RootPath, Path.GetFileName(Path.GetDirectoryName(file)).Trim(), "videos");
                FileHelper.CreateDirectories(ImagePath, VideoPath);
                //下载任务
                var downloadTasks = news.AsParallel().Select(async n =>
                {
                    string savePath = n.MediaType == MediaTypeEnum.image ? ImagePath : VideoPath;
                    string saveType = n.MediaType == MediaTypeEnum.image ? ".png" : ".mp4";
                    if (System.IO.File.Exists(Path.Combine(savePath, $"{n.UrlMd5}{saveType}")))
                    {
                        logger.Log(LogLevel.Info, $"已存在，跳过：{Path.Combine(savePath, $"{n.UrlMd5}{saveType}")}");
                        return;
                    }
                    //下载
                    string Proxy;
                    while (true)
                    {
                        try
                        {
                            Proxy = await TorProxyHelper.Instance.RandomProxy();
                            break;
                        }
                        catch
                        {
                            continue;
                        }
                    }
                    try
                    {
                        using (HttpClientHandler handler = new HttpClientHandler() { Proxy = new WebProxy(Proxy) })
                        using (HttpClient client = new HttpClient(handler))
                        {
                            var response = await client.GetAsync(n.MediaUrl, HttpCompletionOption.ResponseHeadersRead);
                            if (response.StatusCode != HttpStatusCode.OK)
                            {
                                logger.Log(LogLevel.Error, $"文件：{file} 关键词：{n.KeyWord} Url：{n.MediaUrl} Md5：{n.UrlMd5} Status：{response.StatusCode}");
                                return;
                            }
                            using (var fStream = System.IO.File.Create(Path.Combine(savePath, $"{n.UrlMd5}{saveType}")))
                                await response.Content.CopyToAsync(fStream);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Log(LogLevel.Error, $"下载失败：{Path.Combine(savePath, $"{n.UrlMd5}{saveType}")}。{ex.Message}");
                    }
                }).ToArray();
                //存csv
                string csvName = string.Empty;
                try
                {
                    await slim.WaitAsync();
                    csvName = Path.Combine(CsvPath, $"{DateTimeHelper.GetUnixTimestamp()}.csv");
                    using (var writer = new StreamWriter(csvName))
                    using (var csv = new CsvWriter(writer))
                    {
                        csv.Configuration.IgnoreReferences = true;
                        csv.WriteRecords(news);
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, $"Csv保存失败：{csvName}。{ex.Message}");
                }
                finally
                {
                    slim.Release();
                }
                await Task.WhenAll(downloadTasks);
            }).ToList();
            await Task.WhenAll(tasks);
            return "完成";
        }
    }

    class OldTwitterCsv
    {
        public string userId { get; set; }
        public string userName { get; set; }
        public string userAccount { get; set; }
        public string twitterId { get; set; }
        public string createTime { get; set; }
        public string text { get; set; }
        public string mediaType { get; set; }
        public string keyWord { get; set; }
        public string mediaInfo { get; set; }
    }
}