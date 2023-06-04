using System;
using System.Collections.Generic;
using System.Linq;

namespace Openbus.AzureServiceBus.Extensions
{
    public static class DictionaryExtensions
    {
        /// <summary>
        ///     Returns true if the dictionary contains a matching key value pair.
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="stringComparison"></param>
        /// <returns></returns>
        public static bool ContainsKeyValuePair<TValue>(this IEnumerable<KeyValuePair<string, TValue>> dictionary, string key,
            string value, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
        {
            return dictionary.ContainsKeyValuePair(key, v => string.Equals(value, v?.ToString(), stringComparison));
        }

        /// <summary>
        ///     Returns true if the dictionary contains a matching key and the value comparer evaluates to true.
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="valueComparer"></param>
        /// <returns></returns>
        public static bool ContainsKeyValuePair<TValue>(this IEnumerable<KeyValuePair<string, TValue>> dictionary, string key,
            Func<TValue, bool> valueComparer)
        {
            return dictionary != null &&
                   dictionary.Any(prop =>
                       string.Equals(prop.Key, key, StringComparison.OrdinalIgnoreCase) && valueComparer(prop.Value));
        }
    }
}