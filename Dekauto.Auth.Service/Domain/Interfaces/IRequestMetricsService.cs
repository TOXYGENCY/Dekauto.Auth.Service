using System.Collections.Concurrent;

namespace Dekauto.Auth.Service.Domain.Interfaces
{
    public interface IRequestMetricsService
    {
        void Increment();
        List<int> GetRecentCounters();
    }
}
