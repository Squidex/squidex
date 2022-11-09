// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.DefaultValues;
using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Contents.DomainObject.Guards;

public static class ValidationExtensions
{
    public static void MustDeleteDraft(this ContentOperation operation)
    {
        if (operation.Snapshot.NewStatus == null)
        {
            throw new DomainException(T.Get("contents.draftToDeleteNotFound"));
        }
    }

    public static void MustCreateDraft(this ContentOperation operation)
    {
        if (operation.Snapshot.EditingStatus() != Status.Published)
        {
            throw new DomainException(T.Get("contents.draftNotCreateForUnpublished"));
        }
    }

    public static void MustHaveData(this ContentOperation operation, ContentData? data)
    {
        if (data == null)
        {
            operation.AddError(Not.Defined(nameof(data)), nameof(data));
        }

        operation.ThrowOnErrors();
    }

    public static async Task ValidateInputAsync(this ContentOperation operation, ContentData data, bool optimize, bool published)
    {
        var validator = GetValidator(operation, optimize, published);

        await validator.ValidateInputAsync(data);

        operation.AddErrors(validator.Errors).ThrowOnErrors();
    }

    public static async Task ValidateInputPartialAsync(this ContentOperation operation, ContentData data, bool optimize, bool published)
    {
        var validator = GetValidator(operation, optimize, published);

        await validator.ValidateInputPartialAsync(data);

        operation.AddErrors(validator.Errors).ThrowOnErrors();
    }

    public static async Task ValidateContentAsync(this ContentOperation operation, ContentData data, bool optimize, bool published)
    {
        var validator = GetValidator(operation, optimize, published);

        await validator.ValidateContentAsync(data);

        operation.AddErrors(validator.Errors).ThrowOnErrors();
    }

    public static async Task ValidateContentAndInputAsync(this ContentOperation operation, ContentData data, bool optimize, bool published)
    {
        var validator = GetValidator(operation, optimize, published);

        await validator.ValidateInputAndContentAsync(data);

        operation.AddErrors(validator.Errors).ThrowOnErrors();
    }

    public static void GenerateDefaultValues(this ContentOperation operation, ContentData data)
    {
        data.GenerateDefaultValues(operation.Schema.SchemaDef, operation.Partition());
    }

    public static async Task CheckReferrersAsync(this ContentOperation operation)
    {
        var contentRepository = operation.Resolve<IContentRepository>();

        var hasReferrer = await contentRepository.HasReferrersAsync(operation.App.Id, operation.CommandId, SearchScope.All, default);

        if (hasReferrer)
        {
            throw new DomainException(T.Get("contents.referenced"), "OBJECT_REFERENCED");
        }
    }

    private static ContentValidator GetValidator(this ContentOperation operation, bool optimize, bool published)
    {
        var rootContext =
            new RootContext(operation.Resolve<IJsonSerializer>(),
                operation.App.NamedId(),
                operation.Schema.NamedId(),
                operation.SchemaDef,
                operation.CommandId,
                operation.Components);

        var validationContext = new ValidationContext(rootContext).Optimized(optimize).AsPublishing(published);

        var validator =
            new ContentValidator(operation.Partition(),
                validationContext,
                operation.Resolve<IEnumerable<IValidatorsFactory>>());

        return validator;
    }

    private static PartitionResolver Partition(this ContentOperation operation)
    {
        return operation.App.PartitionResolver();
    }
}
