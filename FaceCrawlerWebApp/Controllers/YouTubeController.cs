using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IO;
using Business;

namespace twitter_http.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class YouTubeController : ControllerBase
    {
        List<YouTube> YouTubes = new List<YouTube>();

        /// <summary>
        /// YouTube-dl查询接口
        /// </summary>
        /// <returns></returns>
        [HttpPost("/YouTube/Search")]
        public async Task<string> Search()
        {
            YouTubes = new List<YouTube>();
            var time = DateTime.Now;
            var fPath = @"C:\Users\www\Desktop\YouTube项目\YouTube查询关键词.txt";
            var fLines = System.IO.File.ReadAllLines(fPath).Distinct().ToArray();
            foreach (var word in fLines)
            {
                var youtube = new YouTube(word.Trim(), time);
                await youtube.StartCrawl().ContinueWith(async t =>
                {
                    await Task.Delay(TimeSpan.FromMinutes(10)).ConfigureAwait(false);
                    await youtube.StartParse();
                }, TaskContinuationOptions.NotOnFaulted);
                YouTubes.Add(youtube);
            }
            return string.Join(Environment.NewLine, YouTubes.Select(s => JsonConvert.SerializeObject(s.Results)).ToArray());
        }

        /// <summary>
        /// YouTube-dl下载接口
        /// </summary>
        /// <returns></returns>
        [HttpPost("/YouTube/Download")]
        public async Task<string> Download()
        {
            foreach (var youTube in YouTubes)
            {
                await youTube.StartDownload();
            }
            return string.Join(Environment.NewLine, YouTubes.Select(s => JsonConvert.SerializeObject(s.Results)).ToArray());
        }
    }
}