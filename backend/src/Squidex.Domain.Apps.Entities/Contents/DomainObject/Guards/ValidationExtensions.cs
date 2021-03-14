// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
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
using Squidex.Log;

namespace Squidex.Domain.Apps.Entities.Contents.DomainObject.Test
{
    public static class ValidationExtensions
    {
        public static void MustDeleteDraft(this OperationContext context)
        {
            if (context.Content.NewStatus == null)
            {
                throw new DomainException(T.Get("contents.draftToDeleteNotFound"));
            }
        }

        public static void MustCreateDraft(this OperationContext context)
        {
            if (context.Content.Status != Status.Published)
            {
                throw new DomainException(T.Get("contents.draftNotCreateForUnpublished"));
            }
        }

        public static void MustHaveData(this OperationContext context, ContentData? data)
        {
            if (data == null)
            {
                context.AddError(Not.Defined(nameof(data)), nameof(data));
                context.ThrowOnErrors();
            }
        }

        public static async Task ValidateInputAsync(this OperationContext context, ContentData data, bool optimize)
        {
            var validator = GetValidator(context, optimize);

            await validator.ValidateInputAsync(data);

            context.Errors.AddRange(validator.Errors);
            context.ThrowOnErrors();
        }

        public static async Task ValidateInputPartialAsync(this OperationContext context, ContentData? data, bool optimize)
        {
            var validator = GetValidator(context, optimize);

            await validator.ValidateInputPartialAsync(data);

            context.Errors.AddRange(validator.Errors);
            context.ThrowOnErrors();
        }

        public static async Task ValidateContentAndInputAsync(this OperationContext context, ContentData data, bool optimize)
        {
            var validator = GetValidator(context, optimize);

            await validator.ValidateInputAsync(data);
            await validator.ValidateContentAsync(data);

            context.Errors.AddRange(validator.Errors);
            context.ThrowOnErrors();
        }

        public static async Task ValidateContentAsync(this OperationContext context, ContentData data, bool optimize)
        {
            var validator = GetValidator(context, optimize);

            await validator.ValidateContentAsync(data);

            context.Errors.AddRange(validator.Errors);
            context.ThrowOnErrors();
        }

        public static void GenerateDefaultValues(this OperationContext context, ContentData data)
        {
            data.GenerateDefaultValues(context.Schema.SchemaDef, context.Partition());
        }

        public static async Task CheckReferrersAsync(this OperationContext context)
        {
            var contentRepository = context.Services.GetRequiredService<IContentRepository>();

            var hasReferrer = await contentRepository.HasReferrersAsync(context.App.Id, context.ContentId, SearchScope.All);

            if (hasReferrer)
            {
                throw new DomainException(T.Get("contents.referenced"));
            }
        }

        private static ContentValidator GetValidator(this OperationContext context, bool optimize)
        {
            var serializer = context.Services.GetRequiredService<IJsonSerializer>();

            var validators = context.Services.GetRequiredService<IEnumerable<IValidatorsFactory>>();

            var validationContext =
                new ValidationContext(serializer,
                    context.App.NamedId(),
                    context.Schema.NamedId(),
                    context.SchemaDef,
                    context.ContentId)
                .Optimized(optimize);

            var validator =
                new ContentValidator(context.Partition(),
                    validationContext,
                    validators,
                    context.Services.GetRequiredService<ISemanticLog>());

            return validator;
        }

        private static PartitionResolver Partition(this OperationContext context)
        {
            return context.App.PartitionResolver();
        }
    }
}
