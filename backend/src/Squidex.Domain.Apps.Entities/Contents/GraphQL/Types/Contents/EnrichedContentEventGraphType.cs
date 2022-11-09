// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Resolvers;
using GraphQL.Types;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents;

internal sealed class EnrichedContentEventGraphType : SharedObjectGraphType<EnrichedContentEvent>
{
    public EnrichedContentEventGraphType()
    {
        // The name is used for equal comparison. Therefore it is important to treat it as readonly.
        Name = "EnrichedContentEvent";

        AddField(new FieldType
        {
            Name = "type",
            ResolvedType = Scalars.EnrichedContentEventType,
            Resolver = Resolve(x => x.Type),
            Description = FieldDescriptions.EventType
        });

        AddField(new FieldType
        {
            Name = "id",
            ResolvedType = Scalars.NonNullString,
            Resolver = Resolve(x => x.Id.ToString()),
            Description = FieldDescriptions.EntityId
        });

        AddField(new FieldType
        {
            Name = "version",
            ResolvedType = Scalars.NonNullInt,
            Resolver = Resolve(x => x.Version),
            Description = FieldDescriptions.EntityVersion
        });

        AddField(new FieldType
        {
            Name = "created",
            ResolvedType = Scalars.NonNullDateTime,
            Resolver = Resolve(x => x.Created.ToDateTimeUtc()),
            Description = FieldDescriptions.EntityCreated
        });

        AddField(new FieldType
        {
            Name = "createdBy",
            ResolvedType = Scalars.NonNullString,
            Resolver = Resolve(x => x.CreatedBy.ToString()),
            Description = FieldDescriptions.EntityCreatedBy
        });

        AddField(new FieldType
        {
            Name = "createdByUser",
            ResolvedType = UserGraphType.NonNull,
            Resolver = Resolve(x => x.CreatedBy),
            Description = FieldDescriptions.EntityCreatedBy
        });

        AddField(new FieldType
        {
            Name = "lastModified",
            ResolvedType = Scalars.NonNullDateTime,
            Resolver = Resolve(x => x.LastModified.ToDateTimeUtc()),
            Description = FieldDescriptions.EntityLastModified
        });

        AddField(new FieldType
        {
            Name = "lastModifiedBy",
            ResolvedType = Scalars.NonNullString,
            Resolver = Resolve(x => x.LastModifiedBy.ToString()),
            Description = FieldDescriptions.EntityLastModifiedBy
        });

        AddField(new FieldType
        {
            Name = "lastModifiedByUser",
            ResolvedType = UserGraphType.NonNull,
            Resolver = Resolve(x => x.LastModifiedBy),
            Description = FieldDescriptions.EntityLastModifiedBy
        });

        AddField(new FieldType
        {
            Name = "status",
            ResolvedType = Scalars.NonNullString,
            Resolver = Resolve(x => x.Status.ToString()),
            Description = FieldDescriptions.ContentStatus
        });

        AddField(new FieldType
        {
            Name = "newStatus",
            ResolvedType = Scalars.String,
            Resolver = Resolve(x => x.NewStatus?.ToString()),
            Description = FieldDescriptions.ContentNewStatus
        });

        AddField(new FieldType
        {
            Name = "data",
            ResolvedType = Scalars.JsonNoop,
            Resolver = Resolve(x => x.Data),
            Description = FieldDescriptions.ContentData
        });

        AddField(new FieldType
        {
            Name = "dataOld",
            ResolvedType = Scalars.JsonNoop,
            Resolver = Resolve(x => x.DataOld),
            Description = FieldDescriptions.ContentDataOld
        });

        Description = "An content event";
    }

    private static IFieldResolver Resolve<T>(Func<EnrichedContentEvent, T> resolver)
    {
        return Resolvers.Sync(resolver);
    }
}
