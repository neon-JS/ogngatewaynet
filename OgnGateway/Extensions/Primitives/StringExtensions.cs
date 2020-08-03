using System;

namespace OgnGateway.Extensions.Primitives
{
    /// <summary>
    /// Class containing extension methods for the string type
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Ensures that a string is not null or empty or whitespace
        /// </summary>
        /// <param name="subject">The string to check</param>
        /// <exception cref="NullReferenceException">if string is null or empty or whitespace</exception>
        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Global
        public static void EnsureNotEmpty(this string subject)
        {
            if (string.IsNullOrEmpty(subject) || string.IsNullOrWhiteSpace(subject))
            {
                throw new NullReferenceException(nameof(subject) + " is null or empty!");
            }
        }
    }
}