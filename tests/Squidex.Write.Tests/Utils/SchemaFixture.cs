// ==========================================================================
//  SchemaFixture.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Reflection;
using Squidex.Core.Schemas;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Write.Utils
{
    public class SchemaFixture
    {
        public SchemaFixture()
        {
            TypeNameRegistry.Map(typeof(Schema).GetTypeInfo().Assembly);
        }
    }

    [CollectionDefinition("Schema")]
    public class DatabaseCollection : ICollectionFixture<SchemaFixture>
    {
    }
}
