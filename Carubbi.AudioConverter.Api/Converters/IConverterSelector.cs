namespace Carubbi.AudioConverter.Api.Converters
{
    public interface IConverterSelector
    {
        IConverter Select(string @from, string to);
    }
}
