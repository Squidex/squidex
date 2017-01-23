// ==========================================================================
//  ContentFieldData.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Squidex.Infrastructure;

namespace Squidex.Core.Contents
{
    public sealed class ContentFieldData : Dictionary<string, JToken>
    {
        public ContentFieldData()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public ContentFieldData SetValue(JToken value)
        {
            this["iv"] = value;

            return this;
        }

        public ContentFieldData AddValue(string language, JToken value)
        {
            Guard.NotNullOrEmpty(language, nameof(language));

            this[language] = value;

            return this;
        }
    }
}