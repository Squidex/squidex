// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Types;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents;

internal sealed class ContentInterfaceGraphType : InterfaceGraphType<EnrichedContent>
{
    public ContentInterfaceGraphType()
    {
        // The name is used for equal comparison. Therefore it is important to treat it as readonly.
        Name = "Content";

        AddField(ContentFields.IdNoResolver);
        AddField(ContentFields.VersionNoResolver);
        AddField(ContentFields.CreatedNoResolver);
        AddField(ContentFields.CreatedByNoResolver);
        AddField(ContentFields.LastModifiedNoResolver);
        AddField(ContentFields.LastModifiedByNoResolver);
        AddField(ContentFields.EditTokenNoResolver);
        AddField(ContentFields.StatusNoResolver);
        AddField(ContentFields.StatusColorNoResolver);
        AddField(ContentFields.NewStatusNoResolver);
        AddField(ContentFields.NewStatusColorNoResolver);
        AddField(ContentFields.DataDynamicNoResolver);
        AddField(ContentFields.UrlNoResolver);

        Description = "The structure of all content types.";
    }
}
