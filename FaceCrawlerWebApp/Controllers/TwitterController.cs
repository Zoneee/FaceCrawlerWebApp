using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Newtonsoft.Json;
using Business;

namespace twitter_http.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TwitterController : ControllerBase
    {
        List<Twitter> Twitters = new List<Twitter>();

        /// <summary>
        /// 查询接口
        /// </summary>
        /// <returns></returns>
        [HttpPost("/Twitter/Search")]
        public async Task<string> Search()
        {
            Twitters = new List<Twitter>();
            var time = DateTime.Now;
            var fPath = @"C:\Users\www\Desktop\Twitter项目\Twitter查询关键字.txt";
            var fLines = System.IO.File.ReadAllLines(fPath).Distinct().ToArray();
            foreach (var word in fLines)
            {
                Twitter twitter = new Twitter(word.Trim(), time);
                await twitter.StartCrawl().ContinueWith(async t =>
                {
                    await Task.Delay(TimeSpan.FromMinutes(10)).ConfigureAwait(false);
                    await twitter.StartParse();
                }, TaskContinuationOptions.NotOnFaulted);
                Twitters.Add(twitter);
            }
            return string.Join(Environment.NewLine, Twitters.Select(s => JsonConvert.SerializeObject(s.Csvs)).ToArray());
        }

        /// <summary>
        /// 下载媒体接口
        /// NodeJs会回调这个接口
        /// </summary>
        /// <returns></returns>
        [HttpPost("/Twitter/Download")]
        public async Task<string> Download(string dateTime)
        {
            foreach (var twitter in Twitters)
            {
                await twitter.StartDownload();
            }
            return string.Join(Environment.NewLine, Twitters.Select(s => JsonConvert.SerializeObject(s.Csvs)).ToArray());
        }
    }
}
