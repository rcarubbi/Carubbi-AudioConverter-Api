using System;
using System.Collections.Generic;
using System.Linq;

namespace Carubbi.AudioConverter.Api.Converters
{
    public class ConverterSelector : IConverterSelector
    {
        private readonly IEnumerable<IConverter> _converters;

        public ConverterSelector(IEnumerable<IConverter> converters)
        {
            _converters = converters;
        }

        public IConverter Select(string @from, string to)
        {
            var converter = _converters.FirstOrDefault(x => x.From == from && x.To == to);
            if (converter == null)
                throw new NotSupportedException($"Conversion from {from} to {to} is not supported");

            return converter;
        }
    }
}
