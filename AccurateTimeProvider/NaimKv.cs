using System;
using System.Collections.Generic;
using TimeProviderApi;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AccurateTimeProviderLib
{
    public class NaimKv : ITimeProvider
    {
        private readonly Queue<(long x, long y)> _timeData = new Queue<(long x, long y)>();
        private readonly NtpClient[] _serverNames = {new NtpClient("1.ru.pool.ntp.org"), new NtpClient("0.ru.pool.ntp.org"), new NtpClient("2.ru.pool.ntp.org") };
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private Tuple<double, double> _timeCoeffs;

        public DateTime Now()
        {
            var coeffs = _timeCoeffs;
            var timeTicks = coeffs.Item1 * _stopwatch.ElapsedTicks + coeffs.Item2;
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
            _stopwatch.Start();
            for (int i = 0; i < 100; i++)
            {
                await Sync();
                if (i > 9)
                {
                    _timeData.Dequeue();
                }
                var res = MNK.Mnk(_timeData);
                _timeCoeffs = new Tuple<double, double>(res.a, res.b);
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
            _timeData.Enqueue((_stopwatch.ElapsedTicks, task.Result.Ticks));
            Console.WriteLine("Сервер");
            Console.WriteLine(task.Result.ToString("dd.MM.yyyy hh:mm:ss:fffffff"));
        }

        
    }
    static class MNK
    {
        public static (double a, double b) Mnk(Queue<(long x, long y)> points)
        {
            var sumy = 0L;
            var sumx = 0L;
            var pr = 0L;
            var x2 = 0L;
            foreach (var pt in points)
            {
                sumx += pt.x;
                sumy += pt.y;
                pr += pt.y * pt.x;
                x2 += pt.x * pt.x;
            }
            var a = ((double)( points.Count* pr - (sumx * sumy)) / (points.Count * x2 - (sumx * sumx)));
            var b = ((((double)sumy) / points.Count) - ((double)(a * sumx) / points.Count));
            return (a, b);
        }
    }
}
