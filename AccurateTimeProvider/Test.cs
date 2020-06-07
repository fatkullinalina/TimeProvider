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
    public class Test
    {
        long server, systemFirst, systemSecond, ts = 0;
        Server serverFirst = new Server("time-a.nist.gov");
        Server serverSecond = new Server("ru.pool.ntp.org");
        Server serverThird = new Server("time.windows.com");
        Stopwatch stopwatch = new Stopwatch();

        long v3()
        {
            serverSecond.SNTP();
            stopwatch.Restart();
            return serverSecond.neededtime;
        }
        long v4()
        {
            serverSecond.SNTP();
            ts = stopwatch.ElapsedTicks;
            return serverSecond.neededtime;
        }

        public void stat()
        {
            for (int i = 0; i < 11; i++)
            {
                Console.WriteLine(i);
                Task<long>[] tasks = new Task<long>[]
                {
                    Task.Run(()=>v3()),
                };
                int id = Task.WaitAny(tasks);
                systemFirst = tasks[id].Result;
                Thread.Sleep(100);
                Task<long>[] masks = new Task<long>[]
               {
                    Task.Run(()=>v4()),
               };
                int id2 = Task.WaitAny(masks);
                server = masks[id2].Result;
                systemSecond = systemFirst + ts;
                Console.WriteLine(server - systemSecond);
                Console.WriteLine(ts);
                Console.WriteLine(new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks(server).ToString("dd.MM.yyyy hh:mm:ss:fffffff"));
        }
        }
    }
}
