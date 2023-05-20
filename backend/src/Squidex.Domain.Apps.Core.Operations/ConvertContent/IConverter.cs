// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Json.Objects;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Domain.Apps.Core.ConvertContent;

public interface IConverter
{
}

public interface IContentDataConverter
{
    void ConvertDataBefore(Schema schema, ContentData source)
    {
    }

    void ConvertDataAfter(Schema schema, ContentData source)
    {
    }
}

public interface IContentFieldConverter : IConverter
{
    ContentFieldData? ConvertFieldBefore(IRootField field, ContentFieldData source)
    {
        return source;
    }

    ContentFieldData? ConvertFieldAfter(IRootField field, ContentFieldData source)
    {
        return source;
    }
}

public interface IContentItemConverter : IConverter
{
    JsonObject ConvertItemBefore(IField parentField, JsonObject source, IEnumerable<IField> schema)
    {
        return source;
    }

    JsonObject ConvertItemAfter(IField parentField, JsonObject source, IEnumerable<IField> schema)
    {
        return source;
    }
}

public interface IContentValueConverter : IConverter
{
    (bool Remove, JsonValue) ConvertValue(IField field, JsonValue source, IField? parent);
}
