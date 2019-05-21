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
    public class CommentaryTypeMapper : IKafkaMessageMapper
    {
        public ISpecificRecord ToAvro(NamedContentData namedContentData)
        {
            var commentaryType = new CommentaryType();
            ContentFieldData idData = null;
            namedContentData.TryGetValue("Id", out idData);
            commentaryType.Id = idData["iv"].ToString();
            ContentFieldData nameData = null;
            namedContentData.TryGetValue("Id", out nameData);
            commentaryType.Name = nameData["iv"].ToString();
            return commentaryType;
        }
    }
}
