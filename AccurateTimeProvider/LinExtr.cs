﻿using System;
using System.Collections.Generic;
using TimeProviderApi;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AccurateTimeProviderLib
{
    public class LinExtr : ITimeProvider
    {
     
        private readonly LinkedList<(long x, long y)> _timeData = new LinkedList<(long x, long y)>();
        private readonly NtpClient[] _serverNames = { new NtpClient("1.ru.pool.ntp.org"), new NtpClient("0.ru.pool.ntp.org"), new NtpClient("2.ru.pool.ntp.org") };
        private Stopwatch _stopwatch = new Stopwatch();

        public DateTime Now()
        {
            var timeTicks = LinEx(_stopwatch.ElapsedTicks,_timeData);
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
                if (i > 1)
                {
                    _timeData.RemoveFirst();
                }
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
