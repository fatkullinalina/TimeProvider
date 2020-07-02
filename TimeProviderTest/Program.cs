using System;
using System.Threading.Tasks;
using AccurateTimeProviderLib;
using TimeProviderApi;

namespace TimeProviderTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("старт");
            string[] names = { "1.ru.pool.ntp.org", "0.ru.pool.ntp.org", "2.ru.pool.ntp.org" };
            int amount = 10;
            TimeSpan ts = TimeSpan.FromSeconds(1);
           // using var clock = new StopwatchTimeProvider(names,ts);
           // using var clock = new LinExtrTimeProvider(names,ts);
           using var  clock = new ExpSglazhTimeProvider(names,ts);
            //using var clock = new MnkTimeProvider(names,amount, ts);
            clock.StartSync();
            await test(clock);
        }

        static async Task test(ITimeProvider clock)
        {
            
            await Task.Delay(3000);
            for (int i = 0; i < 20; i++)
            {
                Console.WriteLine("Прошу время");
                await Task.Delay(500);
                Console.WriteLine("твое время");
                Console.WriteLine(clock.Now.ToString("dd.MM.yyyy HH:mm:ss:fffffff"));
            }
        }

    }

}
