// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.MongoDb;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
#pragma warning disable RECS0082 // Parameter has the same name as a member and hides it

public partial class BsonUniqueContentIdSerializer
{
    private readonly record struct IdInfo(int Length, bool IsGuid, Guid AsGuid, string Source)
    {
        public bool IsEmpty => IsGuid && AsGuid == default;

        public static IdInfo Create(DomainId id)
        {
            var source = id.ToString();

            if (Guid.TryParse(source, out var guid))
            {
                return new IdInfo(GuidLength, true, guid, source);
            }

            return new IdInfo(Encoding.UTF8.GetByteCount(source), false, default, source);
        }

        public int SizeWithIntLength(bool writeEmpty)
        {
            return Size(writeEmpty, SizeOfInt);
        }

        public int SizeWithByteLength(bool writeEmpty)
        {
            return Size(writeEmpty, SizeOfByte);
        }

        private int Size(bool writeEmpty, int lengthSize)
        {
            if (IsEmpty && !writeEmpty)
            {
                return 0;
            }

            return lengthSize + Length;
        }

        public int WriteWithIntLength(Span<byte> buffer)
        {
            return Write(buffer, WriteLengthAsInt);
        }

        public int WriteWithByteLength(Span<byte> buffer)
        {
            return Write(buffer, WriteLengthAsByte);
        }

        private int Write(Span<byte> buffer, WriteLength writeLength)
        {
            int lengthSize;
            if (IsGuid)
            {
                // Special length indicator for all guids.
                lengthSize = writeLength(buffer, GuidIndicator);

                AsGuid.TryWriteBytes(buffer[lengthSize..]);
            }
            else
            {
                // We assume that we use relatively small IDs, not longer than 253 bytes.
                lengthSize = writeLength(buffer, Length);

                Encoding.UTF8.GetBytes(Source, buffer[lengthSize..]);
            }

            return lengthSize + Length;
        }

        public static (DomainId Id, int Length) ReadWithIntLength(ReadOnlySpan<byte> buffer)
        {
            return Read(buffer, ReadLengthAsInt);
        }

        public static (DomainId Id, int Length) ReadWithByteLength(ReadOnlySpan<byte> buffer)
        {
            return Read(buffer, ReadLengthAsByte);
        }

        private static (DomainId Id, int Length) Read(ReadOnlySpan<byte> buffer, ReadLength readLength)
        {
            // If we have reached the end of the buffer then there is no ID.
            if (buffer.Length == 0)
            {
                return default;
            }

            var (length, offset) = readLength(buffer);

            if (length == GuidIndicator)
            {
                length = GuidLength;
            }

            // Advance by the size of the prefix offset and the length.
            buffer = buffer.Slice(offset, length);

            var id = length == GuidLength ?
                DomainId.Create(new Guid(buffer)) :
                DomainId.Create(Encoding.UTF8.GetString(buffer));

            return (id, offset + length);
        }

        private static int WriteLengthAsByte(Span<byte> buffer, int length)
        {
            buffer[0] = (byte)length;

            return SizeOfByte;
        }

        private static int WriteLengthAsInt(Span<byte> buffer, int length)
        {
            BitConverter.TryWriteBytes(buffer, length);

            return SizeOfInt;
        }

        private static (int, int) ReadLengthAsByte(ReadOnlySpan<byte> buffer)
        {
            return (buffer[0], SizeOfByte);
        }

        private static (int, int) ReadLengthAsInt(ReadOnlySpan<byte> buffer)
        {
            return (BitConverter.ToInt32(buffer), SizeOfInt);
        }

        private delegate int WriteLength(Span<byte> buffer, int length);

        private delegate (int, int) ReadLength(ReadOnlySpan<byte> buffer);
    }
#pragma warning restore
}
