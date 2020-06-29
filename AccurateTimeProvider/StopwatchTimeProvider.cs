using System;
using System.Collections.Generic;
using TimeProviderApi;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AccurateTimeProviderLib
{
    public class StopwatchTimeProvider : ITimeProvider

    {
        private readonly List<NtpClient> _servers = new List<NtpClient>();
        private readonly TimeSpan _interval = new TimeSpan(0, 0, 0, 0, 1000);
        private long _time = 0;
        private Stopwatch _stopwatch = new Stopwatch();
        public StopwatchTimeProvider(string[] serverNames)
        {
            foreach (string serverName in serverNames)
            {
                _servers.Add(new NtpClient(serverName));
            }
        }
        public DateTime Now
        {
            get
            {
                var timeTicks = _time + _stopwatch.ElapsedTicks;
                var time = new DateTime((long)timeTicks, DateTimeKind.Utc);
                return TimeZoneInfo.ConvertTimeFromUtc(time, TimeZoneInfo.Local); ;
            }

        }
        
        public Task StartSync()
        {
            return Task.Run(SyncLoop);
        }

        private async Task SyncLoop()
        {
            while(true)
            {
                await Sync();
                await Task.Delay(_interval);
            }
        }

        private async Task Sync()
        {
            var tasks = new List<Task<DateTime>>();
            foreach (NtpClient client in _servers)
            {
                tasks.Add(Task.Run(client.RequestTime));
            }
            var task = await Task.WhenAny(tasks);
            _time = task.Result.Ticks;
            _stopwatch.Restart();
            Console.WriteLine("Сервер");
            Console.WriteLine(task.Result.ToString("dd.MM.yyyy HH:mm:ss:fffffff"));
        }
    }
}
