// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ExtractReferenceIds;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Primitives;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents;

internal static class ContentFields
{
    public static readonly IFieldResolver ResolveStringFieldAssets = Resolvers.Sync<string, object>((value, fieldContext, context) =>
    {
        var ids = context.Resolve<StringReferenceExtractor>().GetEmbeddedAssetIds(value).ToList();

        return context.GetAssets(ids,
            fieldContext.CacheDuration());
    });

    public static readonly IFieldResolver ResolveStringFieldContents = Resolvers.Sync<string, object>((value, fieldContext, context) =>
    {
        var ids = context.Resolve<StringReferenceExtractor>().GetEmbeddedContentIds(value).ToList();

        return context.GetContents(ids,
            fieldContext.FieldNames(),
            fieldContext.CacheDuration());
    });

    public static readonly IFieldResolver ResolveRichTextFieldAssets = Resolvers.Sync<RichTextNode, object>((value, fieldContext, context) =>
    {
        var ids = context.Resolve<StringReferenceExtractor>().GetEmbeddedAssetIds(value).ToList();

        return context.GetAssets(ids,
            fieldContext.CacheDuration());
    });

    public static readonly IFieldResolver ResolveRichTextFieldContents = Resolvers.Sync<RichTextNode, object>((value, fieldContext, context) =>
    {
        var ids = context.Resolve<StringReferenceExtractor>().GetEmbeddedContentIds(value).ToList();

        return context.GetContents(ids,
            fieldContext.FieldNames(),
            fieldContext.CacheDuration());
    });

    public static readonly FieldType Id = new FieldType
    {
        Name = "id",
        ResolvedType = Scalars.NonNullString,
        Resolver = EntityResolvers.Id,
        Description = FieldDescriptions.EntityId
    };

    public static readonly FieldType IdNoResolver = Id.WithouthResolver();

    public static readonly FieldType Version = new FieldType
    {
        Name = "version",
        ResolvedType = Scalars.NonNullInt,
        Resolver = EntityResolvers.Version,
        Description = FieldDescriptions.EntityVersion
    };

    public static readonly FieldType VersionNoResolver = Version.WithouthResolver();

    public static readonly FieldType Created = new FieldType
    {
        Name = "created",
        ResolvedType = Scalars.NonNullDateTime,
        Resolver = EntityResolvers.Created,
        Description = FieldDescriptions.EntityCreated
    };

    public static readonly FieldType CreatedNoResolver = Created.WithouthResolver();

    public static readonly FieldType CreatedBy = new FieldType
    {
        Name = "createdBy",
        ResolvedType = Scalars.NonNullString,
        Resolver = EntityResolvers.CreatedBy,
        Description = FieldDescriptions.EntityCreatedBy
    };

    public static readonly FieldType CreatedByNoResolver = CreatedBy.WithouthResolver();

    public static readonly FieldType CreatedByUser = new FieldType
    {
        Name = "createdByUser",
        ResolvedType = UserGraphType.NonNull,
        Resolver = EntityResolvers.CreatedByUser,
        Description = FieldDescriptions.EntityCreatedBy
    };

    public static readonly FieldType CreatedByUserNoResolver = CreatedByUser.WithouthResolver();

    public static readonly FieldType LastModified = new FieldType
    {
        Name = "lastModified",
        ResolvedType = Scalars.NonNullDateTime,
        Resolver = EntityResolvers.LastModified,
        Description = FieldDescriptions.EntityLastModified
    };

    public static readonly FieldType LastModifiedNoResolver = LastModified.WithouthResolver();

    public static readonly FieldType LastModifiedBy = new FieldType
    {
        Name = "lastModifiedBy",
        ResolvedType = Scalars.NonNullString,
        Resolver = EntityResolvers.LastModifiedBy,
        Description = FieldDescriptions.EntityLastModifiedBy
    };

    public static readonly FieldType LastModifiedByNoResolver = LastModifiedBy.WithouthResolver();

    public static readonly FieldType LastModifiedByUser = new FieldType
    {
        Name = "lastModifiedByUser",
        ResolvedType = UserGraphType.NonNull,
        Resolver = EntityResolvers.LastModifiedByUser,
        Description = FieldDescriptions.EntityLastModifiedBy
    };

    public static readonly FieldType LastModifiedByUserNoResolver = LastModifiedByUser.WithouthResolver();

    public static readonly FieldType Status = new FieldType
    {
        Name = "status",
        ResolvedType = Scalars.NonNullString,
        Resolver = Resolve(x => x.Status.ToString().ToUpperInvariant()),
        Description = FieldDescriptions.ContentStatus
    };

    public static readonly FieldType StatusNoResolver = Status.WithouthResolver();

    public static readonly FieldType StatusColor = new FieldType
    {
        Name = "statusColor",
        ResolvedType = Scalars.NonNullString,
        Resolver = Resolve(x => x.StatusColor),
        Description = FieldDescriptions.ContentStatusColor
    };

    public static readonly FieldType StatusColorNoResolver = StatusColor.WithouthResolver();

