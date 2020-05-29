using Carubbi.AudioConverter.Api.Utilities;
using NAudio.Lame;
using NAudio.Wave;
using System.IO;
using System.Threading.Tasks;

namespace Carubbi.AudioConverter.Api.Converters
{
    public class WavToMp3Converter : IConverter
    {
        public string From => "wav";
        public string To => "mp3";

        private readonly EnvironmentVariablesConfig _environmentVariablesConfig;

        public WavToMp3Converter(EnvironmentVariablesConfig environmentVariablesConfig)
        {
            _environmentVariablesConfig = environmentVariablesConfig;
        }

        public async Task<byte[]> ConvertAsync(byte[] content)
        {
            _environmentVariablesConfig.CheckAddBinPath();
            var target = new WaveFormat(8000, 16, 1);
            await using var outPutStream = new MemoryStream();
            await using var waveStream = new WaveFileReader(new MemoryStream(content));
            await using var conversionStream = new WaveFormatConversionStream(target, waveStream);
            await using var writer = new LameMP3FileWriter(outPutStream, conversionStream.WaveFormat, 32, null);
            await conversionStream.CopyToAsync(writer);

            return outPutStream.ToArray();
        }
    }
}
