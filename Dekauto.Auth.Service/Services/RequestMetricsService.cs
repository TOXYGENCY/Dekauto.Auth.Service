using Dekauto.Auth.Service.Domain.Interfaces;
using System.Collections.Concurrent;
using System.Numerics;

namespace Dekauto.Auth.Service.Services
{
    public class RequestMetricsService : IRequestMetricsService
    {
        private Timer counterTimer;
        private ConcurrentQueue<int> counterQueue; // Потокобезопасная очередь
        private int maxPrevCounters = 5; // Максимум значений в хранении
        private int currentCounter = 0;
        private int period = 0;

        public RequestMetricsService(IConfiguration config)
        {
            period = config.GetValue<int?>("Metrics:RequestCounter:Seconds") ?? 60;

            if (this.counterQueue == null) this.counterQueue = new ConcurrentQueue<int>();

            counterTimer = MakeCounterTimer();
        }

        // Создание таймера
        private Timer MakeCounterTimer()
        {
            return new Timer(SaveAndResetCounter, null, 0, (int)TimeSpan.FromSeconds(period).TotalMilliseconds);
        }

        private void SaveAndResetCounter(object? state)
        {
            // Если уже много значений, то убираем последнее
            if (this.counterQueue.Count >= maxPrevCounters)
            {
                counterQueue.TryDequeue(out _);
            }

            // Записываем собранное значение в очередь
            counterQueue.Enqueue(currentCounter);

            // Сбрасываем счетчикы
            Interlocked.Exchange(ref currentCounter, 0);
        }

        public void Increment()
        {
            Interlocked.Increment(ref currentCounter);
        }

        public List<int> GetRecentCounters()
        {
            return counterQueue.ToList();
        }
    }
}
