using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace AccurateTimeProviderLib
{ 
    public class ExponenSglaz
    {
        
        private readonly NtpClient[] _serverNames = { new NtpClient("1.ru.pool.ntp.org"), new NtpClient("0.ru.pool.ntp.org"), new NtpClient("2.ru.pool.ntp.org") };
        Stopwatch _stopwatch = new Stopwatch();
        private Tuple<double, double> _firstParams;
        private long _time;
        private long _interval;
        public DateTime Now()
        {

            var coeffs = _firstParams;
            var timeTicks = coeffs.Item1 * _stopwatch.ElapsedTicks/_interval+ coeffs.Item2;
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
            
            await FirstSync();
            await Task.Delay(1000);
            for (int i = 0; i < 100; i++)
            {
                
                await Sync();
                var res = Exp.ExpSglazh(_time,_firstParams);
                _firstParams = new Tuple<double, double>(res.a, res.b);
                _interval = _stopwatch.ElapsedTicks;
                _stopwatch.Restart();
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
            Console.WriteLine("Сервер");
            Console.WriteLine(task.Result.ToString("dd.MM.yyyy hh:mm:ss:fffffff"));
        }
        private async Task FirstSync()
        {
            _stopwatch.Start();
            var tasks = new[]
            {
                Task.Run(_serverNames[0].RequestTime),
                Task.Run(_serverNames[1].RequestTime),
                Task.Run(_serverNames[2].RequestTime)
            };

            var task = await Task.WhenAny(tasks);
            _interval = _stopwatch.ElapsedTicks;
            _firstParams = new Tuple<double, double>(1, task.Result.Ticks);
            Console.WriteLine("Сервер");
            Console.WriteLine(task.Result.ToString("dd.MM.yyyy hh:mm:ss:fffffff"));
        }

    }

    static class Exp
    {
        public static (double a, double b) ExpSglazh(long time, Tuple<double, double> firstParams)
        {

            var alpha = 1;
            var beta = 1;

            var b = alpha * time + (1 - alpha) * (firstParams.Item2 - firstParams.Item1);
            var a = beta * (b - firstParams.Item2) + (1 - beta) * firstParams.Item1;

            return (a, b);
        }
    }
}


