using System;
using System.Collections.Generic;
using TimeProviderApi;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace AccurateTimeProviderLib
{
    public class LinExtrTimeProvider : ITimeProvider, IDisposable
    {

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly LinkedList<(long x, long y)> _timeData = new LinkedList<(long x, long y)>();
        private readonly NtpClient[] _clients;
        private readonly TimeSpan _interval;

        private Stopwatch _stopwatch = new Stopwatch();
        private Task _syncTask;
        private bool _disposed;

        public LinExtrTimeProvider(string[] serverNames,TimeSpan ts)
        {
            _interval = ts;
            _clients = new NtpClient[serverNames.Length];
            for (int i = 0; i < serverNames.Length; i++)
            {
                _clients[i] = new NtpClient(serverNames[i]);
            }
        }
        public DateTime Now => GetNow();
        public DateTime GetNow()
        {
            
                var timeTicks = LinEx(_stopwatch.ElapsedTicks, _timeData);
                var time = new DateTime((long)timeTicks, DateTimeKind.Utc);
                //Console.WriteLine($"твое время: {time:dd.MM.yyyy hh:mm:ss:fffffff}");
                return TimeZoneInfo.ConvertTimeFromUtc(time, TimeZoneInfo.Local);
            

        }
       
        public void StartSync()
        {
            if (_syncTask != null)
            {
                throw new InvalidOperationException("Sync already started");
            }

            CheckNotDisposed();
            _syncTask = Task.Run(() => SyncLoop(_cancellationTokenSource.Token));
        }

        private async Task SyncLoop(CancellationToken cancellationToken)
        {
            _stopwatch.Start();
            var i = 0;
            while(!cancellationToken.IsCancellationRequested)
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
            var tasks = new Task<DateTime>[_clients.Length];
            for (int i = 0; i < _clients.Length; i++)
            {
                tasks[i] = Task.Run(_clients[i].RequestTime);
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
        private void CheckNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(MnkTimeProvider));
            }
        }

     
        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _disposed = true;
            try
            {
                _syncTask?.GetAwaiter().GetResult();
            }
            catch (TaskCanceledException)
            {
                // не обрабатываем, потому что ожидаемое поведение
            }

            _cancellationTokenSource.Dispose();
        }
    }
    
}
