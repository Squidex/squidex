// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Types;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents;

internal sealed class ContentInterfaceGraphType : InterfaceGraphType<IEnrichedContentEntity>
{
    public ContentInterfaceGraphType()
    {
        // The name is used for equal comparison. Therefore it is important to treat it as readonly.
        Name = "Content";

        AddField(ContentFields.Id);
        AddField(ContentFields.Version);
        AddField(ContentFields.Created);
        AddField(ContentFields.CreatedBy);
        AddField(ContentFields.LastModified);
        AddField(ContentFields.LastModifiedBy);
        AddField(ContentFields.Status);
        AddField(ContentFields.StatusColor);
        AddField(ContentFields.NewStatus);
        AddField(ContentFields.NewStatusColor);
        AddField(ContentFields.Url);

        Description = "The structure of all content types.";
    }
}
