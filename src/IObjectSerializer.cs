using System.Threading.Tasks;

namespace JsonStore
{
    public interface IObjectSerializer<T>
    {
        Task<T> DeserializeAsync(System.IO.Stream inStream);

        Task SerializeAsync(System.IO.Stream outStream, object obj);
    }
}
