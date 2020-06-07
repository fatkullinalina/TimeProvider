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
    public class LinExtr : ITimeProvider
    {
        long x1, x2, y1, y2 = 0;
        double answer = 0.0;
        Server serverFirst = new Server("time-a.nist.gov");
        Server serverSecond = new Server("ru.pool.ntp.org");
        Server serverThird = new Server("time.windows.com");
        Stopwatch stopwatch1000 = new Stopwatch();
        Stopwatch stopwatchRequest = new Stopwatch();
        public DateTime Now()
        {

            long ts = stopwatchRequest.ElapsedTicks;
            Console.WriteLine("твое время:");
            
            // Console.WriteLine(new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks((long)Math.Round(second)).ToString("dd.MM.yyyy hh:mm:ss:fffffff"));
            return new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks((long)Math.Round(method(ts)));

        }
        public void threading()
        {
            stopwatch1000.Start();
            Thread thread = new Thread(new ThreadStart(hm));
            thread.Start();
        }
        public void hm()
        {
            requestFirst();
            Thread.Sleep(1000);
            requestSecond();
            for(int i = 0; i < 20; i++)
            {
                Thread.Sleep(1000);
                requestNext();
            }


        }
        long v1()
        {
            serverFirst.SNTP();
            return serverFirst.neededtime;
        }
        long v2()
        {
            serverSecond.SNTP();
            return serverSecond.neededtime;
        }
        long v3()
        {
            serverThird.SNTP();
            return serverThird.neededtime;
        }
        public void requestFirst()
        {
            Task<long>[] tasks = new Task<long>[]
            {
                    Task.Run(()=>v1()),
                    Task.Run(()=>v2()),
                    Task.Run(()=>v3()),
            };
            int id = Task.WaitAny(tasks);
            x1 = stopwatch1000.ElapsedTicks;
            y1 = tasks[id].Result;

        }
        public void requestSecond()
        {
            Task<long>[] tasks = new Task<long>[]
            {
                    Task.Run(()=>v1()),
                    Task.Run(()=>v2()),
                    Task.Run(()=>v3()),
            };
            int id = Task.WaitAny(tasks);
            x2 = stopwatch1000.ElapsedTicks;
            y2 = tasks[id].Result;
           /* Console.WriteLine("firstx");
            Console.WriteLine(x1);
            Console.WriteLine("firsty");
            Console.WriteLine(y1);*/
            //Console.WriteLine(new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks(y1).ToString("dd.MM.yyyy hh:mm:ss:fffffff"));
            /*Console.WriteLine("secondx");
            Console.WriteLine(x2);
            Console.WriteLine("secondy");
            Console.WriteLine(y2);*/
            Console.WriteLine(new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks(y2).ToString("dd.MM.yyyy hh:mm:ss:fffffff"));
        }
        public void requestNext()
        {
            
            Task<long>[] tasks = new Task<long>[]
            {
                    Task.Run(()=>v1()),
                    Task.Run(()=>v2()),
                    Task.Run(()=>v3()),
            };
            int id = Task.WaitAny(tasks);
            stopwatchRequest.Restart();
            x1 = x2;
            y1 = y2;
            x2 = stopwatch1000.ElapsedTicks;
            y2 = tasks[id].Result;
            /*Console.WriteLine("x2");
            Console.WriteLine(x2);
            Console.WriteLine("y2");*/
            Console.WriteLine("Сервер");
            Console.WriteLine(new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks(y2).ToString("dd.MM.yyyy hh:mm:ss:fffffff"));
        }
        public double method(long ts)
        {
            answer = y1 + (ts + x2 - x1) * (y2 - y1) / (x2 - x1);
            return answer;
        }
    }
    
}
