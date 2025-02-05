// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.MongoDb;

public partial class BsonUniqueContentIdSerializer : SerializerBase<UniqueContentId>
{
    private static readonly BsonUniqueContentIdSerializer Instance = new BsonUniqueContentIdSerializer();

    public static void Register()
    {
        BsonSerializer.TryRegisterSerializer(Instance);
    }

    public override UniqueContentId Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var buffer = context.Reader.ReadBytes()!.AsSpan();

        var (appId, read) = IdInfo.Read(buffer);

        return new UniqueContentId(appId, IdInfo.Read(buffer[read..]).Id);
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, UniqueContentId value)
    {
        var appId = IdInfo.Create(value.AppId);
        if (appId.Length >= IdInfo.LongIdIndicator)
        {
            ThrowHelper.InvalidOperationException("App ID cannot be longer than 253 bytes.");
        }

        var contentId = IdInfo.Create(value.ContentId);

        var size = appId.Size(true) + contentId.Size(false);

        var bufferArray = new byte[size];
        var bufferSpan = bufferArray.AsSpan();

        var written = appId.Write(bufferSpan);

        if (!contentId.IsEmpty)
        {
            // Do not write empty Ids to the buffer to allow prefix searches.
            contentId.Write(bufferSpan[written..]);
        }

        context.Writer.WriteBytes(bufferArray);
    }
}
