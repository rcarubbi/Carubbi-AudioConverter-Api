using Carubbi.AudioConverter.Api.Utilities;

namespace Carubbi.AudioConverter.Api.Tests;

public class EnvironmentVariablesConfigTests
{
    [Test]
    public async Task CheckAddBinPath_appends_base_directory_once_and_skips_when_already_present()
    {
        var config = new EnvironmentVariablesConfig();
        var originalPath = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        var binPath = AppDomain.CurrentDomain.BaseDirectory;

        try
        {
            Environment.SetEnvironmentVariable("PATH", string.Empty);

            config.CheckAddBinPath();

            var pathAfterAppend = Environment.GetEnvironmentVariable("PATH");
            await Assert.That(pathAfterAppend).IsNotNull();
            await Assert.That(pathAfterAppend!.Split(Path.PathSeparator))
                .Contains(binPath);

            config.CheckAddBinPath();

            var pathAfterSecondCall = Environment.GetEnvironmentVariable("PATH");
            await Assert.That(pathAfterSecondCall)
                .IsEqualTo(pathAfterAppend);
        }
        finally
        {
            Environment.SetEnvironmentVariable("PATH", originalPath);
        }
    }
}
