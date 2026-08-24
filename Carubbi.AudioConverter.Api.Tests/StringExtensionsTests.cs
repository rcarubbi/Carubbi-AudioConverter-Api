using Carubbi.AudioConverter.Api.Extensions;

namespace Carubbi.AudioConverter.Api.Tests;

public class StringExtensionsTests
{
    [Test]
    public async Task Capitalize_uppercases_first_char()
    {
        await Assert.That("carubbi".Capitalize()).IsEqualTo("Carubbi");
    }

    [Test]
    public async Task Capitalize_keeps_rest_of_string_untouched()
    {
        await Assert.That("aUDIO".Capitalize()).IsEqualTo("AUDIO");
    }

    [Test]
    public async Task Capitalize_single_char_returns_uppercase()
    {
        await Assert.That("x".Capitalize()).IsEqualTo("X");
    }

    [Test]
    public async Task Capitalize_null_or_empty_returns_empty()
    {
        await Assert.That(string.Empty.Capitalize()).IsEqualTo(string.Empty);
        await Assert.That(((string?)null).Capitalize()).IsEqualTo(string.Empty);
    }
}
