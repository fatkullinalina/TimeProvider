using System;
using System.Collections.Generic;
using TimeProviderApi;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AccurateTimeProviderLib
{ 
    public class ExpSglazhTimeProvider: ITimeProvider
    {

        private readonly List<NtpClient> _servers = new List<NtpClient>();
        Stopwatch _stopwatch = new Stopwatch();
        private Tuple<double, double> _firstParams;
        private long _time;
        private readonly TimeSpan _interval = new TimeSpan(0, 0, 0, 0, 1000);
        private long _ts;

        public ExpSglazhTimeProvider(string[] serverNames)
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
                var coeffs = _firstParams;
                var timeTicks = coeffs.Item1 * _stopwatch.ElapsedTicks / _ts + coeffs.Item2;
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
            
            await FirstSync();
            await Task.Delay(_interval);
            while(true)
            {
                
                await Sync();
                var res = ExpSglazh.CountCoef(_time,_firstParams);
                _firstParams = new Tuple<double, double>(res.a, res.b);
                _ts = _stopwatch.ElapsedTicks;
                _stopwatch.Restart();
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
            Console.WriteLine("Сервер");
            Console.WriteLine(task.Result.ToString("dd.MM.yyyy hh:mm:ss:fffffff"));
        }
        private async Task FirstSync()
        {
            _stopwatch.Start();
            var tasks = new List<Task<DateTime>>();
            foreach (NtpClient client in _servers)
            {
                tasks.Add(Task.Run(client.RequestTime));
            }
            var task = await Task.WhenAny(tasks);
            _ts = _stopwatch.ElapsedTicks;
            _firstParams = new Tuple<double, double>(1, task.Result.Ticks);
            Console.WriteLine("Сервер");
            Console.WriteLine(task.Result.ToString("dd.MM.yyyy hh:mm:ss:fffffff"));
        }

    }

    
}


