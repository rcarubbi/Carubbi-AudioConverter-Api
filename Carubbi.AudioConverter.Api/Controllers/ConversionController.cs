using Carubbi.AudioConverter.Api.Converters;
using Carubbi.AudioConverter.Api.Validators;
using Microsoft.AspNetCore.Mvc;

namespace Carubbi.AudioConverter.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConversionController(IConverterSelector converterSelector, IFileValidator fileValidator) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Post([FromQuery] string to, IFormFile source)
        {
            var (input, from) = await fileValidator.Validate(source, ModelState, int.MaxValue);
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var converter = converterSelector.Select(from!, to);
            var output = await converter.ConvertAsync(input);
            var fileDownloadName = Path.ChangeExtension(source.FileName, to);
            return File(output, "application/octet-stream", fileDownloadName);
        }
    }
}
