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
    private const byte GuidIndicator = 0xff;
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
        var buffer = context.Reader.ReadBytes()!.AsSpan();
        var offset = 0;

        static DomainId ReadId(ReadOnlySpan<byte> buffer, ref int offset)
        {
            DomainId id;

            // If we have reached the end of the buffer then
            if (offset >= buffer.Length)
            {
                return default;
            }

            var length = buffer[offset++];

            // Special length indicator for all guids.
            if (length == GuidIndicator)
            {
                id = DomainId.Create(new Guid(buffer.Slice(offset, GuidLength)));
                offset += GuidLength;
            }
            else
            {
                id = DomainId.Create(Encoding.UTF8.GetString(buffer.Slice(offset, length)));
                offset += length;
            }

            return id;
        }

        return new UniqueContentId(ReadId(buffer, ref offset), ReadId(buffer, ref offset));
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, UniqueContentId value)
    {
        var appIdentity = CheckId(value.AppId);
        var appIdLength = appIdentity.Length + 1;

        var contentIdentity = CheckId(value.ContentId);
        var contentIdLength = contentIdentity.IsEmpty ? 0 : contentIdentity.Length + 1;

        var bufferLength = appIdLength + contentIdLength;
        var bufferArray = new byte[bufferLength];

        Write(bufferArray, appIdentity);

        if (!contentIdentity.IsEmpty)
        {
            // Do not write empty Ids to the buffer to allow prefix searches.
            Write(bufferArray.AsSpan()[appIdLength..], contentIdentity);
        }

        static void Write(Span<byte> buffer, IdInfo id)
        {
            if (id.IsGuid)
            {
                // Special length indicator for all guids.
                buffer[0] = GuidIndicator;

                id.AsGuid.TryWriteBytes(buffer[1..]);
            }
            else
            {
                // We assume that we use relatively small IDs, not longer than 254 bytes.
                buffer[0] = id.Length;

                Encoding.UTF8.GetBytes(id.Source, buffer[1..]);
            }
        }

        context.Writer.WriteBytes(bufferArray);
    }

    private static IdInfo CheckId(DomainId id)
    {
        var source = id.ToString();

        if (Guid.TryParse(source, out var guid))
        {
            return new IdInfo(GuidLength, true, guid, source);
        }

        var idLength = (byte)Encoding.UTF8.GetByteCount(source);

        if (idLength > 254)
        {
            // We only use a single byte to write the length, therefore we do not allow large strings.
            ThrowHelper.InvalidOperationException("Cannot write long IDs.");
        }

        return new IdInfo(idLength, false, default, source);
    }

#pragma warning disable
    record struct IdInfo(byte Length, bool IsGuid, Guid AsGuid, string Source)
    {
        public bool IsEmpty => IsGuid && AsGuid == default;
    }
#pragma warning restore
}
