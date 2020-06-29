using System;
using System.Collections.Generic;
using TimeProviderApi;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AccurateTimeProviderLib
{
    public class LinExtrTimeProvider : ITimeProvider
    {
     
        private readonly LinkedList<(long x, long y)> _timeData = new LinkedList<(long x, long y)>();
        private readonly List<NtpClient> _servers = new List<NtpClient>();
        private Stopwatch _stopwatch = new Stopwatch();
        private readonly TimeSpan _interval = new TimeSpan(0, 0, 0, 0, 1000);
        public LinExtrTimeProvider(string[] serverNames)
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
                var timeTicks = LinEx(_stopwatch.ElapsedTicks, _timeData);
                var time = new DateTime((long)timeTicks, DateTimeKind.Utc);
                //Console.WriteLine($"твое время: {time:dd.MM.yyyy hh:mm:ss:fffffff}");
                return TimeZoneInfo.ConvertTimeFromUtc(time, TimeZoneInfo.Local);
            }

        }
       
        public Task StartSync()
        {
            return Task.Run(SyncLoop);
        }

        private async Task SyncLoop()
        {
            _stopwatch.Start();
            var i = 0;
            while(true)
            {
                
                await Sync();
                if (i > 1)
                {
                    _timeData.RemoveFirst();
                }
                await Task.Delay(_interval);
                i++;
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
            _timeData.AddLast((_stopwatch.ElapsedTicks, task.Result.Ticks));
            Console.WriteLine("Сервер");
            Console.WriteLine(task.Result.ToString("dd.MM.yyyy hh:mm:ss:fffffff"));
        }
    
        public double LinEx(long ts, LinkedList<(long x, long y)> points)
        {
            
            return points.First.Value.y + (ts - points.First.Value.x) * (points.Last.Value.y - points.First.Value.y) / (points.Last.Value.x - points.First.Value.x);
        }
    }
    
}
