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
   public class SkolSred: ITimeProvider
    {
        long first, second, third = 0;
        long a, b, c = 0;
        long m = 0;
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
            Console.WriteLine(new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks((long)time + send + ts).ToString("dd.MM.yyyy hh:mm:ss:fffffff"));
            return new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks((long)time + send + ts);

        }
        public void Threading()
        {
            Thread thread = new Thread(new ThreadStart(hm));
            thread.Start();


        }
        public void hm()
        {
            forFirst();
            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(500);
                Thread thread4 = new Thread(new ThreadStart(method));
                thread4.Start();
                Thread.Sleep(500);
                Task<long>[] tasks = new Task<long>[]
                {
                    Task.Run(()=>v1()),
                    Task.Run(()=>v2()),
                    Task.Run(()=>v3()),
                };
                int id = Task.WaitAny(tasks);
                Console.WriteLine("meow");
                first = second;
                second = third;
                third = tasks[id].Result;
                Console.WriteLine("От сервера");
                Console.WriteLine(new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks(third).ToString("dd.MM.yyyy hh:mm:ss:fffffff"));
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
        public void forFirst()
        {
            Task<long>[] tasks = new Task<long>[]
                {
                    Task.Run(()=>v1()),
                    Task.Run(()=>v2()),
                    Task.Run(()=>v3()),
                };
            int id = Task.WaitAny(tasks);
            first = tasks[id].Result;
            Console.WriteLine("Сервер 1");
            Console.WriteLine(new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks(first).ToString("dd.MM.yyyy hh:mm:ss:fffffff"));
            Thread.Sleep(500);
            tasks = new Task<long>[]
               {
                    Task.Run(()=>v1()),
                    Task.Run(()=>v2()),
                    Task.Run(()=>v3()),
               };
            id = Task.WaitAny(tasks);
            second= tasks[id].Result;
            Console.WriteLine("Сервер 2");
            Console.WriteLine(new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks(second).ToString("dd.MM.yyyy hh:mm:ss:fffffff"));
            Thread.Sleep(500);
            tasks = new Task<long>[]
               {
                    Task.Run(()=>v1()),
                    Task.Run(()=>v2()),
                    Task.Run(()=>v3()),
               };
            id = Task.WaitAny(tasks);
            third = tasks[id].Result;
            Console.WriteLine("Сервер 3");
            Console.WriteLine(new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks(third).ToString("dd.MM.yyyy hh:mm:ss:fffffff"));

        }
        void method()
        {
            /*Console.WriteLine("Первое");
            Console.WriteLine(new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks(first).ToString("dd.MM.yyyy hh:mm:ss:fffffff"));
            Console.WriteLine("Второе");
            Console.WriteLine(new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks(second).ToString("dd.MM.yyyy hh:mm:ss:fffffff"));
            Console.WriteLine("Третье");
            Console.WriteLine(new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks(third).ToString("dd.MM.yyyy hh:mm:ss:fffffff"));
            */m = (first + second + third) / 3;
            first = second;
            second = third;
            third = m + (second - first) / 3;
            Console.WriteLine("по методу");
            Console.WriteLine(new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks(third).ToString("dd.MM.yyyy hh:mm:ss:fffffff"));
        }
        
    }
}
