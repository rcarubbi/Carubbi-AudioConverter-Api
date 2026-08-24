using Carubbi.AudioConverter.Api.Converters;

namespace Carubbi.AudioConverter.Api.Tests;

public class FakeConverter(string from, string to) : IConverter
{
    public string From { get; } = from;
    public string To { get; } = to;

    public Task<byte[]> ConvertAsync(byte[] content) => Task.FromResult(content);
}

public class ConverterSelectorTests
{
    private static ConverterSelector CreateSelector() => new(
    [
        new FakeConverter("wav", "mp3"),
        new FakeConverter("mp3", "wav"),
        new FakeConverter("wav", "ogg"),
        new FakeConverter("ogg", "wav"),
        new FakeConverter("ogg", "mp3"),
        new FakeConverter("mp3", "ogg"),
    ]);

    [Test]
    public async Task Select_returns_converter_matching_from_and_to()
    {
        var selector = CreateSelector();

        var converter = selector.Select("wav", "mp3");

        await Assert.That(converter.From).IsEqualTo("wav");
        await Assert.That(converter.To).IsEqualTo("mp3");
    }

    [Test]
    public async Task Select_is_case_sensitive()
    {
        var selector = CreateSelector();

        Assert.Throws<NotSupportedException>(() => selector.Select("WAV", "MP3"));
    }

    [Test]
    [Arguments("flac", "mp3")]
    [Arguments("wav", "flac")]
    [Arguments("", "")]
    public async Task Select_throws_when_pair_not_supported(string from, string to)
    {
        var selector = CreateSelector();

        var exception = Assert.Throws<NotSupportedException>(() => selector.Select(from, to));

        await Assert.That(exception.Message).Contains($"Conversion from {from} to {to} is not supported");
    }

    [Test]
    public async Task Select_returns_each_direction_of_same_format_pair()
    {
        var selector = CreateSelector();

        var toMp3 = selector.Select("wav", "mp3");
        var toWav = selector.Select("mp3", "wav");

        await Assert.That(toMp3).IsNotSameReferenceAs(toWav);
    }
}
