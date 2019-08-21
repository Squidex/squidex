// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Newtonsoft.Json;
using Squidex.ClientLibrary;

namespace LoadTest
{
    public sealed class TestEntity : SquidexEntityBase<TestEntityData>
    {
    }

    public sealed class TestEntityData
    {
        [JsonConverter(typeof(InvariantConverter))]
        public int Value { get; set; }
    }
}
