using System.Threading;
using Common.Message;
using Helpers;
using Models;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business
{
    public abstract class CrawlerBusiness<Source, Result> where Result : CrawlStatistics
    {
        /// <summary>
        /// 原始数据
        /// </summary>
        protected List<Source> Sources { get; set; }
        /// <summary>
        /// 解析数据
        /// </summary>
        protected List<Result> Results { get; set; }
        /// <summary>
        /// 代理
        /// </summary>
        public string Proxy { get; private set; }

        /// <summary>
        /// 关键词
        /// </summary>
        public string KeyWord { get; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; }
        /// <summary>
        /// 同步任务数
        /// </summary>
        public int SyncCount { get; }
        /// <summary>
        /// 单次采集数
        /// </summary>
        public int CrawlCount { get; }
        /// <summary>
        /// 原始数据文件路径
        /// Twitter
        /// </summary>
        public string SourcePath { get; }

        /// <summary>
        /// 根目录
        /// </summary>
        public string RootPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CreateTime.ToString("yyyy-MM-dd"), GetType().Name);
        /// <summary>
        /// Csv文件保存目录
        /// </summary>
        public string CsvPath => Path.Combine(RootPath, KeyWord);
        /// <summary>
        /// Img文件保存目录
        /// </summary>
        public string ImagePath => Path.Combine(RootPath, KeyWord, "images");
        /// <summary>
        /// Video文件保存目录
        /// </summary>
        public string VideoPath => Path.Combine(RootPath, KeyWord, "videos");

        /// <summary>
        /// 邮件发送者
        /// </summary>
        protected MailHelper _mailer = new MailHelper("buxinghao@juxinli.com", "cmonitor@juxinli.com", "skwTwAFA7dfDGnjd", "smtp.exmail.qq.com", 25);
        /// <summary>
        /// 日志记录者
        /// </summary>
        protected LoggerHelper _logger => LoggerHelper.CreateLoggerHelper(Path.Combine(CreateTime.ToString("yyyy-MM-dd"), GetType().Name));
        /// <summary>
        /// 同步信号量
        /// </summary>
        protected SemaphoreSlim _slim { get; private set; }

        public CrawlerBusiness(string keyWord, DateTime time, int crawlCount, int syncCount, string sourcePath)
        {
            KeyWord = keyWord;
            CreateTime = time;
            CrawlCount = crawlCount;
            SyncCount = syncCount;
            SourcePath = Path.Combine(sourcePath, CreateTime.ToString("yyyyMMdd"), KeyWord.Trim());
            _slim = new SemaphoreSlim(syncCount);
            FileHelper.CreateDirectory(ImagePath);
            FileHelper.CreateDirectory(VideoPath);
        }

        /// <summary>
        /// 开始采集任务
        /// </summary>
        public async Task Crawler()
        {
            try
            {
                _logger.Log(LogLevel.Info, $"开始采集：{KeyWord}");
                await BorrowProxy();
                await Crawl();
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"采集异常：{KeyWord}{Environment.NewLine}{ex.ToString()}");
                throw ex;
            }
        }

        /// <summary>
        /// 开始解析任务
        /// </summary>
        public async Task Parser()
        {
            try
            {
                _logger.Log(LogLevel.Info, $"开始解析：{KeyWord}");
                await Parse();
                //保存Csv文件
                Helpers.CsvHelper.SaveCsv(Path.Combine(CsvPath, $"{DateTimeHelper.GetUnixTimestamp()}.csv"), Results);
                //保存数据库信息
                var tasks = Results.Select(async r => await r.Add()).ToArray();
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"解析异常：{KeyWord}{Environment.NewLine}{ex.ToString()}");
                throw ex;
            }
        }

        /// <summary>
        /// 开始下载任务
        /// </summary>
        public async Task Downloader()
        {
            try
            {
                _logger.Log(LogLevel.Info, $"开始下载：{KeyWord}");
                foreach (var r in Results)
                {
                    try
                    {
                        if (await HasHistorySame(r))
                        {
                            continue;
                        }
                        await BorrowProxy();
                        await Download(r);
                        _logger.Log(LogLevel.Info, $"下载成功：{r.UrlMd5}");
                        await r.UpdateSuccess(DownloadStatusEnum.Success);
                        _logger.Log(LogLevel.Info, $"Mongo写入成功：{r.UrlMd5}");
                    }
                    catch (Exception ex)
                    {
                        //下载失败
                        _logger.Log(LogLevel.Info, $"下载失败：{r.UrlMd5}。原因：{ex.Message}");
                        var _ = DeleteFaidedMedia(r);
                        await r.UpdateSuccess(DownloadStatusEnum.Failed);
                        _logger.Log(LogLevel.Info, $"Mongo写入成功：{r.UrlMd5}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"下载异常：{KeyWord}{Environment.NewLine}{ex.ToString()}");
                throw ex;
            }
        }

        /// <summary>
        /// 借代理
        /// </summary>
        protected async Task BorrowProxy()
        {
            while (true)
            {
                try
                {
                    Proxy = await TorProxyHelper.Instance.RandomProxy();
                    break;
                }
                catch (Exception ex)
                {
                    var _ = _mailer.SendMail($"{KeyWord}。连接Redis失败：{ex.ToString()}", "TorProxy异常报警");
                    Proxy = string.Empty;
                    _logger.Log(LogLevel.Info, $"获取代理失败：{ex.ToString()}");
                    continue;
                }
            }
            _logger.Log(LogLevel.Info, $"获取代理成功：{Proxy}");
        }

        protected abstract Task Crawl();

        protected abstract Task Parse();

        protected abstract Task Download(Result result);

        /// <summary>
        /// 发送通知邮件
        /// </summary>
        /// <param name="message">额外信息</param>
        protected virtual async Task SendMail(string message)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("时间：{0}<br/>关键词：{1}<br/>代理：{2}<br/>额外信息：{3}"
                , CreateTime.ToString("yyyy-MM-dd hh:mm:ss")
                , KeyWord
                , Proxy
                , message);
            await _mailer.SendMail(builder.ToString(), "YouTube");
        }

        /// <summary>
        /// 删除下载失败媒体
        /// </summary>
        protected virtual Task DeleteFaidedMedia(Result result)
        {
            var sourcePath = result.MediaType == MediaTypeEnum.image ? ImagePath : VideoPath;
            var files = Directory.GetFiles(sourcePath, $"*{result.UrlMd5}*", SearchOption.TopDirectoryOnly);
            FileHelper.DeleteFiles(files);
            _logger.Log(LogLevel.Info, $"保存媒体数据失败，已删除文件：{result.UrlMd5}");
            return Task.FromResult(true);
        }

        /// <summary>
        /// 是否与历史重复
        /// </summary>
        /// <param name="single"></param>
        /// <returns></returns>
        protected async Task<bool> HasHistorySame(CrawlStatistics single)
        {
            var same = await single.FindHistorySameByMd5();
            if (same == null)
            {
                return false;
            }
            _logger.Log(LogLevel.Info, $"有历史重复文件，跳过下载：{single.UrlMd5}");
            await single.UpdateSuccess(DownloadStatusEnum.Skip);
            return true;
        }

        /// <summary>
        /// 复制媒体信息
        /// </summary>
        /// <returns>复制成功/失败</returns>
        protected async Task<bool> CopyMedia(CrawlStatistics single)
        {
            if (single == null)
                return false;

            //找文件
            var sourcePath = single.MediaType == MediaTypeEnum.image ?
                  Directory.GetFiles(ImagePath, $"*{single.UrlMd5}*", SearchOption.TopDirectoryOnly).FirstOrDefault()
                : Directory.GetFiles(VideoPath, $"*{single.UrlMd5}*", SearchOption.TopDirectoryOnly).FirstOrDefault();
            if (!File.Exists(sourcePath))
                return false;
            if (sourcePath.Contains(RootPath))
            {
                //重复文件跳过下载
                _logger.Log(LogLevel.Info, $"相同文件，跳过复制：{single.UrlMd5}");
                return true;
            }
            //复制
            FileHelper.CopyFile(sourcePath, sourcePath, true);
            _logger.Log(LogLevel.Info, $"成功复制历史文件，跳过下载：{single.UrlMd5}");
            await single.UpdateSuccess(DownloadStatusEnum.Skip);
            return true;
        }
    }
}
