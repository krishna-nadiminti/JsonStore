using System.Threading.Tasks;

namespace JsonStore.DataAcces
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