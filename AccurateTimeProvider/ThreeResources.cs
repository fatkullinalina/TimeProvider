using System;
using System.Collections.Generic;
using System.Text;
using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using TimeProviderApi;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace AccurateTimeProviderLib
{
    public class ThreeResources : ITimeProvider
    {
        long a = 0;
        ulong time = 0;
        long send = 0;
        Server serverFirst = new Server("time-a.nist.gov");
        Server serverSecond = new Server("ru.pool.ntp.org");
        Server serverThird = new Server("time.windows.com");
        Stopwatch stopwatch = new Stopwatch();
        public DateTime Now()
        {
            
            long ts = stopwatch.ElapsedTicks;
            Console.WriteLine("твое время:");
            Console.WriteLine(new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks(a+ ts).ToString("dd.MM.yyyy hh:mm:ss:fffffff"));
            return new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks(a + ts);

        }
        
        long v1()
        {
            serverFirst.SNTP();
            //Console.WriteLine(new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks(serverFirst.neededtime).ToString("dd.MM.yyyy hh:mm:ss:fffffff"));
            return serverFirst.neededtime;
        }
        long v2()
        {
            serverSecond.SNTP();
            //Console.WriteLine(new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks(serverSecond.neededtime).ToString("dd.MM.yyyy hh:mm:ss:fffffff"));
            return serverSecond.neededtime;
        }
        long v3()
        {
            serverThird.SNTP();
            //Console.WriteLine(new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks(serverThird.neededtime).ToString("dd.MM.yyyy hh:mm:ss:fffffff"));
            return serverThird.neededtime;
        }
        public void hm()
        {
            for (int i = 0; i < 10; i++)
            {
                Thread thread = new Thread(new ThreadStart(stat));
                //Console.WriteLine("начал");
                thread.Start();
                Thread.Sleep(1000);
                stopwatch.Restart();
            }
        }
        public void stat()
        {
            //Console.WriteLine("stat тоже начал");
            //for (int i = 0; i < 10; i++)
           // {
                Task<long>[] tasks = new Task<long>[]
                {
                    Task.Run(()=>v1()),
                    Task.Run(()=>v2()),
                    Task.Run(()=>v1()),
                };
                int id=Task.WaitAny(tasks);
                //Console.WriteLine(id);
                a = tasks[id].Result;
                //Console.WriteLine(new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks(a).ToString("dd.MM.yyyy hh:mm:ss:fffffff"));
            //stopwatch.Restart();
            //Thread.Sleep(1000);
            // }
        }
        //
    }
}
