// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Immutable;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Core.ValidateContent
{
    public sealed class ValidationContext : ValidatorContext
    {
        public ImmutableQueue<string> Path { get; private set; } = ImmutableQueue<string>.Empty;

        public IJsonSerializer JsonSerializer { get; }

        public ResolvedComponents Components { get; }

        public DomainId ContentId { get; }

        public bool IsOptional { get; private set; }

        public ValidationContext(
            IJsonSerializer jsonSerializer,
            NamedId<DomainId> appId,
            NamedId<DomainId> schemaId,
            Schema schema,
            ResolvedComponents components,
            DomainId contentId)
            : base(appId, schemaId, schema)
        {
            JsonSerializer = jsonSerializer;

            Components = components;
            ContentId = contentId;
        }

        public ValidationContext Optimized(bool optimized = true)
        {
            return WithMode(optimized ? ValidationMode.Optimized : ValidationMode.Default);
        }

        public ValidationContext AsPublishing(bool publish = true)
        {
            return WithAction(publish ? ValidationAction.Publish : ValidationAction.Upsert);
        }

        public ValidationContext Optional(bool isOptional = true)
        {
            if (IsOptional == isOptional)
            {
                return this;
            }

            return Clone(clone => clone.IsOptional = isOptional);
        }

        public ValidationContext WithAction(ValidationAction action)
        {
            if (Action == action)
            {
                return this;
            }

            return Clone(clone => clone.Action = action);
        }

        public ValidationContext WithMode(ValidationMode mode)
        {
            if (Mode == mode)
            {
                return this;
            }

            return Clone(clone => clone.Mode = mode);
        }

        public ValidationContext Nested(string property)
        {
            return Clone(clone => clone.Path = clone.Path.Enqueue(property));
        }

        private ValidationContext Clone(Action<ValidationContext> updater)
        {
            var clone = (ValidationContext)MemberwiseClone();

            updater(clone);

            return clone;
        }
    }
}
