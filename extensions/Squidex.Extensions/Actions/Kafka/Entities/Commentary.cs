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
        public static readonly Schema SCHEMA = Schema.Parse(@"
            {
                ""type"": ""record"",
                ""name"": ""Commentary"",
                ""namespace"": ""Cosmos.Kafka.Entities"",
                ""fields"": [
                    {""name"": ""id"", ""type"": ""string""},
                    {""name"": ""commentaryType"", ""type"": 
                        {
                            ""type"": ""record"", 
                            ""name"": ""commentaryType"",
                            ""fields"": [
                                {""name"": ""id"", ""type"": ""string""},
                                {""name"": ""name"", ""type"": ""string""}
                            ]
                        }
                    },
                    {""name"": ""body"", ""type"": [""string"", ""null""]}
                ]
            }");

        public virtual Schema Schema => SCHEMA;

        public string Id { get; set; }
        public string Body { get; set; }
        public CommentaryType CommentaryType { get; set; }

        public virtual object Get(int fieldPos)
        {
            switch (fieldPos)
            {
                case 0:
                    return Id;
                case 1:
                    return Body;
                case 2:
                    return CommentaryType;
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
                    CommentaryType = (CommentaryType)fieldValue;
                    break;
                case 2:
                    Body = (string)fieldValue;
                    break;
                default: throw new AvroRuntimeException("Bad index " + fieldPos + " in Put()");
            }
        }
    }
}
