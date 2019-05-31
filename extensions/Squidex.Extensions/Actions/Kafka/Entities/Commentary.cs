// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Avro;
using Avro.Specific;

namespace Squidex.Extensions.Actions.Kafka.Entities
{
    public class Commentary : ISpecificRecord
    {
        // Please do not rename _SCHEMA variable. AvroSerializer class in Avro assembly looks for this particular property name.
        public static readonly Schema _SCHEMA = Schema.Parse(@"
            {
                ""type"": ""record"",
                ""name"": ""Commentary"",
                ""namespace"": ""Cosmos.Kafka.Entities"",
                ""fields"": [
                    {""name"": ""id"", ""type"": ""string""},
                    {""name"": ""commmentaryTypeId"", ""type"": ""string""},
                    {""name"": ""commodityId"", ""type"": ""string""},
                    {""name"": ""body"", ""type"": [""string"", ""null""]}
                ]
            }");

        public virtual Schema Schema => _SCHEMA;

        public string Id { get; set; }
        public string Body { get; set; }
        public string CommentaryTypeId { get; set; }
        public string CommodityId { get; set; }

        public virtual object Get(int fieldPos)
        {
            switch (fieldPos)
            {
                case 0:
                    return Id;
                case 1:
                    return CommentaryTypeId;
                case 2:
                    return CommodityId;
                case 3:
                    return Body;
                default:
                    throw new AvroRuntimeException("Bad index " + fieldPos + " in Get()");
            }
        }

        public virtual void Put(int fieldPos, object fieldValue)
        {
            switch (fieldPos)
            {
                case 0:
                    Id = (string)fieldValue;
                    break;
                case 1:
                    CommentaryTypeId = (string)fieldValue;
                    break;
                case 2:
                    CommodityId = (string)fieldValue;
                    break;
                case 3:
                    Body = (string)fieldValue;
                    break;
                default: throw new AvroRuntimeException("Bad index " + fieldPos + " in Put()");
            }
        }
    }
}
