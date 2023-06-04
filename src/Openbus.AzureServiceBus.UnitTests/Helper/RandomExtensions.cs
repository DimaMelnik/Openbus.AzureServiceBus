using System;
using System.Linq;

namespace Openbus.AzureServiceBus.UnitTests.Helper
{
    public static class RandomExtensions
    {
        public static string NextString(this Random random, int minLength, int? maxLength = null, bool forceNumeric = false, bool forceAlpha = false)
        {
            const string Numeric = @"0123456789";
            const string Alpha = @"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            const string AlphaNumeric = Alpha + Numeric;

            _ = random ?? throw new ArgumentNullException(nameof(random));
            var length = maxLength == null || minLength == maxLength ? minLength : random.Next(minLength, maxLength.Value);
            if (length <= 0)
            {
                return string.Empty;
            }

            var chars = new char[length];
            var numericChar = forceNumeric ? random.Next(0, length) : -1;
            var alphaChar = forceAlpha ? random.Next(0, length) : -1;
            for (var i = 0; i < length; i++)
            {
                chars[i] = i switch
                {
                    { } when numericChar >= 0 && i >= numericChar => Numeric[random.Next((numericChar = 0) + Numeric.Length)],
                    { } when alphaChar >= 0 && i >= alphaChar => Alpha[random.Next((alphaChar = 0) + Alpha.Length)],
                    _ => AlphaNumeric[random.Next(AlphaNumeric.Length)]
                };
            }

            return new string(chars);
        }

        public static int NextInt32(this Random random)
        {
            _ = random ?? throw new ArgumentNullException(nameof(random));

            var firstBits = random.Next(0, 1 << 4) << 28;
            var lastBits = random.Next(0, 1 << 28);

            return firstBits | lastBits;
        }

        public static TEnum NextEnum<TEnum>(this Random random, params TEnum[] excluding) where TEnum : struct, Enum
        {
            _ = random ?? throw new ArgumentNullException(nameof(random));

            var enumValues = Enum.GetValues(typeof(TEnum)).OfType<TEnum>().Where(e => !excluding.Contains(e)).ToArray();

            return random.NextValue(enumValues);
        }

        public static T NextValue<T>(this Random random, params T[] values)
        {
            _ = random ?? throw new ArgumentNullException(nameof(random));

            var length = values.Length;
            if (length <= 0)
            {
                return default!;
            }

            var idx = random.Next(length);

            return values[idx];
        }
    }
}
