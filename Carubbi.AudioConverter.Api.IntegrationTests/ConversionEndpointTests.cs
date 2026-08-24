using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;

namespace Carubbi.AudioConverter.Api.IntegrationTests;

public class ConversionEndpointTests
{
    private static IContainer? _container;
    private static string _baseUrl = string.Empty;

    [Before(Class)]
    public static async Task StartApiContainer()
    {
        var repoRoot = FindRepoRoot();

        var image = new ImageFromDockerfileBuilder()
            .WithDockerfileDirectory(repoRoot)
            .WithDockerfile("Dockerfile")
            .WithCleanUp(true)
            .Build();

        await image.CreateAsync();

        _container = new ContainerBuilder(image)
            .WithPortBinding(8080, assignRandomHostPort: true)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilHttpRequestIsSucceeded(request => request
                    .ForPort(8080)
                    .ForPath("/swagger/v1/swagger.json")))
            .Build();

        await _container.StartAsync();

        _baseUrl = new UriBuilder("http", _container.Hostname, _container.GetMappedPublicPort(8080)).Uri.ToString();
    }

    [After(Class)]
    public static async Task StopApiContainer()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
            _container = null;
        }
    }

    private static HttpClient CreateClient() => new() { BaseAddress = new Uri(_baseUrl) };

    [Test]
    public async Task Swagger_endpoint_is_available()
    {
        using var client = CreateClient();

        using var response = await client.GetAsync("swagger/v1/swagger.json");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task Post_wav_converts_to_ogg()
    {
        using var client = CreateClient();

        using var response = await client.PostAsync(
            "conversion?to=ogg",
            BuildForm(TestData.CreateWav(), "audio.wav"));

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var output = await response.Content.ReadAsByteArrayAsync();
        await Assert.That(Encoding.ASCII.GetString(output, 0, 4)).IsEqualTo("OggS");
    }

    [Test]
    public async Task Post_ogg_roundtrip_back_to_wav()
    {
        using var client = CreateClient();

        byte[] ogg;
        using (var oggResponse = await client.PostAsync(
                   "conversion?to=ogg",
                   BuildForm(TestData.CreateWav(), "audio.wav")))
        {
            oggResponse.EnsureSuccessStatusCode();
            ogg = await oggResponse.Content.ReadAsByteArrayAsync();
        }

        using var wavResponse = await client.PostAsync(
            "conversion?to=wav",
            BuildForm(ogg, "audio.ogg"));

        await Assert.That(wavResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var output = await wavResponse.Content.ReadAsByteArrayAsync();
        await Assert.That(Encoding.ASCII.GetString(output, 0, 4)).IsEqualTo("RIFF");
    }

    [Test]
    public async Task Post_without_file_returns_bad_request()
    {
        using var client = CreateClient();

        using var response = await client.PostAsync("conversion?to=mp3", new MultipartFormDataContent());

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task Post_file_with_unsupported_signature_returns_bad_request()
    {
        using var client = CreateClient();

        using var response = await client.PostAsync(
            "conversion?to=ogg",
            BuildForm([0xFF, 0xFF, 0xFF, 0xFF], "audio.txt"));

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    private static MultipartFormDataContent BuildForm(byte[] fileBytes, string fileName)
    {
        var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        form.Add(fileContent, "source", fileName);
        return form;
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Dockerfile")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new InvalidOperationException("Could not locate the repository root (Dockerfile).");
    }
}

public static class TestData
{
    public static byte[] CreateWav(int sampleRate = 8000, int seconds = 1, double frequency = 440)
    {
        var samples = sampleRate * seconds;
        var dataBytes = samples * sizeof(short);

        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream))
        {
            writer.Write("RIFF"u8);
            writer.Write(36 + dataBytes);
            writer.Write("WAVE"u8);
            writer.Write("fmt "u8);
            writer.Write(16);
            writer.Write((short)1); // PCM
            writer.Write((short)1); // mono
            writer.Write(sampleRate);
            writer.Write(sampleRate * sizeof(short));
            writer.Write((short)sizeof(short));
            writer.Write((short)16);
            writer.Write("data"u8);
            writer.Write(dataBytes);

            for (var i = 0; i < samples; i++)
            {
                writer.Write((short)(Math.Sin(2 * Math.PI * frequency * i / sampleRate) * 10000));
            }
        }

        return stream.ToArray();
    }
}
