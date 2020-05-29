using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Carubbi.AudioConverter.Api.Converters
{
    public class OggToMp3Converter : IConverter
    {
        private readonly IServiceProvider _serviceProvider;
        
        public OggToMp3Converter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

        }

        public string From => "ogg";
        public string To => "mp3";

        public async Task<byte[]> ConvertAsync(byte[] content)
        {
            var selector = _serviceProvider.GetService<IConverterSelector>();

            var oggToWavConverter = selector.Select("ogg", "wav");
            var wavToMp3Converter = selector.Select("wav", "mp3");
            return (await wavToMp3Converter.ConvertAsync(await oggToWavConverter.ConvertAsync(content)));
        }
    }
}
