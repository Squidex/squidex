// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.MongoDb;

public partial class BsonUniqueContentIdSerializer : SerializerBase<UniqueContentId>
{
    private const byte LongContentIdIndicator = byte.MaxValue - 1;
    private const byte GuidLength = 16;
    private const byte GuidIndicator = byte.MaxValue;
    private const byte SizeOfInt = 4;
    private const byte SizeOfByte = 1;
    private static readonly BsonUniqueContentIdSerializer Instance = new BsonUniqueContentIdSerializer();

    public static void Register()
    {
        BsonSerializer.TryRegisterSerializer(Instance);
    }

    public static UniqueContentId NextAppId(DomainId appId)
    {
        static void IncrementByteArray(byte[] bytes)
        {
            for (var i = 0; i < bytes.Length; i++)
            {
                var value = bytes[i];
                if (value < byte.MaxValue)
                {
                    value += 1;
                    bytes[i] = value;
                    break;
                }
            }
        }

        if (Guid.TryParse(appId.ToString(), out var id))
        {
            var bytes = id.ToByteArray();

            IncrementByteArray(bytes);

            return new UniqueContentId(DomainId.Create(new Guid(bytes)), DomainId.Empty);
        }
        else
        {
            var bytes = Encoding.UTF8.GetBytes(appId.ToString());

            IncrementByteArray(bytes);

            return new UniqueContentId(DomainId.Create(Encoding.UTF8.GetString(bytes)), DomainId.Empty);
        }
    }

    public override UniqueContentId Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var buffer = context.Reader.ReadBytes()!.AsSpan();

        var isLongContentId = buffer[0] == LongContentIdIndicator;
        if (isLongContentId)
        {
            // Skip over the length indicator.
            buffer = buffer[1..];
        }

        var (appId, read) = IdInfo.ReadWithByteLength(buffer);

        if (isLongContentId)
        {
            return new UniqueContentId(appId, IdInfo.ReadWithIntLength(buffer[read..]).Id);
        }
        else
        {
            return new UniqueContentId(appId, IdInfo.ReadWithByteLength(buffer[read..]).Id);
        }
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, UniqueContentId value)
    {
        var appId = IdInfo.Create(value.AppId);

        if (appId.Length >= LongContentIdIndicator)
        {
            ThrowHelper.InvalidOperationException("App ID cannot be longer than 253 bytes.");
        }

        var contentId = IdInfo.Create(value.ContentId);

        if (contentId.Length >= LongContentIdIndicator)
        {
            WriteV2(context.Writer, appId, contentId);
        }
        else
        {
            WriteV1(context.Writer, appId, contentId);
        }
    }

    private static void WriteV2(IBsonWriter writer, IdInfo appId, IdInfo contentId)
    {
        var size = SizeOfByte + appId.SizeWithByteLength(true) + contentId.SizeWithIntLength(false);

        var bufferArray = new byte[size];
        var bufferSpan = bufferArray.AsSpan();

        bufferSpan[0] = LongContentIdIndicator;
        bufferSpan = bufferSpan[SizeOfByte..];

        var written = appId.WriteWithByteLength(bufferSpan);

        if (!contentId.IsEmpty)
        {
            // Do not write empty Ids to the buffer to allow prefix searches.
            contentId.WriteWithIntLength(bufferSpan[written..]);
        }

        writer.WriteBytes(bufferArray);
    }

    private static void WriteV1(IBsonWriter writer, IdInfo appId, IdInfo contentId)
    {
        var size = appId.SizeWithByteLength(true) + contentId.SizeWithByteLength(false);

        var bufferArray = new byte[size];
        var bufferSpan = bufferArray.AsSpan();

        var written = appId.WriteWithByteLength(bufferSpan);

        if (!contentId.IsEmpty)
        {
            // Do not write empty Ids to the buffer to allow prefix searches.
            contentId.WriteWithByteLength(bufferSpan[written..]);
        }

        writer.WriteBytes(bufferArray);
    }
}
