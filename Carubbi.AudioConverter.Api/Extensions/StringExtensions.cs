namespace Carubbi.AudioConverter.Api.Extensions
{
    public static class StringExtensions
    {
        public static string Capitalize(this string instance)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(instance))
            {
                return string.Empty;
            }
            // Return char and concat substring.
            return char.ToUpper(instance[0]) + instance.Substring(1);
        }
    }
}
