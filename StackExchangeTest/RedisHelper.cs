using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace StackExchangeTest
{
    public class RedisHelper
    {
        private static readonly Rediscache cache = new Rediscache();
        public static int Add(string key, object value)
        {
            if (value != null)
                cache.Insert(key, value);
            return 1;
        }
        
        public static T GetDatas<T>(string key) where T : class
        {
            if (key == null || key.Length < 1)
            {
                return null;
            }
            T t = default(T);
            var res = cache.Get<T>(key);
            if (res != null)
            {
                t = res;
            }

            return t;
        }
        public static int LAdd(string key, object value)
        {
            if (value != null)
                cache.LPushs(key, value);
            return 1;
        }
        public static T LGetDatas<T>(string key) where T:class
        {
            if(key== null || key.Length<1)
            {
                return null;
            }
            T t = default(T);
            var res = cache.LPops<T>(key);
            if(res != null)
            {
                t = res;
            }

            return t;
        }
    }

    internal class Rediscache : IDisposable
    {      
        public Rediscache()
        {
            var redisConnect = ConnectionMultiplexer.Connect(_conn);
            _database = redisConnect.GetDatabase();
        }

        //添加析构函数
        ~Rediscache()
        {
            ToDispose(false);
        }
        //readonly,运行时确定其值，定义的时候或构造函数赋值
        private readonly string _conn = "127.0.0.1:6379";
        private readonly IDatabase _database;
        //当前资源是否释放
        private bool disposed;

        //实现Dispose方法
        public void Dispose()
        {
            ToDispose(true);
        }
        //当前或子类，实现释放的具体逻辑,disposing:True,是否需要显示释放托管资源
        protected virtual void ToDispose(bool disposing)
        {
            if(disposing)
            {

            }

            _database.Multiplexer.GetSubscriber().UnsubscribeAll();
            _database.Multiplexer.Dispose();
        }

        #region 数据类型：string
        /// <summary>
        /// redis:string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(string key) where T : class
        {
            var strValue = _database.StringGet(key);
            return string.IsNullOrEmpty(strValue) ? null : JsonConvert.DeserializeObject<T>(strValue.ToString());
        }

        /// <summary>
        /// redis:string
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        public void Insert(string key, object obj)
        {
            var value = JsonConvert.SerializeObject(obj);
            _database.StringSet(key, value);
        }
        /// <summary>
        /// redis:string
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <param name="absoluteExpiration"></param>
        /// <param name="slidingExpiration"></param>
        public void Insert(string key, object obj, DateTime absoluteExpiration, TimeSpan slidingExpiration)
        {
            if (obj == null)
                return;
            var value = JsonConvert.SerializeObject(obj);
            if (slidingExpiration != new TimeSpan())
                _database.StringSet(key, value, slidingExpiration);
            else if (absoluteExpiration != DateTime.MinValue)
            {
                _database.StringSet(key, value, absoluteExpiration - DateTime.Now);
            }
            else
            {
                _database.StringSet(key, value);
            }
        }
        /// <summary>
        /// 单个key
        /// </summary>
        /// <param name="key"></param>
        public void Remove(string key)
        {
            _database.KeyDelete(key);
        }
        /// <summary>
        /// 多个key模糊搜索到所有
        /// </summary>
        /// <param name="parament"></param>
        /// <returns></returns>
        public long RemoveKeys(string parament)
        {
            if (String.IsNullOrEmpty(parament))
                return long.MinValue;

            var mux = _database.Multiplexer;
            var server = mux.GetServer(mux.GetEndPoints()[0]);
            var keys = server.Keys(_database.Database, parament);
            return _database.KeyDelete(keys.ToArray());
        }
        #endregion

        #region 数据类型：列表

        public void LPushs(string key,object obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            _database.ListLeftPush(key,json);
        }
        public T LPops<T>(string key)
        {
            var json = _database.ListLeftPop(key);
            var obj = JsonConvert.DeserializeObject<T>(json);

            return obj;
        }
        #endregion
    }
}
