// ==========================================================================
//  ContentFieldData.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Immutable;
using Newtonsoft.Json.Linq;
using Squidex.Infrastructure;

namespace Squidex.Core.Contents
{
    public sealed class ContentFieldData
    {
        private readonly ImmutableDictionary<string, JToken> valueByLanguage;

        public ImmutableDictionary<string, JToken> ValueByLanguage
        {
            get { return valueByLanguage; }
        }

        public ContentFieldData(ImmutableDictionary<string, JToken> valueByLanguage)
        {
            Guard.NotNull(valueByLanguage, nameof(valueByLanguage));

            this.valueByLanguage = valueByLanguage;
        }

        public static ContentFieldData New()
        {
            return new ContentFieldData(ImmutableDictionary<string, JToken>.Empty.WithComparers(StringComparer.OrdinalIgnoreCase));
        }

        public ContentFieldData AddValue(JToken value)
        {
            return new ContentFieldData(valueByLanguage.Add("iv", value));
        }

        public ContentFieldData AddValue(string language, JToken value)
        {
            Guard.NotNullOrEmpty(language, nameof(language));

            return new ContentFieldData(valueByLanguage.Add(language, value));
        }
    }
}