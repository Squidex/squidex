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
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.ValidateContent.Validators;
using Squidex.Infrastructure;

#pragma warning disable SA1028, IDE0004 // Code must not contain trailing whitespace

namespace Squidex.Domain.Apps.Core.ValidateContent
{
    public sealed class ContentValidator
    {
        private static readonly ContentFieldData DefaultFieldData = new ContentFieldData();
        private static readonly JToken DefaultValue = JValue.CreateNull();
        private readonly Schema schema;
        private readonly PartitionResolver partitionResolver;
        private readonly ValidationContext context;
        private readonly ConcurrentBag<ValidationError> errors = new ConcurrentBag<ValidationError>();

        public IReadOnlyCollection<ValidationError> Errors
        {
            get { return errors; }
        }

        public ContentValidator(Schema schema, PartitionResolver partitionResolver, ValidationContext context)
        {
            Guard.NotNull(schema, nameof(schema));
            Guard.NotNull(partitionResolver, nameof(partitionResolver));

            this.schema = schema;
            this.context = context;
            this.partitionResolver = partitionResolver;
        }

        private void AddError(IEnumerable<string> path, string message)
        {
            var pathString = path.ToPathString();

            errors.Add(new ValidationError($"{pathString}: {message}", pathString));
        }

        public Task ValidatePartialAsync(NamedContentData data)
        {
            Guard.NotNull(data, nameof(data));

            var validator = CreateSchemaValidator(true);

            return validator.ValidateAsync(data, context, AddError);
        }

        public Task ValidateAsync(NamedContentData data)
        {
            Guard.NotNull(data, nameof(data));

            var validator = CreateSchemaValidator(false);

            return validator.ValidateAsync(data, context, AddError);
        }

        private IValidator CreateSchemaValidator(bool isPartial)
        {
            var fieldsValidators = new Dictionary<string, (bool IsOptional, IValidator Validator)>();

            foreach (var field in schema.FieldsByName)
            {
                fieldsValidators[field.Key] = (!field.Value.RawProperties.IsRequired, CreateFieldValidator(field.Value, isPartial));
            }

            return new ObjectValidator<ContentFieldData>(fieldsValidators, isPartial, "field", DefaultFieldData);
        }

        private IValidator CreateFieldValidator(IRootField field, bool isPartial)
        {
            var partitioning = partitionResolver(field.Partitioning);

            var fieldValidator = new FieldValidator(ValidatorsFactory.CreateValidators(field).ToArray(), field);
            var fieldsValidators = new Dictionary<string, (bool IsOptional, IValidator Validator)>();

            foreach (var partition in partitioning)
            {
                fieldsValidators[partition.Key] = (partition.IsOptional, fieldValidator);
            }

            var isLanguage = field.Partitioning.Equals(Partitioning.Language);

            var type = isLanguage ? "language" : "invariant value";

            return new ObjectValidator<JToken>(fieldsValidators, isPartial, type, DefaultValue);
        }
    }
}
