// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.ValidateContent.Validators;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Validation;

#pragma warning disable SA1028, IDE0004 // Code must not contain trailing whitespace

namespace Squidex.Domain.Apps.Core.ValidateContent
{
    public sealed class ContentValidator
    {
        private readonly PartitionResolver partitionResolver;
        private readonly ValidationContext context;
        private readonly IEnumerable<IValidatorsFactory> factories;
        private readonly ISemanticLog log;
        private readonly ConcurrentBag<ValidationError> errors = new ConcurrentBag<ValidationError>();

        public IReadOnlyCollection<ValidationError> Errors
        {
            get { return errors; }
        }

        public ContentValidator(PartitionResolver partitionResolver, ValidationContext context, IEnumerable<IValidatorsFactory> factories, ISemanticLog log)
        {
            Guard.NotNull(context, nameof(context));
            Guard.NotNull(factories, nameof(factories));
            Guard.NotNull(partitionResolver, nameof(partitionResolver));

            this.context = context;
            this.factories = factories;
            this.log = log;
            this.partitionResolver = partitionResolver;
        }

        private void AddError(IEnumerable<string> path, string message)
        {
            var pathString = path.ToPathString();

            errors.Add(new ValidationError(message, pathString));
        }

        public Task ValidateInputPartialAsync(NamedContentData data)
        {
            Guard.NotNull(data, nameof(data));

            var validator = CreateSchemaValidator(true);

            return validator.ValidateAsync(data, context, AddError);
        }

        public Task ValidateInputAsync(NamedContentData data)
        {
            Guard.NotNull(data, nameof(data));

            var validator = CreateSchemaValidator(false);

            return validator.ValidateAsync(data, context, AddError);
        }

        public Task ValidateContentAsync(NamedContentData data)
        {
            Guard.NotNull(data, nameof(data));

            var validator = new AggregateValidator(CreateContentValidators(), log);

            return validator.ValidateAsync(data, context, AddError);
        }

        private IValidator CreateSchemaValidator(bool isPartial)
        {
            var fieldsValidators = new Dictionary<string, (bool IsOptional, IValidator Validator)>(context.Schema.Fields.Count);

            foreach (var field in context.Schema.Fields)
            {
                fieldsValidators[field.Name] = (!field.RawProperties.IsRequired, CreateFieldValidator(field, isPartial));
            }

            return new ObjectValidator<ContentFieldData>(fieldsValidators, isPartial, "field");
        }

        private IValidator CreateFieldValidator(IRootField field, bool isPartial)
        {
            var partitioning = partitionResolver(field.Partitioning);

            var fieldValidator = CreateFieldValidator(field);
            var fieldsValidators = new Dictionary<string, (bool IsOptional, IValidator Validator)>();

            foreach (var partitionKey in partitioning.AllKeys)
            {
                var optional = partitioning.IsOptional(partitionKey);

                fieldsValidators[partitionKey] = (optional, fieldValidator);
            }

            var typeName = partitioning.ToString()!;

            return new AggregateValidator(
                CreateFieldValidators(field)
                    .Union(Enumerable.Repeat(
                        new ObjectValidator<IJsonValue>(fieldsValidators, isPartial, typeName), 1)), log);
        }

        private IValidator CreateFieldValidator(IField field)
        {
            return new FieldValidator(CreateValueValidators(field), field);
        }

        private IEnumerable<IValidator> CreateContentValidators()
        {
            return factories.SelectMany(x => x.CreateContentValidators(context, CreateFieldValidator));
        }

        private IEnumerable<IValidator> CreateValueValidators(IField field)
        {
            return factories.SelectMany(x => x.CreateValueValidators(context, field, CreateFieldValidator));
        }

        private IEnumerable<IValidator> CreateFieldValidators(IField field)
        {
            return factories.SelectMany(x => x.CreateFieldValidators(context, field, CreateFieldValidator));
        }
    }
}
