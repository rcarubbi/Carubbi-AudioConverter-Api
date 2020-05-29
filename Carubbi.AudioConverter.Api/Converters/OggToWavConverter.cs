using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Carubbi.AudioConverter.Api.Converters
{
    public class OggToWavConverter : IConverter
    {
        public string From => "ogg";
        public string To => "wav";

        public async Task<byte[]> ConvertAsync(byte[] content)
        {
            var inputTemp = Path.GetTempFileName();
            var outputTemp = Path.ChangeExtension(inputTemp, "wav");
            await File.WriteAllBytesAsync(inputTemp, content);

            using var process = new Process
            {
                StartInfo =
                {
                    FileName = @".\opusdec.exe",
                    Arguments = $"{inputTemp} {outputTemp}",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            // relative path. absolute path works too.

            process.OutputDataReceived += (sender, data) => Console.WriteLine(data.Data);
            process.ErrorDataReceived += (sender, data) => Console.WriteLine(data.Data);
            Console.WriteLine("starting");
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            var exited = process.WaitForExit(1000 * 10);     // (optional) wait up to 10 seconds
            Console.WriteLine($"exit {exited}");
            File.Delete(inputTemp);
            var outputContent = await File.ReadAllBytesAsync(outputTemp);
            File.Delete(outputTemp);
            return outputContent;
        }
    }
}
