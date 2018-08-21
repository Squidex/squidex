// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public class GuidMapperTests
    {
        private readonly Guid id1 = Guid.NewGuid();
        private readonly Guid id2 = Guid.NewGuid();
        private readonly GuidMapper map = new GuidMapper();

        [Fact]
        public void Should_map_guid_string_if_valid()
        {
            var result = map.NewGuidString(id1.ToString());

            Assert.Equal(map.NewGuid(id1).ToString(), result);
        }

        [Fact]
        public void Should_return_null_if_mapping_invalid_guid_string()
        {
            var result = map.NewGuidString("invalid");

            Assert.Null(result);
        }

        [Fact]
        public void Should_return_null_if_mapping_null_guid_string()
        {
            var result = map.NewGuidString(null);

            Assert.Null(result);
        }

        [Fact]
        public void Should_map_guid()
        {
            var result = map.NewGuids(id1);

            Assert.Equal(map.NewGuid(id1), result.Value<Guid>());
        }

        [Fact]
        public void Should_return_old_guid()
        {
            var newGuid = map.NewGuids(id1).Value<Guid>();

            Assert.Equal(id1, map.OldGuid(newGuid));
        }

        [Fact]
        public void Should_map_guid_string()
        {
            var result = map.NewGuids(id1.ToString());

            Assert.Equal(map.NewGuid(id1).ToString(), result.Value<string>());
        }

        [Fact]
        public void Should_map_named_id()
        {
            var result = map.NewGuids($"{id1},name");

            Assert.Equal($"{map.NewGuid(id1)},name", result.Value<string>());
        }

        [Fact]
        public void Should_map_array_with_guid()
        {
            var obj =
                new JObject(
                    new JProperty("k",
                        new JArray(id1, id1, id2)));

            map.NewGuids(obj);

            Assert.Equal(map.NewGuid(id1), obj["k"][0].Value<Guid>());
            Assert.Equal(map.NewGuid(id1), obj["k"][1].Value<Guid>());
            Assert.Equal(map.NewGuid(id2), obj["k"][2].Value<Guid>());
        }

        [Fact]
        public void Should_map_objects_with_guid_keys()
        {
            var obj =
                new JObject(
                    new JProperty("k",
                        new JObject(
                            new JProperty(id1.ToString(), id1),
                            new JProperty(id2.ToString(), id2))));

            map.NewGuids(obj);

            Assert.Equal(map.NewGuid(id1), obj["k"].Value<Guid>(map.NewGuid(id1).ToString()));
            Assert.Equal(map.NewGuid(id2), obj["k"].Value<Guid>(map.NewGuid(id2).ToString()));
        }

        [Fact]
        public void Should_map_objects_with_guid()
        {
            var obj =
                new JObject(
                    new JProperty("k",
                        new JObject(
                            new JProperty("v1", id1),
                            new JProperty("v2", id1),
                            new JProperty("v3", id2))));

            map.NewGuids(obj);

            Assert.Equal(map.NewGuid(id1), obj["k"].Value<Guid>("v1"));
            Assert.Equal(map.NewGuid(id1), obj["k"].Value<Guid>("v2"));
            Assert.Equal(map.NewGuid(id2), obj["k"].Value<Guid>("v3"));
        }

        [Fact]
        public void Should_map_objects_with_guid_string()
        {
            var obj =
                new JObject(
                    new JProperty("k",
                        new JObject(
                            new JProperty("v1", id1.ToString()),
                            new JProperty("v2", id1.ToString()),
                            new JProperty("v3", id2.ToString()))));

            map.NewGuids(obj);

            Assert.Equal(map.NewGuid(id1).ToString(), obj["k"].Value<string>("v1"));
            Assert.Equal(map.NewGuid(id1).ToString(), obj["k"].Value<string>("v2"));
            Assert.Equal(map.NewGuid(id2).ToString(), obj["k"].Value<string>("v3"));
        }

        [Fact]
        public void Should_map_objects_with_named_id()
        {
            var obj =
                new JObject(
                    new JProperty("k",
                        new JObject(
                            new JProperty("v1", $"{id1},v1"),
                            new JProperty("v2", $"{id1},v2"),
                            new JProperty("v3", $"{id2},v3"))));

            map.NewGuids(obj);

            Assert.Equal($"{map.NewGuid(id1).ToString()},v1", obj["k"].Value<string>("v1"));
            Assert.Equal($"{map.NewGuid(id1).ToString()},v2", obj["k"].Value<string>("v2"));
            Assert.Equal($"{map.NewGuid(id2).ToString()},v3", obj["k"].Value<string>("v3"));
        }
    }
}
