using Carubbi.AudioConverter.Api.Utilities;
using NAudio.Wave;
using System.IO;
using System.Threading.Tasks;

namespace Carubbi.AudioConverter.Api.Converters
{
    public class Mp3ToWavConverter : IConverter
    {
        public string From => "mp3";
        public string To => "wav";

        private readonly EnvironmentVariablesConfig _environmentVariablesConfig;

        public Mp3ToWavConverter(EnvironmentVariablesConfig environmentVariablesConfig)
        {
            _environmentVariablesConfig = environmentVariablesConfig;
        }

        public async Task<byte[]> ConvertAsync(byte[] content)
        {
            _environmentVariablesConfig.CheckAddBinPath();
            var target = new WaveFormat(8000, 16, 1);
            await using var outPutStream = new MemoryStream();
            await using var mp3Reader = new Mp3FileReader(new MemoryStream(content));
            await using var conversionStream = new WaveFormatConversionStream(target, mp3Reader);
            await using var writer = new WaveFileWriter(outPutStream, conversionStream.WaveFormat);
            await conversionStream.CopyToAsync(writer);

            return outPutStream.ToArray();
        }
    }
}
