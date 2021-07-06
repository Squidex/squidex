// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.ConvertContent
{
    public delegate IJsonValue? ValueConverter(IJsonValue value, IField field, IArrayField? parent);

    public static class ValueConverters
    {
        public static readonly ValueConverter Noop = (value, field, parent) => value;

        public static readonly ValueConverter ExcludeHidden = (value, field, parent) =>
        {
            return field.IsForApi() ? value : null;
        };

        public static ValueConverter ExcludeChangedTypes(IJsonSerializer jsonSerializer)
        {
            return (value, field, parent) =>
            {
                if (value.Type == JsonValueType.Null)
                {
                    return value;
                }

                try
                {
                    if (!JsonValueValidator.IsValid(field, value, jsonSerializer))
                    {
                        return null;
                    }
                }
                catch
                {
                    return null;
                }

                return value;
            };
        }

        public static ValueConverter ResolveAssetUrls(NamedId<DomainId> appId, IReadOnlyCollection<string>? fields, IUrlGenerator urlGenerator)
        {
            if (fields?.Any() != true)
            {
                return Noop;
            }

            Func<IField, IField?, bool> shouldHandle;

            if (fields.Contains("*"))
            {
                shouldHandle = (field, parent) => true;
            }
            else
            {
                var paths = fields.Select(x => x.Split('.')).ToList();

                shouldHandle = (field, parent) =>
                {
                    for (var i = 0; i < paths.Count; i++)
                    {
                        var path = paths[i];

                        if (parent != null)
                        {
                            if (path.Length == 2 && path[0] == parent.Name && path[1] == field.Name)
                            {
                                return true;
                            }
                        }
                        else
                        {
                            if (path.Length == 1 && path[0] == field.Name)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                };
            }

            return (value, field, parent) =>
            {
                if (field is IField<AssetsFieldProperties> && value is JsonArray array && shouldHandle(field, parent))
                {
                    for (var i = 0; i < array.Count; i++)
                    {
                        var id = array[i].ToString();

                        array[i] = JsonValue.Create(urlGenerator.AssetContent(appId, id));
                    }
                }

                return value;
            };
        }
    }
}
