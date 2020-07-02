using System;
using System.Collections.Generic;
using TimeProviderApi;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace AccurateTimeProviderLib
{ 
    public class ExpSglazhTimeProvider: ITimeProvider,IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly NtpClient[] _clients;
        private readonly TimeSpan _interval;

        private Stopwatch _stopwatch = new Stopwatch();
        private Tuple<double, double> _firstParams;
        private long _time;
        private long _ts;
        private Task _syncTask;
        private bool _disposed;

        public ExpSglazhTimeProvider(string[] serverNames,TimeSpan ts)
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
           
                var coeffs = _firstParams;
                var timeTicks = coeffs.Item1 * _stopwatch.ElapsedTicks / _ts + coeffs.Item2;
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
            
            await FirstSync();
            await Task.Delay(_interval);
            while(!cancellationToken.IsCancellationRequested)
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
            foreach (NtpClient client in _clients)
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
            foreach (NtpClient client in _clients)
            {
                tasks.Add(Task.Run(client.RequestTime));
            }
            var task = await Task.WhenAny(tasks);
            _ts = _stopwatch.ElapsedTicks;
            _firstParams = new Tuple<double, double>(1, task.Result.Ticks);
            Console.WriteLine("Сервер");
            Console.WriteLine(task.Result.ToString("dd.MM.yyyy hh:mm:ss:fffffff"));
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


