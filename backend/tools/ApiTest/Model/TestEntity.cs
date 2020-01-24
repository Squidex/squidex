// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Newtonsoft.Json;
using Squidex.ClientLibrary;

namespace ApiTest.Model
{
    public sealed class TestEntity : SquidexEntityBase<TestEntityData>
    {
    }

    public sealed class TestEntityData
    {
        [JsonConverter(typeof(InvariantConverter))]
        public int Number { get; set; }

        [JsonConverter(typeof(InvariantConverter))]
        public string String { get; set; }
    }
}
