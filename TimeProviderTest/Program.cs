using System;
using System.Threading.Tasks;
using AccurateTimeProviderLib;

namespace TimeProviderTest
{
    class Program
    {

        static void Main(string[] args)

        {
            Console.WriteLine("старт");
            string[] names = { "1.ru.pool.ntp.org", "0.ru.pool.ntp.org", "2.ru.pool.ntp.org" };
            //StopwatchTimeProvider clock = new StopwatchTimeProvider(names);
            //LinExtrTimeProvider clock = new LinExtrTimeProvider(names);
            ExpSglazhTimeProvider clock = new ExpSglazhTimeProvider(names);
            //MnkTimeProvider clock = new MnkTimeProvider(names);
            var task = clock.StartSync();

            async void test()
            {
                Console.WriteLine(TimeZoneInfo.Local);
                Console.WriteLine(TimeZoneInfo.Utc);
                await Task.Delay(3000);
                for (int i = 0; i < 20; i++)
                {
                    Console.WriteLine("Прошу время");
                    await Task.Delay(500);
                    Console.WriteLine("твое время");
                    Console.WriteLine(clock.Now.ToString("dd.MM.yyyy HH:mm:ss:fffffff"));
                }
            }

            test();

            task.Wait();
        }

    }

}
