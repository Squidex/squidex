// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Domain.Apps.Core.ValidateContent.Validators;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;
using Squidex.Log;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent
{
    public delegate ValidationContext ValidationUpdater(ValidationContext context);

    public static class ValidationTestExtensions
    {
        private static readonly NamedId<DomainId> AppId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private static readonly NamedId<DomainId> SchemaId = NamedId.Of(DomainId.NewGuid(), "my-schema");

        public static Task ValidateAsync(this IValidator validator, object? value, IList<string> errors,
            Schema? schema = null,
            ValidationMode mode = ValidationMode.Default,
            ValidationUpdater? updater = null,
            ValidationAction action = ValidationAction.Upsert,
            ResolvedComponents? components = null,
            DomainId? contentId = null)
        {
            var context = CreateContext(schema, mode, updater, action, components, contentId);

            return validator.ValidateAsync(value, context, CreateFormatter(errors));
        }

        public static Task ValidateAsync(this IField field, object? value, IList<string> errors,
            Schema? schema = null,
            ValidationMode mode = ValidationMode.Default,
            ValidationUpdater? updater = null,
            IValidatorsFactory? factory = null,
            ValidationAction action = ValidationAction.Upsert,
            ResolvedComponents? components = null,
            DomainId? contentId = null)
        {
            var context = CreateContext(schema, mode, updater, action, components, contentId);

            var validator = new ValidatorBuilder(factory, context).ValueValidator(field);

            return validator.ValidateAsync(value, context, CreateFormatter(errors));
        }

        public static async Task ValidatePartialAsync(this ContentData data, PartitionResolver partitionResolver, IList<ValidationError> errors,
            Schema? schema = null,
            ValidationMode mode = ValidationMode.Default,
            ValidationUpdater? updater = null,
            IValidatorsFactory? factory = null,
            ValidationAction action = ValidationAction.Upsert,
            ResolvedComponents? components = null,
            DomainId? contentId = null)
        {
            var context = CreateContext(schema, mode, updater, action, components, contentId);

            var validator = new ValidatorBuilder(factory, context).ContentValidator(partitionResolver);

            await validator.ValidateInputPartialAsync(data);

            foreach (var error in validator.Errors)
            {
                errors.Add(error);
            }
        }

        public static async Task ValidateAsync(this ContentData data, PartitionResolver partitionResolver, IList<ValidationError> errors,
            Schema? schema = null,
            ValidationMode mode = ValidationMode.Default,
            ValidationUpdater? updater = null,
            IValidatorsFactory? factory = null,
            ValidationAction action = ValidationAction.Upsert,
            ResolvedComponents? components = null,
            DomainId? contentId = null)
        {
            var context = CreateContext(schema, mode, updater, action, components, contentId);

            var validator = new ValidatorBuilder(factory, context).ContentValidator(partitionResolver);

            await validator.ValidateInputAsync(data);

            foreach (var error in validator.Errors)
            {
                errors.Add(error);
            }
        }

        public static AddError CreateFormatter(IList<string> errors)
        {
            return (field, message) =>
            {
                if (field == null || !field.Any())
                {
                    errors.Add(message);
                }
                else
                {
                    errors.Add($"{field.ToPathString()}: {message}");
                }
            };
        }

        public static ValidationContext CreateContext(
            Schema? schema = null,
            ValidationMode mode = ValidationMode.Default,
            ValidationUpdater? updater = null,
            ValidationAction action = ValidationAction.Upsert,
            ResolvedComponents? components = null,
            DomainId? contentId = null)
        {
            var context = new ValidationContext(
                TestUtils.DefaultSerializer,
                AppId,
                SchemaId,
                schema ?? new Schema(SchemaId.Name),
                components ?? ResolvedComponents.Empty,
                contentId ?? DomainId.NewGuid());

            context = context.WithMode(mode).WithAction(action);

            if (updater != null)
            {
                context = updater(context);
            }

            return context;
        }

        private sealed class ValidatorBuilder
        {
            private static readonly ISemanticLog Log = A.Fake<ISemanticLog>();
            private static readonly IValidatorsFactory Default = new DefaultValidatorsFactory();
            private readonly IValidatorsFactory? validatorFactory;
            private readonly ValidationContext validationContext;

            public ValidatorBuilder(IValidatorsFactory? validatorFactory, ValidationContext validationContext)
            {
                this.validatorFactory = validatorFactory;
                this.validationContext = validationContext;
            }

            public ContentValidator ContentValidator(PartitionResolver partitionResolver)
            {
                return new ContentValidator(partitionResolver, validationContext, CreateFactories(), Log);
            }

            public IValidator ValueValidator(IField field)
            {
                return CreateValueValidator(field);
            }

            private IValidator CreateValueValidator(IField field)
            {
                return new FieldValidator(new AggregateValidator(CreateValueValidators(field), Log), field);
            }

            private IEnumerable<IValidator> CreateValueValidators(IField field)
            {
                return CreateFactories().SelectMany(x => x.CreateValueValidators(validationContext, field, CreateValueValidator));
            }

            private IEnumerable<IValidatorsFactory> CreateFactories()
            {
                yield return Default;

                if (validatorFactory != null)
                {
                    yield return validatorFactory;
                }
            }
        }
    }
}
