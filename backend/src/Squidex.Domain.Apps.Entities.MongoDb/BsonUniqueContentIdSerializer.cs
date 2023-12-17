// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.MongoDb;

public sealed class BsonUniqueContentIdSerializer : SerializerBase<UniqueContentId>
{
    private const byte GuidLength = 16;
    private static readonly BsonUniqueContentIdSerializer Instance = new BsonUniqueContentIdSerializer();

    public static void Register()
    {
        BsonSerializer.TryRegisterSerializer(Instance);
    }

    private BsonUniqueContentIdSerializer()
    {
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
        var buffer = context.Reader.ReadBytes()!;
        var offset = 0;

        static DomainId ReadId(byte[] buffer, ref int offset)
        {
            DomainId id;

            // If we have reached the end of the buffer then
            if (offset >= buffer.Length)
            {
                return default;
            }

            var length = buffer[offset++];
            // Special length indicator for all guids.
            if (length == 0xFF)
            {
                id = DomainId.Create(new Guid(buffer.AsSpan(offset, GuidLength)));
                offset += GuidLength;
            }
            else
            {
                id = DomainId.Create(Encoding.UTF8.GetString(buffer.AsSpan(offset, length)));
                offset += length;
            }

            return id;
        }

        return new UniqueContentId(ReadId(buffer, ref offset), ReadId(buffer, ref offset));
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, UniqueContentId value)
    {
        var appId = CheckId(value.AppId);

        var contentId = CheckId(value.ContentId);

        var isEmptyContentId =
            contentId.IsGuid &&
            contentId.Guid == default;

        // Do not write empty Ids to the buffer to allow prefix searches.
        var contentLength = !isEmptyContentId ? contentId.Length + 1 : 0;

        var bufferLength = appId.Length + 1 + contentLength;
        var bufferArray = new byte[bufferLength];

        var offset = Write(bufferArray, 0,
            appId.IsGuid,
            appId.Guid,
            appId.Source,
            appId.Length);

        if (!isEmptyContentId)
        {
            // Do not write the empty content id, so we can search for app as well.
            Write(bufferArray, offset,
                contentId.IsGuid,
                contentId.Guid,
                contentId.Source,
                contentId.Length);
        }

        static int Write(byte[] buffer, int offset, bool isGuid, Guid guid, string id, int idLength)
        {
            if (isGuid)
            {
                // Special length indicator for all guids.
                buffer[offset++] = 0xFF;
                WriteGuid(buffer.AsSpan(offset), guid);

                return offset + GuidLength;
            }
            else
            {
                // We assume that we use relatively small IDs, not longer than 254 bytes.
                buffer[offset++] = (byte)idLength;
                WriteString(buffer.AsSpan(offset), id);

                return offset + idLength;
            }
        }

        context.Writer.WriteBytes(bufferArray);
    }

    private static (int Length, bool IsGuid, Guid Guid, string Source) CheckId(DomainId id)
    {
        var source = id.ToString();

        var idIsGuid = Guid.TryParse(source, out var idGuid);
        var idLength = GuidLength;

        if (!idIsGuid)
        {
            idLength = (byte)Encoding.UTF8.GetByteCount(source);

            // We only use a single byte to write the length, therefore we do not allow large strings.
            if (idLength > 254)
            {
                ThrowHelper.InvalidOperationException("Cannot write long IDs.");
            }
        }

        return (idLength, idIsGuid, idGuid, source);
    }

    private static void WriteString(Span<byte> span, string id)
    {
        Encoding.UTF8.GetBytes(id, span);
    }

    private static void WriteGuid(Span<byte> span, Guid guid)
    {
        guid.TryWriteBytes(span);
    }
}
