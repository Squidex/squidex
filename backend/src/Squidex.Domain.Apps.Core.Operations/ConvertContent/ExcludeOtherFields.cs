// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Core.ConvertContent;

public sealed class ExcludeOtherFields(HashSet<string> fieldsToInclude) : IContentFieldConverter
{
    public ContentFieldData? ConvertFieldBefore(IRootField field, ContentFieldData source)
    {
        return fieldsToInclude.Contains(field.Name) ? source : null;
    }
}
