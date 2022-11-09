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

public interface IContentFieldAfterConverter : IConverter
{
    ContentFieldData? ConvertFieldAfter(IRootField field, ContentFieldData source);
}

public interface IContentFieldConverter : IConverter
{
    ContentFieldData? ConvertField(IRootField field, ContentFieldData source);
}

public interface IContentItemConverter : IConverter
{
    JsonObject ConvertItem(IField field, JsonObject source);
}

public interface IContentValueConverter : IConverter
{
    (bool Remove, JsonValue) ConvertValue(IField field, JsonValue source, IField? parent);
}
