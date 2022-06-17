// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Immutable;

namespace Squidex.Domain.Apps.Core.ValidateContent
{
    public sealed record ValidationContext
    {
        public ImmutableQueue<string> Path { get; private set; } = ImmutableQueue<string>.Empty;

        public bool IsOptional { get; init; }

        public RootContext Root { get; }

        public ValidationMode Mode { get; init; }

        public ValidationAction Action { get; init; }

        public ValidationContext(RootContext rootContext)
        {
            Root = rootContext;
        }

        public void AddError(IEnumerable<string> path, string message)
        {
            Root.AddError(path, message);
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

            return this with { IsOptional = isOptional };
        }

        public ValidationContext WithAction(ValidationAction action)
        {
            if (Action == action)
            {
                return this;
            }

            return this with { Action = action };
        }

        public ValidationContext WithMode(ValidationMode mode)
        {
            if (Mode == mode)
            {
                return this;
            }

            return this with { Mode = mode };
        }

        public ValidationContext Nested(string property)
        {
            return this with { Path = Path.Enqueue(property) };
        }

        public ValidationContext Nested(string property, bool isOptional)
        {
            return this with { Path = Path.Enqueue(property), IsOptional = isOptional };
        }
    }
}
