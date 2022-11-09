// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Reflection;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Shared.Identity;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Core.HandleRules;

public static class RuleVariable
{
    public static (object? Result, string[] Remaining) GetValue(object @event, string[] path)
    {
        object? current = @event;

        var i = 0;

        for (; i < path.Length; i++)
        {
            var segment = path[i];

            if (current is ContentData data)
            {
                if (!data.TryGetValue(segment, out var temp) || temp == null)
                {
                    break;
                }

                current = temp;
            }
            else if (current is ContentFieldData field)
            {
                if (!field.TryGetValue(segment, out var temp))
                {
                    break;
                }

                current = temp;
            }
            else if (current is JsonValue json)
            {
                if (!json.TryGetValue(segment, out var temp) || temp == JsonValue.Null)
                {
                    break;
                }

                current = temp;
            }
            else if (current != null)
            {
                if (current is IUser user)
                {
                    var type = segment;

                    if (string.Equals(type, "Name", StringComparison.OrdinalIgnoreCase))
                    {
                        type = SquidexClaimTypes.DisplayName;
                    }

                    var claim = user.Claims.FirstOrDefault(x => string.Equals(x.Type, type, StringComparison.OrdinalIgnoreCase));

                    if (claim != null)
                    {
                        current = claim.Value;
                        continue;
                    }
                }

                const BindingFlags bindingFlags =
                    BindingFlags.FlattenHierarchy |
                    BindingFlags.Public |
                    BindingFlags.Instance;

                var properties = current.GetType().GetProperties(bindingFlags);
                var property = properties.FirstOrDefault(x => x.CanRead && string.Equals(x.Name, segment, StringComparison.OrdinalIgnoreCase));

                if (property == null)
                {
                    break;
                }

                current = property.GetValue(current);
            }
            else
            {
                break;
            }
        }

        return (current, path.Skip(i).ToArray());
    }
}
