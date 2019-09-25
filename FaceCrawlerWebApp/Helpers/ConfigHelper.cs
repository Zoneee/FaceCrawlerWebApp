using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Helpers
{
    public static class ConfigHelper
    {
        static IConfiguration _configuration = null;
        public static void SetConfigtion(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        /// <summary>
        /// 获取配置信息
        /// </summary>
        /// <param name="key">Key或Key:Key</param>
        /// <returns></returns>
        public static string GetConfigtion(string key)
        {
            if (_configuration.GetSection(key) == null)
            {
                throw new ArgumentNullException($"{key}节点不存在！");
            }
            return _configuration.GetSection(key).Value;
        }

        /// <summary>
        /// 获取数据库配置信息
        /// ConnectionStrings 节点下
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetConnectionString(string key)
        {
            return _configuration.GetConnectionString(key);
        }
    }
}
