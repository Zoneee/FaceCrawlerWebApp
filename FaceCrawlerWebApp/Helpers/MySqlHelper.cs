using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models;
using NLog;
using SqlSugar;

namespace Helpers
{
    public class MySqlHelper
    {
        public SqlSugarClient Db;
        LoggerHelper _logger;
        public MySqlHelper(LoggerHelper logger)
        {
            _logger = logger;
            Db = new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = ConfigHelper.GetConnectionString("SaveDataMysql"),
                //server=localhost;user id=root;password=root;persistsecurityinfo=True;database=face_crawler;Charset=utf8;
                DbType = DbType.MySql,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute
            });
            Db.Aop.OnLogExecuting = (sql, pars) =>
            {
                _logger.Log(LogLevel.Trace, $"{sql}{Environment.NewLine}{Db.Utilities.SerializeObject(pars.ToDictionary(it => it.ParameterName, it => it.Value))}");
            };
        }

        public DbSet<crawl_statistics> Statistics => new DbSet<crawl_statistics>(Db);
    }

    public class DbSet<T> : SimpleClient<T> where T : class, new()
    {
        public DbSet(ISqlSugarClient context) : base(context)
        {
        }
    }
}
