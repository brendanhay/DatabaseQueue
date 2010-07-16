using System.Collections.Generic;
using System.Linq;

namespace DatabaseQueue.Extensions
{
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Determine whether an <see cref="IEnumerable{T}" /> is null or empty.
        /// </summary>
        /// <returns>True if null, or empty.</returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> self)
        {
            return self == null || !self.Any();
        }
    }
}
