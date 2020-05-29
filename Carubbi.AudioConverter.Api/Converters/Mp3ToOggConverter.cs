using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Carubbi.AudioConverter.Api.Converters
{
    public class Mp3ToOggConverter : IConverter
    {
        private readonly IServiceProvider _serviceProvider;
        
        public Mp3ToOggConverter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            
        }

        public string From => "mp3";
        public string To => "ogg";

        public async Task<byte[]> ConvertAsync(byte[] content)
        {
            var selector = _serviceProvider.GetService<IConverterSelector>();
            var wavToOggConverter = selector.Select("wav", "ogg");
            var mp3ToWavConverter = selector.Select("mp3", "wav");
            return (await wavToOggConverter.ConvertAsync(await mp3ToWavConverter.ConvertAsync(content)));
        }
    }
}
