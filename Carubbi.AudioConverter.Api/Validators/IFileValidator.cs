using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Carubbi.AudioConverter.Api.Validators
{
    public interface IFileValidator
    {
        Task<(byte[], string)> Validate(IFormFile formFile, ModelStateDictionary modelState, long sizeLimit);
    }
}
