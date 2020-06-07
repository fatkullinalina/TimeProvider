using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using AccurateTimeProviderLib;
using TimeProviderApi;

namespace TimeProviderTest
{
    class Program
    {

        static void Main(string[] args)

        {
            Console.WriteLine("старт");
            //TimerSNTP clock = new TimerSNTP();
            //LinExtr clock = new LinExtr();
            NaimKv clock = new NaimKv();
            clock.threading();
            async void test()
            {
                await Task.Delay(15000);
                for (int i = 0; i < 10; i++)
                {
                    Console.WriteLine("Прошу время");
                    await Task.Delay(500);
                    Console.WriteLine(clock.Now().ToString("dd.MM.yyyy hh:mm:ss:fffffff"));
                }
            }
            test();
        }

    }    
        
}
