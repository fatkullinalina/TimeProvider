using System;
using TimeProviderApi;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AccurateTimeProviderLib
{
    public class TimerSNTP : ITimeProvider

    {
        private readonly NtpClient[] _serverNames = { new NtpClient("1.ru.pool.ntp.org"), new NtpClient("0.ru.pool.ntp.org"), new NtpClient("2.ru.pool.ntp.org") };
        private long _time = 0;
        private Stopwatch _stopwatch = new Stopwatch();
       
        public DateTime Now()
        {

            var timeTicks = _time + _stopwatch.ElapsedTicks;
            var time = new DateTime((long)timeTicks, DateTimeKind.Utc);
            //Console.WriteLine($"твое время: {time:dd.MM.yyyy hh:mm:ss:fffffff}");
            return time;
        }
        public Task StartSync()
        {
            return Task.Run(SyncLoop);
        }

        private async Task SyncLoop()
        {
            for (int i = 0; i < 100; i++)
            {
                await Sync();
                await Task.Delay(1000);
            }
        }

        private async Task Sync()
        {
            var tasks = new[]
            {
                Task.Run(_serverNames[0].RequestTime),
                Task.Run(_serverNames[1].RequestTime),
                Task.Run(_serverNames[2].RequestTime)
            };
            var task = await Task.WhenAny(tasks);
            _time = task.Result.Ticks;
            _stopwatch.Restart();
            Console.WriteLine("Сервер");
            Console.WriteLine(task.Result.ToString("dd.MM.yyyy hh:mm:ss:fffffff"));
        }
    }
}
