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
    public class CommentaryType : ISpecificRecord
    {
        public static readonly Schema SCHEMA = Schema.Parse(@"
            {
                ""type"": ""record"",
                ""name"": ""CommentaryType"",
                ""namespace"": ""Cosmos.Kafka.Entities"",
                ""fields"": [
                    {""name"": ""id"", ""type"": ""string""},
                    {""name"": ""name"", ""type"": ""string""}
                ]
            }");

        public virtual Schema Schema => SCHEMA;

        public string Id { get; set; }
        public string Name { get; set; }

        public virtual object Get(int fieldPos)
        {
            switch (fieldPos)
            {
                case 0:
                    return Id;
                case 1:
                    return Name;
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
                    Name = (string)fieldValue;
                    break;
                default: throw new AvroRuntimeException("Bad index " + fieldPos + " in Put()");
            }
        }
    }
}
