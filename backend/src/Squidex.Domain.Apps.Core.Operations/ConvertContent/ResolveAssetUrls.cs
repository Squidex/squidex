// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.ConvertContent;

public sealed class ResolveAssetUrls : IContentValueConverter
{
    private readonly NamedId<DomainId> appId;
    private readonly IUrlGenerator urlGenerator;
    private readonly Func<IField, IField?, bool> shouldHandle;

    public ResolveAssetUrls(NamedId<DomainId> appId, IUrlGenerator urlGenerator, IReadOnlyCollection<string>? fields)
    {
        this.appId = appId;

        if (fields == null || fields.Count == 0)
        {
            shouldHandle = (field, parent) => false;
        }
        else if (fields.Contains("*"))
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

        this.urlGenerator = urlGenerator;
    }

    public (bool Remove, JsonValue) ConvertValue(IField field, JsonValue source, IField? parent)
    {
        if (field is IField<AssetsFieldProperties> && source.Value is JsonArray a && shouldHandle(field, parent))
        {
            for (var i = 0; i < a.Count; i++)
            {
                a[i] = urlGenerator.AssetContent(appId, a[i].ToString());
            }
        }

        return (false, source);
    }
}
