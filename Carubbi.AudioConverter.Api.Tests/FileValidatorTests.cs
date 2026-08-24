using Carubbi.AudioConverter.Api.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Carubbi.AudioConverter.Api.Tests;

public class FileValidatorTests
{
    private static readonly byte[] Mp3Header = [0x49, 0x44, 0x33, 0x04, 0x00];
    private static readonly byte[] WavHeader = [0x52, 0x49, 0x46, 0x46, 0x24, 0x08, 0x00, 0x00];
    private static readonly byte[] OggHeader = [0x4F, 0x67, 0x67, 0x53, 0x00, 0x02, 0x00, 0x00];

    private static (FileValidator Validator, ModelStateDictionary ModelState) Create() =>
        (new FileValidator(), new ModelStateDictionary());

    private static IFormFile CreateFormFile(byte[] content, string fileName = "audio.mp3") =>
        new FormFile(new MemoryStream(content), 0, content.Length, "source", fileName);

    [Test]
    public async Task Validate_null_file_adds_error_and_returns_empty()
    {
        var (validator, modelState) = Create();

        var (content, from) = await validator.Validate(null, modelState, int.MaxValue);

        await Assert.That(modelState.IsValid).IsFalse();
        await Assert.That(content).IsEmpty();
        await Assert.That(from).IsNull();
        await Assert.That(modelState.Root.Errors.Single().ErrorMessage).IsEqualTo("the file was not sent.");
    }

    [Test]
    public async Task Validate_empty_file_is_rejected()
    {
        var (validator, modelState) = Create();

        var (content, from) = await validator.Validate(CreateFormFile([]), modelState, int.MaxValue);

        await Assert.That(modelState.IsValid).IsFalse();
        await Assert.That(content).IsEmpty();
        await Assert.That(from).IsNull();
    }

    [Test]
    public async Task Validate_file_above_size_limit_is_rejected()
    {
        var (validator, modelState) = Create();

        var (content, from) = await validator.Validate(CreateFormFile(Mp3Header), modelState, sizeLimit: 2);

        await Assert.That(modelState.IsValid).IsFalse();
        await Assert.That(content).IsEmpty();
        await Assert.That(from).IsNull();
        await Assert.That(modelState["source"]!.Errors.Single().ErrorMessage).Contains("exceeds");
    }

    [Test]
    public async Task Validate_unsupported_signature_is_rejected()
    {
        var (validator, modelState) = Create();
        var file = CreateFormFile([0xFF, 0xFF, 0xFF, 0xFF], "audio.exe");

        var (content, from) = await validator.Validate(file, modelState, int.MaxValue);

        await Assert.That(modelState.IsValid).IsFalse();
        await Assert.That(content).IsEmpty();
        await Assert.That(from).IsNull();
        await Assert.That(modelState["source"]!.Errors.Single().ErrorMessage).Contains("isn't supported");
    }

    [Test]
    public async Task Validate_file_without_extension_is_rejected()
    {
        var (validator, modelState) = Create();

        var (_, from) = await validator.Validate(CreateFormFile(Mp3Header, "audio"), modelState, int.MaxValue);

        await Assert.That(modelState.IsValid).IsFalse();
        await Assert.That(from).IsNull();
    }

    [Test]
    [Arguments(".mp3", new byte[] { 0x49, 0x44, 0x33, 0x04, 0x00 })]
    [Arguments(".wav", new byte[] { 0x52, 0x49, 0x46, 0x46, 0x24, 0x08, 0x00, 0x00 })]
    [Arguments(".ogg", new byte[] { 0x4F, 0x67, 0x67, 0x53, 0x00, 0x02, 0x00, 0x00 })]
    public async Task Validate_known_signatures_return_content_and_extension(string expectedExtension, byte[] header)
    {
        var (validator, modelState) = Create();

        var (content, from) = await validator.Validate(
            CreateFormFile(header, $"audio{expectedExtension}"), modelState, int.MaxValue);

        await Assert.That(modelState.IsValid).IsTrue();
        await Assert.That(content.SequenceEqual(header)).IsTrue();
        await Assert.That(from).IsEqualTo(expectedExtension.TrimStart('.'));
    }
}
