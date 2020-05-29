using System.Collections;
using System.Collections.Generic;
using System.IO;
using Carubbi.AudioConverter.Api.Converters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Carubbi.AudioConverter.Api.Validators;

namespace Carubbi.AudioConverter.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConversionController : ControllerBase
    {
        private readonly IConverterSelector _converterSelector;
        private readonly IFileValidator _fileValidator;
        private readonly ILogger<ConversionController> _logger;

        public ConversionController(IConverterSelector converterSelector, IFileValidator fileValidator, ILogger<ConversionController> logger)
        {
            _converterSelector = converterSelector;
            _logger = logger;
            _fileValidator = fileValidator;
        }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="to"></param>
      /// <param name="source"></param>
      /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromQuery] string to, IFormFile source)
        {
            var (input, from) = await _fileValidator.Validate(source, ModelState, int.MaxValue);
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var converter = _converterSelector.Select(from, to);
            var output = await converter.ConvertAsync(input);
            var fileDownloadName = Path.ChangeExtension(source.FileName, to);
            return File(output, "application/octet-stream", fileDownloadName);
        }
    }
}
