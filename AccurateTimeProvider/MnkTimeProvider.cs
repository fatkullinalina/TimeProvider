using System;
using System.Collections.Generic;
using TimeProviderApi;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AccurateTimeProviderLib
{

    public class MnkTimeProvider : ITimeProvider
    {
        private readonly Queue<(long x, long y)> _timeData = new Queue<(long x, long y)>();
        private readonly List<NtpClient> _servers = new List<NtpClient>();
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private Tuple<double, double> _timeCoeffs;
        private readonly TimeSpan _interval = new TimeSpan(0, 0, 0, 0, 1000);
        private readonly int _amount = 9;
        public DateTime Now
        {
            get
            {
                var coeffs = _timeCoeffs;
                var timeTicks = coeffs.Item1 * _stopwatch.ElapsedTicks + coeffs.Item2;
                var time = new DateTime((long)timeTicks, DateTimeKind.Unspecified);
                //Console.WriteLine($"твое время: {time:dd.MM.yyyy hh:mm:ss:fffffff}");
                return TimeZoneInfo.ConvertTimeFromUtc(time, TimeZoneInfo.Local);
            }
            
        }

        public MnkTimeProvider(string[] serverNames)
        {
            foreach(string serverName in serverNames)
            {
                _servers.Add(new NtpClient(serverName));
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
                if (i > _amount)
                {
                    _timeData.Dequeue();
                }
                var res = Mnk.CountCoef(_timeData);
                _timeCoeffs = new Tuple<double, double>(res.a, res.b);
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
            _timeData.Enqueue((_stopwatch.ElapsedTicks, task.Result.Ticks));

            Console.WriteLine("Сервер");
            Console.WriteLine(task.Result.ToString("dd.MM.yyyy hh:mm:ss:fffffff"));
        }
    }
}
