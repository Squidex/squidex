// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using System.Linq;
using Squidex.Infrastructure;
using Squidex.Text;

namespace Microsoft.Extensions.Configuration
{
    public static class ConfigurationExtensions
    {
        public static T GetOptionalValue<T>(this IConfiguration config, string path, T defaultValue = default)
        {
            var value = config.GetValue(path, defaultValue!);

            return value;
        }

        public static int GetOptionalValue(this IConfiguration config, string path, int defaultValue)
        {
            var value = config.GetValue<string>(path);

            if (string.IsNullOrWhiteSpace(value) || !int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
            {
                result = defaultValue;
            }

            return result;
        }

        public static string GetRequiredValue(this IConfiguration config, string path)
        {
            var value = config.GetValue<string>(path);

            if (string.IsNullOrWhiteSpace(value))
            {
                var name = string.Join(" ", path.Split(':').Select(x => x.ToPascalCase()));

                throw new ConfigurationException($"Configure the {name} with '{path}'.");
            }

            return value;
        }

        public static string ConfigureByOption(this IConfiguration config, string path, Alternatives options)
        {
            var value = config.GetRequiredValue(path);

            if (options.TryGetValue(value, out var action))
            {
                action();
            }
            else if (options.TryGetValue("default", out action))
            {
                action();
            }
            else
            {
                throw new ConfigurationException($"Unsupported value '{value}' for '{path}', supported: {string.Join(" ", options.Keys)}.");
            }

            return value;
        }
    }
}
