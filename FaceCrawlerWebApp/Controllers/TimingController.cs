using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Business;
using Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Models;
using Newtonsoft.Json.Linq;

namespace FaceCrawlerWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimingController : ControllerBase
    {
        public CrawlConfig Config;
        public TimingController(IOptions<CrawlConfig> options)
        {
            Config = options.Value;
        }

#if DEBUG
        /// <summary>
        /// Twitter定时任务（10分钟）
        /// </summary>
        /// <returns></returns>
        [HttpPost("/Timing/Twitter")]
        public async Task<ActionResult> TwitterTiming()
        {
            List<Task> syncTasks = new List<Task>();
            dynamic o = new JObject();
            try
            {
                if (Config.TimingCompress)
                    Globals.StartCompress();
                Globals.CrawlTaskCts.Token.ThrowIfCancellationRequested();

                var time = DateTime.Now;
                var fPath = Config.Twitter.KeyWordPath;
                var fLines = System.IO.File.ReadAllLines(fPath).Distinct().ToArray();

                foreach (var word in fLines)
                {

                    try
                    {
                        TwitterBusiness twitter = new TwitterBusiness(word.Trim(), time, Config.Twitter.CrawlCount, Config.Twitter.SyncCount, Config.Twitter.SourcePath);
                        await twitter.Crawler();
                        var task = Task.Run(async () =>
                        {
                            await twitter.Parser();
                            await twitter.Downloader();
                        });
                        syncTasks.Add(task);
                    }
                    catch { }
                }
                await Task.WhenAll(syncTasks);
            }
            catch (Exception ex)
            {
                var faulted = syncTasks.Select(s => s.Status == TaskStatus.Faulted).ToArray();
                o.Message = $"结束执行。失败数：{faulted.Length}。";
                o.ExMessage = ex.Message;
                return new JsonResult(o);
            }
            o.Message = $"结束执行。失败数：0。";
            return new JsonResult(o);
        }
#else
        /// <summary>
        /// Twitter定时任务（10分钟）
        /// </summary>
        /// <returns></returns>
        [HttpPost("/Timing/Twitter")]
        public async Task<ActionResult> TwitterTiming()
        {
            List<Task> syncTasks = new List<Task>();
            dynamic o = new JObject();
            try
            {
                if (Config.TimingCompress)
                    Globals.StartCompress();
                //SemaphoreSlim slim = new SemaphoreSlim(Config.Twitter.SyncCount);
                while (true)
                {
                    Globals.CrawlTaskCts.Token.ThrowIfCancellationRequested();

                    var time = DateTime.Now;
                    var fPath = Config.Twitter.KeyWordPath;
                    var fLines = System.IO.File.ReadAllLines(fPath).Distinct().ToArray();

                    foreach (var word in fLines)
                    {

                        try
                        {
                            TwitterBusiness twitter = new TwitterBusiness(word.Trim(), time, Config.Twitter.CrawlCount, Config.Twitter.SyncCount, Config.Twitter.SourcePath);
                            await twitter.Crawler();
                            var task = Task.Run(async () =>
                            {
                                await twitter.Parser();
                                await twitter.Downloader();
                            });
                            syncTasks.Add(task);
                        }
                        catch { }
                    }
                    await Task.Delay(TimeSpan.FromMinutes(10), Globals.CrawlTaskCts.Token).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    await Task.WhenAll(syncTasks);
                }
                catch { }
                var faulted = syncTasks.Select(s => s.Status == TaskStatus.Faulted).ToArray();
                o.Message = $"结束执行。失败数：{faulted?.Length}。";
                o.ExMessage = ex.Message;
                return new JsonResult(o);
            }
        }
#endif

#if DEBUG
        /// <summary>
        /// YouTube定时任务（10分钟）
        /// </summary>
        /// <returns></returns>
        [HttpPost("/Timing/YouTube")]
        public async Task<ActionResult> YouTubeTiming()
        {
            List<Task> syncTasks = new List<Task>();
            dynamic o = new JObject();
            try
            {
                if (Config.TimingCompress)
                    Globals.StartCompress();
                Globals.CrawlTaskCts.Token.ThrowIfCancellationRequested();

                var time = DateTime.Now;
                var fPath = Config.YouTube.KeyWordPath;
                var fLines = System.IO.File.ReadAllLines(fPath).Distinct().ToArray();

                foreach (var word in fLines)
                {
                    try
                    {
                        var youtube = new YouTubeBusiness(word.Trim(), time, Config.YouTube.CrawlCount, Config.YouTube.SyncCount, Config.YouTube.SourcePath);
                        await youtube.Crawler();
                        var task = Task.Run(async () =>
                        {
                            await youtube.Parser();
                            await youtube.Downloader();
                        });
                        syncTasks.Add(task);
                    }
                    catch { }
                }
                await Task.WhenAll(syncTasks);
            }
            catch (Exception ex)
            {
                var faulted = syncTasks.Select(s => s.Status == TaskStatus.Faulted).ToArray();
                o.Message = $"结束执行。失败数：{faulted?.Length}。";
                o.ExMessage = ex.Message;
                return new JsonResult(o);
            }
            o.Message = $"结束执行。失败数：0。";
            return new JsonResult(o);
        }
#else
        /// <summary>
        /// YouTube定时任务（10分钟）
        /// </summary>
        /// <returns></returns>
        [HttpPost("/Timing/YouTube")]
        public async Task<ActionResult> YouTubeTiming()
        {
            List<Task> syncTasks = new List<Task>();
            dynamic o = new JObject();
            try
            {
                if (Config.TimingCompress)
                    Globals.StartCompress();
                //SemaphoreSlim slim = new SemaphoreSlim(Config.YouTube.SyncCount);
                while (true)
                {
                    Globals.CrawlTaskCts.Token.ThrowIfCancellationRequested();

                    var time = DateTime.Now;
                    var fPath = Config.YouTube.KeyWordPath;
                    var fLines = System.IO.File.ReadAllLines(fPath).Distinct().ToArray();

                    foreach (var word in fLines)
                    {
                        try
                        {
                            var youtube = new YouTubeBusiness(word.Trim(), time, Config.YouTube.CrawlCount, Config.YouTube.SyncCount, Config.YouTube.SourcePath);
                            await youtube.Crawler();
                            var task = Task.Run(async () =>
                            {
                                await youtube.Parser();
                                await youtube.Downloader();
                            });
                            syncTasks.Add(task);
                        }
                        catch { }
                    }
                    await Task.Delay(TimeSpan.FromMinutes(10), Globals.CrawlTaskCts.Token).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    await Task.WhenAll(syncTasks);
                }
                catch { }
                var faulted = syncTasks.Select(s => s.Status == TaskStatus.Faulted).ToArray();
                o.Message = $"结束执行。失败数：{faulted?.Length}。";
                o.ExMessage = ex.Message;
                return new JsonResult(o);
            }
        }
#endif

        /// <summary>
        /// 停止定时任务
        /// </summary>
        /// <returns></returns>
        [HttpPost("/Timing/Stop")]
        public string StopTask()
        {
            return Globals.StopTask();
        }

        /// <summary>
        /// 关闭定时压缩
        /// </summary>
        [HttpGet("/Timing/CloseCompress")]
        public string CloseCompress()
        {
            return Globals.CloseCompress();
        }

        /// <summary>
        /// 查看当前环境配置信息
        /// </summary>
        [HttpGet("/Config/CrawlTaskInfo")]
        public ActionResult CrawlTaskInfo()
        {
            Config.TimingCompressStatus = Globals.TimingCompressTask?.Status.ToString();
            return new JsonResult(Config);
        }

        ///// <summary>
        ///// 下载指定的文件
        ///// </summary>
        ///// <param name="filePath">文件完整路径</param>
        ///// <returns></returns>
        //[HttpGet("/Download/BigFile")]
        //public async Task<IActionResult> DownloadBigFile(string filePath)
        //{
        //    var fileName = Path.GetFileName(filePath);
        //    var contentDisposition = "attachment;" + "filename=" + HttpUtility.UrlEncode(fileName);
        //    Response.ContentType = "application/octet-stream";
        //    Response.Headers.Add("Content-Disposition", new string[] { contentDisposition });
        //    using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        //    {
        //        int bufferSize = 1024;
        //        using (Response.Body)
        //        {
        //            long contentLength = fs.Length;
        //            Response.ContentLength = fs.Length;
        //            byte[] buffer;
        //            long hasRead = 0;
        //            while (hasRead < fs.Length)
        //            {
        //                if (HttpContext.RequestAborted.IsCancellationRequested)
        //                {
        //                    break;
        //                }
        //                buffer = new byte[bufferSize];
        //                int currentRead = await fs.ReadAsync(buffer, 0, bufferSize);
        //                await Response.Body.WriteAsync(buffer, 0, currentRead);
        //                await Response.Body.FlushAsync();
        //                hasRead += currentRead;
        //            }
        //        }
        //    }
        //    return new EmptyResult();
        //}
    }
}