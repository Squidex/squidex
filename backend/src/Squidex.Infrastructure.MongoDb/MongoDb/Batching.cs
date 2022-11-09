// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;

namespace Squidex.Infrastructure.MongoDb;

public static class Batching
{
    public static readonly FindOptions Options = new FindOptions
    {
        BatchSize = 200
    };
}
