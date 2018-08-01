// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public class GuidMapperTests
    {
        [Fact]
        public void Should_map_guid()
        {
            var m = new Dictionary<Guid, Guid>();

            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var obj =
                new JObject(
                    new JProperty("k",
                        new JObject(
                            new JProperty("v1", id1),
                            new JProperty("v2", id1),
                            new JProperty("v3", id2))));

            GuidMapper.GenerateNewGuid(obj, m);

            Assert.Equal(m[id1], obj["k"].Value<Guid>("v1"));
            Assert.Equal(m[id1], obj["k"].Value<Guid>("v2"));
            Assert.Equal(m[id2], obj["k"].Value<Guid>("v3"));
        }

        [Fact]
        public void Should_map_guid_string()
        {
            var m = new Dictionary<Guid, Guid>();

            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var obj =
                new JObject(
                    new JProperty("k",
                        new JObject(
                            new JProperty("v1", id1.ToString()),
                            new JProperty("v2", id1.ToString()),
                            new JProperty("v3", id2.ToString()))));

            GuidMapper.GenerateNewGuid(obj, m);

            Assert.Equal(m[id1].ToString(), obj["k"].Value<string>("v1"));
            Assert.Equal(m[id1].ToString(), obj["k"].Value<string>("v2"));
            Assert.Equal(m[id2].ToString(), obj["k"].Value<string>("v3"));
        }

        [Fact]
        public void Should_map_named_id()
        {
            var m = new Dictionary<Guid, Guid>();

            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var obj =
                new JObject(
                    new JProperty("k",
                        new JObject(
                            new JProperty("v1", $"{id1},v1"),
                            new JProperty("v2", $"{id1},v2"),
                            new JProperty("v3", $"{id2},v3"))));

            GuidMapper.GenerateNewGuid(obj, m);

            Assert.Equal($"{m[id1].ToString()},v1", obj["k"].Value<string>("v1"));
            Assert.Equal($"{m[id1].ToString()},v2", obj["k"].Value<string>("v2"));
            Assert.Equal($"{m[id2].ToString()},v3", obj["k"].Value<string>("v3"));
        }
    }
}
