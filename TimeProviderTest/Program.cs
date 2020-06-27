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
            //TimerSNTP clock = new TimerSNTP();
            //LinExtr clock = new LinExtr();
            //ExponenSglaz clock = new ExponenSglaz();
            NaimKv clock = new NaimKv();
            var task = clock.StartSync();

            async void test()
            {
                await Task.Delay(3000);
                for (int i = 0; i < 20; i++)
                {
                    Console.WriteLine("Прошу время");
                    await Task.Delay(500);
                    Console.WriteLine("твое время");
                    Console.WriteLine(clock.Now().ToString("dd.MM.yyyy hh:mm:ss:fffffff"));
                }
            }

            test();

            task.Wait();
        }

    }

}
