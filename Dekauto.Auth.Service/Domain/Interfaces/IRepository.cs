namespace Dekauto.Auth.Service.Domain.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();

        Task<T> GetByIdAsync(Guid id);

        Task AddAsync(T obj);

        Task UpdateAsync(T obj);

        Task DeleteAsync(Guid id);
    }
}
