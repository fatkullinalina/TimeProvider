using System;
using System.Collections.Generic;
using TimeProviderApi;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace AccurateTimeProviderLib
{
    public class StopwatchTimeProvider : ITimeProvider, IDisposable

    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly NtpClient[] _clients;
        private readonly TimeSpan _interval;

        private long _time = 0;
        private Stopwatch _stopwatch = new Stopwatch();
        private bool _disposed;
        private Task _syncTask;

        public StopwatchTimeProvider(string[] serverNames,TimeSpan ts)
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

            CheckNotDisposed();
            var timeTicks = _time + _stopwatch.ElapsedTicks;
                var time = new DateTime((long)timeTicks, DateTimeKind.Utc);
                return TimeZoneInfo.ConvertTimeFromUtc(time, TimeZoneInfo.Local); ;
            

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
            while(!cancellationToken.IsCancellationRequested)
            {
                await Sync();
                await Task.Delay(_interval);
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
            _time = task.Result.Ticks;
            _stopwatch.Restart();
            Console.WriteLine("Сервер");
            Console.WriteLine(task.Result.ToString("dd.MM.yyyy HH:mm:ss:fffffff"));
        }
        private void CheckNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(MnkTimeProvider));
            }
        }

        //TODO: в .Net есть специальный интерфейс IDisposable. Вызов метода Dispose обзначает, что этот объект больше не нужен.
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
