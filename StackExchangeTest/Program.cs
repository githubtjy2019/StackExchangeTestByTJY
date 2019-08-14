using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackExchangeTest
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Student> stus = new List<Student>();
            for (int i = 0;i < 100000;i++)
            {
                stus.Add(new Student
                {
                    name = "m" + i,
                    age = "age"+i,
                    gas = i,
                    sds =i,
                    adsf=i
                });
            }
            var json = JsonConvert.SerializeObject(stus);
            var count = Encoding.Default.GetByteCount(json);
            #region string
            //RedisHelper.Add("TJY001",stus);

            //var res = RedisHelper.GetDatas<List<Student>>("TJY001");
            #endregion

            RedisHelper.LAdd("TJY002",stus);
            var resS = RedisHelper.LGetDatas<List<Student>>("TJY002");
        }
    }

    public class Student
    {
        public string name { get; set; }
        public string age { get; set; }
        public int gas { get; set; }
        public int adsf { get; set; }
        public int sds { get; set; }
    }
}
