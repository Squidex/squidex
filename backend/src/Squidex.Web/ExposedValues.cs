// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Reflection;
using System.Text;
using Microsoft.Extensions.Configuration;
using Squidex.Infrastructure;

namespace Squidex.Web;

public sealed class ExposedValues : Dictionary<string, string>
{
    public ExposedValues()
    {
    }

    public ExposedValues(ExposedConfiguration configured, IConfiguration configuration, Assembly? assembly = null)
    {
        Guard.NotNull(configured);
        Guard.NotNull(configuration);

        foreach (var kvp in configured)
        {
            var value = configuration.GetValue<string>(kvp.Value);

            if (!string.IsNullOrWhiteSpace(value))
            {
                this[kvp.Key] = value;
            }
        }

        if (assembly != null)
        {
            if (!ContainsKey("version"))
            {
                this["version"] = assembly.GetName()!.Version!.ToString();
            }
        }
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        foreach (var (key, value) in this)
        {
            sb.AppendIfNotEmpty(", ");
            sb.Append(key);
            sb.Append(": ");
            sb.Append(value);
        }

        return sb.ToString();
    }
}
