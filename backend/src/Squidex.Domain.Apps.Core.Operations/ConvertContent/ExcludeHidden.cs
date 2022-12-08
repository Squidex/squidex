// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.ConvertContent;

public sealed class ExcludeHidden : IContentFieldConverter, IContentValueConverter
{
    public static readonly ExcludeHidden Instance = new ExcludeHidden();

    private ExcludeHidden()
    {
    }

    public ContentFieldData? ConvertField(IRootField field, ContentFieldData source)
    {
        return field.IsForApi() ? source : null;
    }

    public (bool Remove, JsonValue) ConvertValue(IField field, JsonValue source, IField? parent)
    {
        return field.IsForApi() ? (false, source) : (true, default);
    }
}
