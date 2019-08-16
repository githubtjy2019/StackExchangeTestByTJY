using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StackExchangeTest
{
    class SemaphoreWithThread
    {
    }

    #region autoresetevent-两个线程交替打印0~100的奇偶数
    public class ThreadExample
    {
        /// <summary>
        /// 两个线程交替打印0~100的奇偶数
        /// </summary>
        public static void PrintOddEvenNumber()
        {
            var work = new TheadWorkTest();
            var thread1 = new Thread(work.PrintOddNumer) { Name = "奇数线程" };
            var thread2 = new Thread(work.PrintEvenNumber) { Name = "偶数线程" };
            thread1.Start();
            thread2.Start();
        }
    }

    public class TheadWorkTest
    {
        //默认不给予信号
        private static readonly AutoResetEvent oddAre = new AutoResetEvent(false);
        private static readonly AutoResetEvent evenAre = new AutoResetEvent(false);

        public void PrintOddNumer()
        {
            oddAre.WaitOne();
            for (var i = 0; i < 10; i++)
            {
                if (i % 2 != 1) continue;
                
                Console.WriteLine($"{Thread.CurrentThread.Name}：{i}");
                
                evenAre.Set();//给予eventAre信号
                oddAre.WaitOne();//等待oddAre信号
                Thread.Sleep(10000);

            }
        }

        public void PrintEvenNumber()
        {
            for (var i = 0; i < 10; i++)
            {
                if (i % 2 != 0) continue;
                Console.WriteLine($"{Thread.CurrentThread.Name}：{i}");
                
                oddAre.Set();//给予eventAre信号
                evenAre.WaitOne();// 等待eventAre信号
                Thread.Sleep(1000);
            }
        }
    }

    #endregion
    #region semaphore-通过N个线程顺序循环打印0~100

    public class ProgramF
    {
        static Semaphore sema = new Semaphore(0, 1);
        const int cycleNum = 9;
        public static void MainF()
        {
            for (int i = 0; i < cycleNum; i++)
            {
                Thread td = new Thread(new ParameterizedThreadStart(testFun));
                td.Name = string.Format("编号{0}", i.ToString());
                td.Start(td.Name);
            }
            Console.ReadKey();
        }
        public static void testFun(object obj)
        {
            sema.WaitOne();
            Console.WriteLine(obj.ToString() + "进洗手间：" + DateTime.Now.ToString());
            Thread.Sleep(1000);
            Console.WriteLine(obj.ToString() + "出洗手间：" + DateTime.Now.ToString());
            sema.Release();
        }
    }

    public class ThreadExamplea
    {

        /// <summary>
        /// N个线程顺序循环打印从0至100
        /// </summary>
        /// <param name="n"></param>
        public static void PrintNumber(int n = 3)
        {
            var work = new TheadWorkTesta { Semaphores = new Semaphore[n] };
            for (var i = 0; i < n; i++)
            {
                work.Semaphores[i] = new Semaphore(1, 1);
                if (i != n - 1)
                    work.Semaphores[i].WaitOne();
            }
            for (var i = 0; i < n; i++)
            {
                new Thread(work.PrintNumber) { Name = "线程" + i }.Start(i);
            }
        }
    }

    public class TheadWorkTesta
    {
        public Semaphore[] Semaphores { get; set; }
        public static int index;
        public void PrintNumber(object c)
        {
            var i = Convert.ToInt32(c);
            var preSemaphore = i == 0 ? Semaphores[Semaphores.Length - 1] : Semaphores[i - 1];
            var curSemaphore = Semaphores[i];
            while (true)
            {
                preSemaphore.WaitOne();
                Interlocked.Increment(ref index);
                if (index > 99)
                    return;
                Console.WriteLine($"{Thread.CurrentThread.Name}：{index}");                
                curSemaphore.Release();

                Thread.Sleep(1000);
            }
        }
    }

    #endregion
}
