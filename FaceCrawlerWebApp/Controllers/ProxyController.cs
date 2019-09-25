using System.Threading.Tasks;
using Helpers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace twitter_http.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProxyController : ControllerBase
    {
        /// <summary>
        /// Redis测试
        /// </summary>
        /// <returns></returns>
        [HttpPost("/Proxy/Redis/Test")]
        public async Task<string> Redis()
        {
            return JsonConvert.SerializeObject(await TorProxyHelper.Instance.GetTorProxy());
        }
    }
}