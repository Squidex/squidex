// ==========================================================================
//  ContentDataTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Squidex.Core.Contents
{
    public class ContentDataTests
    {
        [Fact]
        public void Should_convert_from_dictionary()
        {
            var input =
                new Dictionary<string, Dictionary<string, JToken>>
                {
                    ["field1"] = new Dictionary<string, JToken>
                    {
                        ["en"] = "en_string",
                        ["de"] = "de_string"
                    },
                    ["field2"] = new Dictionary<string, JToken>
                    {
                        ["en"] = 1,
                        ["de"] = 2
                    }
                };

            var actual = ContentData.Create(input);

            var expected =
                ContentData.Empty
                    .AddField("field1",
                        ContentFieldData.Empty
                            .AddValue("en", "en_string")
                            .AddValue("de", "de_string"))
                    .AddField("field2",
                        ContentFieldData.Empty
                            .AddValue("en", 1)
                            .AddValue("de", 2));

            var output = actual.ToRaw();

            actual.ShouldBeEquivalentTo(expected);
            output.ShouldBeEquivalentTo(input);
        }
    }
}
