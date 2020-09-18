// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Immutable;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.ValidateContent
{
    public sealed class ValidationContext
    {
        public ImmutableQueue<string> Path { get; }

        public NamedId<DomainId> AppId { get; }

        public NamedId<DomainId> SchemaId { get; }

        public Schema Schema { get; }

        public DomainId ContentId { get; }

        public bool IsOptional { get; }

        public ValidationMode Mode { get; }

        public ValidationContext(
            NamedId<DomainId> appId,
            NamedId<DomainId> schemaId,
            Schema schema,
            DomainId contentId,
            ValidationMode mode = ValidationMode.Default)
            : this(appId, schemaId, schema, contentId, ImmutableQueue<string>.Empty, false, mode)
        {
        }

        private ValidationContext(
            NamedId<DomainId> appId,
            NamedId<DomainId> schemaId,
            Schema schema,
            DomainId contentId,
            ImmutableQueue<string> path,
            bool isOptional,
            ValidationMode mode = ValidationMode.Default)
        {
            AppId = appId;

            ContentId = contentId;

            Mode = mode;

            Schema = schema;
            SchemaId = schemaId;

            IsOptional = isOptional;

            Path = path;
        }

        public ValidationContext Optimized(bool isOptimized = true)
        {
            var mode = isOptimized ? ValidationMode.Optimized : ValidationMode.Default;

            if (Mode == mode)
            {
                return this;
            }

            return Clone(Path, IsOptional, mode);
        }

        public ValidationContext Optional(bool fieldIsOptional)
        {
            if (IsOptional == fieldIsOptional)
            {
                return this;
            }

            return Clone(Path, fieldIsOptional, Mode);
        }

        public ValidationContext Nested(string property)
        {
            return Clone(Path.Enqueue(property), IsOptional, Mode);
        }

        private ValidationContext Clone(ImmutableQueue<string> path, bool isOptional, ValidationMode mode)
        {
            return new ValidationContext(AppId, SchemaId, Schema, ContentId, path, isOptional, mode);
        }
    }
}
