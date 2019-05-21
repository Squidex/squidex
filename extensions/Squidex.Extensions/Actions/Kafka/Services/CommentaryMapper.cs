// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Avro.Specific;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Extensions.Actions.Kafka.Entities;

namespace Squidex.Extensions.Actions.Kafka
{
    public class CommentaryMapper : IKafkaMessageMapper
    {
        public ISpecificRecord ToAvro(NamedContentData namedContentData)
        {
            var commentary = new Commentary();
            commentary.Id = "1";
            commentary.Body = "test";
            commentary.CommentaryType = new CommentaryType() { Id = "1", Name = "Overview" };
            return commentary;
        }
    }
}
