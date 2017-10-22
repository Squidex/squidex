// ==========================================================================
//  BsonJsonAttribute.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.MongoDb
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class BsonJsonAttribute : Attribute
    {
    }
}
