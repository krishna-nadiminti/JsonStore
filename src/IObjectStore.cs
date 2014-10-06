using System.Threading.Tasks;

namespace Money.DataAccess
{
    public interface IReadOnlyStore<T>
    {
        Task<T> LoadAsync();
    }

    public interface IObjectStore<T> : IReadOnlyStore<T>
    {
        Task<bool> SaveAsync(T objectGraph);
    }

}