// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.MongoDb
{
    public sealed class MongoDbOptions
    {
        public MongoDbEngine Engine { get; set; } = MongoDbEngine.MongoDb;

        public bool IsDocumentDb
        {
            get { return Engine == MongoDbEngine.DocumentDb; }
        }

        public bool IsMongoDb
        {
            get { return Engine == MongoDbEngine.MongoDb; }
        }
    }
}
