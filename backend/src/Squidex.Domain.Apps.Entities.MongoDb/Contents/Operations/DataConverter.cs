// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations
{
    public sealed class DataConverter
    {
        private readonly FieldConverter[] decodeJsonConverters;
        private readonly FieldConverter[] encodeJsonConverters;

        public DataConverter(IJsonSerializer serializer)
        {
            var decoder = ValueConverters.DecodeJson(serializer);

            decodeJsonConverters = new[]
            {
                FieldConverters.ForValues(decoder, ValueConverters.ForNested(decoder))
            };

            var encoder = ValueConverters.EncodeJson(serializer);

            encodeJsonConverters = new[]
            {
                FieldConverters.ForValues(encoder, ValueConverters.ForNested(encoder))
            };
        }

        public NamedContentData FromMongoModel(IdContentData result, Schema schema)
        {
            return result.ConvertId2Name(schema, decodeJsonConverters);
        }

        public IdContentData ToMongoModel(NamedContentData result, Schema schema)
        {
            return result.ConvertName2IdCloned(schema, encodeJsonConverters);
        }
    }
}
