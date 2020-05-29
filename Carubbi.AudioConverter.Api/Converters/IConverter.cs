using System.Threading.Tasks;

namespace Carubbi.AudioConverter.Api.Converters
{
    public interface IConverter
    {
        string From { get; }
        string To { get;   }
        Task<byte[]> ConvertAsync(byte[] content);
    }
}