    public static readonly FieldType NewStatus = new FieldType
    {
        Name = "newStatus",
        ResolvedType = Scalars.String,
        Resolver = Resolve(x => x.NewStatus?.ToString().ToUpperInvariant()),
        Description = FieldDescriptions.ContentNewStatus
    };

    public static readonly FieldType NewStatusNoResolver = NewStatus.WithouthResolver();

    public static readonly FieldType NewStatusColor = new FieldType
    {
        Name = "newStatusColor",
        ResolvedType = Scalars.String,
        Resolver = Resolve(x => x.NewStatusColor),
        Description = FieldDescriptions.ContentStatusColor
    };

    public static readonly FieldType NewStatusColorNoResolver = NewStatusColor.WithouthResolver();

    public static readonly FieldType SchemaId = new FieldType
    {
        Name = "schemaId",
        ResolvedType = Scalars.NonNullString,
        Resolver = Resolve(x => x[Component.Discriminator].ToString()),
        Description = FieldDescriptions.ContentSchemaId
    };

    public static readonly FieldType SchemaIdNoResolver = SchemaId.WithouthResolver();

    public static readonly FieldType SchemaName = new FieldType
    {
        Name = "schemaName",
        ResolvedType = Scalars.String,
        Resolver = Resolve(x => GetSchemaName(x)),
        Description = FieldDescriptions.ContentSchemaName
    };

    public static readonly FieldType SchemaNameNoResolver = SchemaName.WithouthResolver();

    public static readonly FieldType Url = new FieldType
    {
        Name = "url",
        ResolvedType = Scalars.NonNullString,
        Resolver = ContentResolvers.Url,
        Description = FieldDescriptions.ContentUrl
    };

    public static readonly FieldType UrlNoResolver = Url.WithouthResolver();

    public static readonly FieldType EditToken = new FieldType
    {
        Name = "editToken",
        ResolvedType = Scalars.String,
        Resolver = Resolve(x => x.EditToken),
        Description = FieldDescriptions.EditToken
    };

    public static readonly FieldType EditTokenNoResolver = EditToken.WithouthResolver();

    public static readonly FieldType DataDynamic = new FieldType
    {
        Name = "data__dynamic",
        ResolvedType = Scalars.Json,
        Resolver = Resolve(x => x.Data),
        Description = FieldDescriptions.ContentData
    };

    public static readonly FieldType DataDynamicNoResolver = DataDynamic.WithouthResolver();

    public static readonly FieldType StringFieldText = new FieldType
    {
        Name = "text",
        ResolvedType = Scalars.String,
        Resolver = Resolvers.Sync<string, string>(x => x),
        Description = FieldDescriptions.StringFieldText
    };

    public static readonly FieldType StringFieldAssets = new FieldType
    {
        Name = "assets",
        ResolvedType = new NonNullGraphType(SharedTypes.AssetsList),
        Resolver = ResolveStringFieldAssets,
        Description = FieldDescriptions.StringFieldAssets
    };

    public static readonly FieldType RichTextFieldValue = new FieldType
    {
        Name = "value",
        ResolvedType = Scalars.Json,
        Resolver = Resolvers.Sync<RichTextNode, JsonObject?>(x => x.Root),
        Description = FieldDescriptions.RichTextFieldValue
    };

    public static readonly FieldType RichTextFieldAssets = new FieldType
    {
        Name = "assets",
        ResolvedType = new NonNullGraphType(SharedTypes.AssetsList),
        Resolver = ResolveRichTextFieldAssets,
        Description = FieldDescriptions.RichTextFieldAssets
    };

    public static readonly FieldType RichTextFieldMarkdown = new FieldType
    {
        Name = "markdown",
        ResolvedType = Scalars.NonNullString,
        Resolver = Resolvers.Sync<RichTextNode, string>(x => x.ToMarkdown()),
        Description = FieldDescriptions.RichTextFieldMarkdown
    };

    public static readonly FieldType RichTextFieldText = new FieldType
    {
        Name = "text",
        ResolvedType = Scalars.NonNullString,
        Resolver = Resolvers.Sync<RichTextNode, string>(x => x.ToText()),
        Description = FieldDescriptions.RichTextFieldMarkdown
    };

    public static readonly FieldType RichTextFieldHtml = new FieldType
    {
        Name = "html",
        Arguments =
        [
            new QueryArgument(Scalars.Int)
            {
                Name = "indentation",
                Description = FieldDescriptions.Indentation,
                DefaultValue = 4
            },
        ],
        ResolvedType = Scalars.NonNullString,
        Resolver = Resolvers.Sync<RichTextNode, string>((x, ctx, _) => x.ToHtml(ctx.GetArgument<int>("indentation", 4))),
        Description = FieldDescriptions.RichTextFieldHtml
    };

    private static IFieldResolver Resolve<T>(Func<JsonObject, T> resolver)
    {
        return Resolvers.Sync(resolver);
    }

    private static IFieldResolver Resolve<T>(Func<EnrichedContent, T> resolver)
    {
        return Resolvers.Sync(resolver);
    }

    private static string? GetSchemaName(JsonObject component)
    {
        if (component.TryGetValue("schemaName", out var value) && value.Value is string name)
        {
            return name;
        }

        return null;
    }
}
