using System.Threading.Tasks;

namespace Money.UI.Services
{
    public interface IObjectSerializer<T>
    {
        Task<T> DeserializeAsync(System.IO.Stream inStream);

        Task SerializeAsync(System.IO.Stream outStream, object obj);
    }
}
