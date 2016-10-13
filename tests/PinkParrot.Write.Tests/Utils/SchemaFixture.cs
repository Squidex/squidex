// ==========================================================================
//  SchemaFixture.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using PinkParrot.Core.Schemas;
using PinkParrot.Infrastructure;
using System.Reflection;
using Xunit;

namespace PinkParrot.Write.Tests.Utils
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
