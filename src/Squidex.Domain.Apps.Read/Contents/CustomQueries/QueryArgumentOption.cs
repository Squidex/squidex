// ==========================================================================
//  QueryArgumentOption.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Read.Contents.CustomQueries
{
    public sealed class QueryArgumentOption
    {
        public string Name { get; }

        public string Description { get; }

        public QueryArgumentOption(string name, string description)
        {
            Guard.ValidPropertyName(name, nameof(name));
            Guard.NotNullOrEmpty(description, nameof(description));

            Name = name;

            Description = description;
        }
    }
}