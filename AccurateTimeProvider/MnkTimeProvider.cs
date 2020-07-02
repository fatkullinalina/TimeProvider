using System;
using System.Collections.Generic;
using TimeProviderApi;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace AccurateTimeProviderLib
{
    //TODO: Старайся группировать переменные и поля в классе. Общепринятая конвенция - сначала константы, потом неизменяемые поля, потом изменяемые поля и потом свойства. Тут я уже сделал.
    public class MnkTimeProvider : ITimeProvider, IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly Queue<(long x, long y)> _timeData = new Queue<(long x, long y)>();
        private readonly NtpClient[] _clients;
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private readonly TimeSpan _interval;
        private readonly int _queueLength;

        private Tuple<double, double> _timeCoeffs;
        private Task _syncTask;
        private bool _disposed;

        public DateTime Now => GetNow();

        public MnkTimeProvider(string[] serverNames,int amount,TimeSpan ts)
        {
            _interval = ts;
            _queueLength = amount;
            _clients = new NtpClient[serverNames.Length];
            for(int i=0;i<serverNames.Length;i++)
            {
                _clients[i]=new NtpClient(serverNames[i]);
            }
            
        }

        private DateTime GetNow()
        {
            CheckNotDisposed();
            var coeffs = _timeCoeffs;
            var timeTicks = coeffs.Item1 * _stopwatch.ElapsedTicks + coeffs.Item2;
            var time = new DateTime((long)timeTicks, DateTimeKind.Unspecified);
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
            if (!cancellationToken.IsCancellationRequested)
            {
                _stopwatch.Start();
                _timeData.Enqueue((_stopwatch.ElapsedTicks, DateTime.UtcNow.Ticks));
                _timeData.Enqueue((_stopwatch.ElapsedTicks, DateTime.UtcNow.Ticks));
                var res = Mnk.CountCoef(_timeData);
                _timeCoeffs = new Tuple<double, double>(res.a, res.b);
                var i = 0;
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Sync();
                    if (i >= _queueLength)
                    {
                        _timeData.Dequeue();
                    }
                    res = Mnk.CountCoef(_timeData);
                    _timeCoeffs = new Tuple<double, double>(res.a, res.b);

                    await Task.Delay(_interval, cancellationToken);
                    i++;
                }
            }
        }

        private async Task Sync()
        {
            var tasks = new Task<DateTime>[_clients.Length];
            for(int i=0;i<_clients.Length;i++)
            {
                tasks[i]=Task.Run(_clients[i].RequestTime);
            }
            var task = await Task.WhenAny(tasks);
            _timeData.Enqueue((_stopwatch.ElapsedTicks, task.Result.Ticks));
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
