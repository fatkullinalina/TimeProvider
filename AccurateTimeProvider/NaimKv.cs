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
    public class NaimKv : ITimeProvider
    {
    double first, second, third = 0;
    List<long> x = new List<long>();
    List<long> y = new List<long>();
    long x2,pr,sumy,sumx,d= 0;
        double a, b = 0.0;
        int m = 0;
    ulong time = 0;
    long send = 0;
    Server serverFirst = new Server("time-a.nist.gov");
    Server serverSecond = new Server("pool.ntp.org");
    Server serverThird = new Server("time.windows.com");
    Stopwatch stopwatch = new Stopwatch();
    Stopwatch stopwatch1000 = new Stopwatch();
        public DateTime Now()
    {

            second = method(stopwatch1000.ElapsedTicks);
        Console.WriteLine("твое время:");
       // Console.WriteLine(new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks((long)Math.Round(second)).ToString("dd.MM.yyyy hh:mm:ss:fffffff"));
        return new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks((long)Math.Round(second));

    }
        public void threading()
        {
            Thread thread = new Thread(new ThreadStart(hm));
            thread.Start();


        }
        public void hm()
        {
            stopwatch1000.Start();
            for (int i = 0; i < 10; i++)
            {

                //Thread thread = new Thread(new ThreadStart(forFirst));
                //thread.Start();
                forFirst();
                Thread.Sleep(1000);
            }
            Console.WriteLine("Набрали статистику");
            for (int i = 0; i < 20; i++)
            {
                Thread thread = new Thread(new ThreadStart(forSecond));
                thread.Start();
                //forSecond();
                Thread.Sleep(1000);

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
            x.Add(stopwatch1000.ElapsedTicks);
            y.Add(tasks[id].Result);
            Console.WriteLine("Сервер");
            Console.WriteLine(new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks(tasks[id].Result).ToString("dd.MM.yyyy hh:mm:ss:fffffff"));



        }
        public void forSecond()
        {
            
                Task<long>[] tasks = new Task<long>[]
                {
                    Task.Run(()=>v1()),
                    Task.Run(()=>v2()),
                    Task.Run(()=>v3()),
                };
                int id = Task.WaitAny(tasks);
            //method(stopwatch1000.ElapsedTicks);
 
              
                x.Add(stopwatch1000.ElapsedTicks);
                y.Add(tasks[id].Result);
            x.RemoveAt(0);
            y.RemoveAt(0);
            //foreach (long i in y)
            //Console.WriteLine(i);

            stopwatch.Restart();
                Console.WriteLine("Сервер");
                Console.WriteLine(new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks(tasks[id].Result).ToString("dd.MM.yyyy hh:mm:ss:fffffff"));



        }
        double method(long d)
        {
            a= 0;
            b= 0;
            sumy = 0;
            sumx = 0;
            sumy = 0;
            pr = 0;
            x2 = 0;
            for(int i = 0; i < 10; i++)
            {
                sumx += x[i];
                sumy += y[i];
                pr += y[i] * x[i];
                x2 += x[i]*x[i];
                //Console.WriteLine(i);
                //Console.WriteLine(x[i]);
                //Console.WriteLine(y[i]);


            }

             a = ((double)(10*pr - (sumx * sumy)) / (10 * x2 - (sumx * sumx)));
             b =((((double)sumy) / 10) - ((double)(a * sumx) / 10));
            Console.WriteLine(a);
            Console.WriteLine(b);
            Console.WriteLine(d);

            first = b + a * d;
            Console.WriteLine(first);
            //Console.WriteLine("Метод");
            //Console.WriteLine(new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks((long)Math.Round(first)).ToString("dd.MM.yyyy hh:mm:ss:fffffff"));
            return first;
        }


    }
}
