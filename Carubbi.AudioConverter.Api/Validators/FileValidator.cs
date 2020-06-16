using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;


namespace Carubbi.AudioConverter.Api.Validators
{
    public class FileValidator : IFileValidator
    {

        private static readonly Dictionary<string, List<byte[]>> FileSignature = new Dictionary<string, List<byte[]>>
        {
            { ".mp3", new List<byte[]> { new byte[] { 0x49, 0x44, 0x33 } } },
            { ".wav", new List<byte[]> { new byte[] { 0x52, 0x49, 0x46, 0x46 } } },
            { ".ogg", new List<byte[]>
            {
                new byte[] { 0x4F, 0x67, 0x67, 0x53, 0x00, 0x02, 0x00, 0x00 },
            } },
        };
         
        public async Task<(byte[], string)> Validate(IFormFile formFile, ModelStateDictionary modelState, long sizeLimit)
        {
            if (formFile == null)
            {
                modelState.AddModelError(string.Empty, "the file was not sent.");

                return (new byte[0], null);
            }

            // the file name, HTML-encode the value.
            var trustedFileNameForDisplay = WebUtility.HtmlEncode(formFile.FileName);

            // Check the file length. This check doesn't catch files that only have 
            // a BOM as their content.
            if (formFile.Length == 0)
            {
                modelState.AddModelError(formFile.Name, $"{trustedFileNameForDisplay} is empty.");

                return (new byte[0], null);
            }

            if (formFile.Length > sizeLimit)
            {
                var megabyteSizeLimit = sizeLimit / 1048576;
                modelState.AddModelError(formFile.Name, $"{trustedFileNameForDisplay} exceeds {megabyteSizeLimit:N1} MB.");
                return (new byte[0], null);
            }

            try
            {
                await using var memoryStream = new MemoryStream();
                await formFile.CopyToAsync(memoryStream);

                // Check the content length in case the file's only
                // content was a BOM and the content is actually
                // empty after removing the BOM.
                if (memoryStream.Length == 0)
                {
                    modelState.AddModelError(formFile.Name,$"{trustedFileNameForDisplay} is empty.");
                }

                var (valid, extension) = IsValidFileExtensionAndSignature(formFile.FileName, memoryStream);
                if (!valid)
                {
                    modelState.AddModelError(formFile.Name,$"{trustedFileNameForDisplay} file type isn't supported.");
                }
                else
                {
                    return (memoryStream.ToArray(), extension.Substring(1));
                }
            }
            catch (Exception ex)
            {
                modelState.AddModelError(formFile.Name,
                    $"{trustedFileNameForDisplay} upload failed. " +
                    $"Please contact the Help Desk for support. Error: {ex.HResult}");
                // Log the exception
            }

            return (new byte[0], null);
        }

        private static (bool, string) IsValidFileExtensionAndSignature(string fileName, Stream data)
        {
            if (string.IsNullOrEmpty(fileName) || data == null || data.Length == 0)
            {
                return (false, null);
            }

            var ext = Path.GetExtension(fileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(ext))
            {
                return (false, null);
            }

            data.Position = 0;

            using var reader = new BinaryReader(data);
            // File signature check
            // --------------------
            // With the file signatures provided in the _fileSignature
            // dictionary, the following code tests the input content's
            // file signature.
            var headerBytes = reader.ReadBytes(FileSignature.Values.SelectMany(x => x).Max(m => m.Length));

            foreach (var (extension, signatures) in FileSignature)
            {
                var validExtension = signatures.Any(signature =>
                    headerBytes.Take(signature.Length).SequenceEqual(signature));
                if (validExtension)
                    return (true, extension);
            }

            return (false, null);
        }
    }
}
