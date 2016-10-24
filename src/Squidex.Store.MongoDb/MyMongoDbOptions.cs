// ==========================================================================
//  MyMongoDbOptions.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================
namespace PinkParrot.Store.MongoDb
{
    public class MyMongoDbOptions
    {
        public string ConnectionString { get; set; }

        public string DatabaseName { get; set; }
    }
}
