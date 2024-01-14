// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Cryptography;
using System.Text;

namespace Squidex.Infrastructure;

public class HashExtensionsTests
{
    public static readonly TheoryData<int> InputLengths =
        new TheoryData<int>(1, 20, 50, 500);

    [Fact]
    public void Should_calculate_hex_code_from_empty_array()
    {
        var source = Array.Empty<byte>();

        var actual = ((ReadOnlySpan<byte>)source.AsSpan()).ToHexString();

        Assert.Equal(string.Empty, actual);
    }

    [Fact]
    public void Should_calculate_hex_code_from_byte_array()
    {
        var source = new byte[] { 0x00, 0x01, 0xFF, 0x1A, 0x2B, 0x3C };

        var actual = ((ReadOnlySpan<byte>)source.AsSpan()).ToHexString();

        Assert.Equal("0001FF1A2B3C", actual);
    }

    [Theory]
    [MemberData(nameof(InputLengths))]
    public void Should_hash_md5(int length)
    {
        AssertHash(HashExtensions.ToMD5, length);
    }

    [Theory]
    [MemberData(nameof(InputLengths))]
    public void Should_hash_md5_from_bytes(int length)
    {
        AssertHashFromBytes(HashExtensions.ToMD5, length);
    }

    [Theory]
    [MemberData(nameof(InputLengths))]
    public void Should_hash_sha256(int length)
    {
        AssertHash(HashExtensions.ToSha256, length);
    }

    [Theory]
    [MemberData(nameof(InputLengths))]
    public void Should_hash_sha256_from_bytes(int length)
    {
        AssertHashFromBytes(HashExtensions.ToSha256, length);
    }

    [Theory]
    [MemberData(nameof(InputLengths))]
    public void Should_hash_sha512(int length)
    {
        AssertHash(HashExtensions.ToSha512, length);
    }

    [Theory]
    [MemberData(nameof(InputLengths))]
    public void Should_hash_sha512_from_bytes(int length)
    {
        AssertHashFromBytes(HashExtensions.ToSha512, length);
    }

    [Theory]
    [MemberData(nameof(InputLengths))]
    public void Should_hash_to_sha256_base64(int length)
    {
        AssertHash(HashExtensions.ToSha256Base64, length);
    }

    [Theory]
    [MemberData(nameof(InputLengths))]
    public void Should_hash_sha256_base64_from_bytes(int length)
    {
        AssertHashFromBytes(HashExtensions.ToSha256Base64, length);
    }

    [Fact]
    public void Should_convert_same_hash_as_webhook_utils()
    {
        var input = $"{Guid.NewGuid()}{new string('0', 100)}{Guid.NewGuid()}";

        var hash1 = WebhookHash(input);
        var hash2 = input.ToSha256Base64();

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void Should_calculate_incremental_hash()
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.MD5);

        hash.AppendString($"{Guid.Empty}-{Guid.Empty}");
        hash.AppendString($"{Guid.Empty}-{Guid.Empty}-{Guid.Empty}-{Guid.Empty}");
        hash.AppendLong(42);

        var result = hash.GetHexStringAndReset();

        Assert.Equal("62D6F977A8C2B79983A489AA5932C512", result);
    }

    [Fact]
    public void Should_calculate_incremental_quoted_hash()
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.MD5);

        hash.AppendString($"{Guid.Empty}-{Guid.Empty}");
        hash.AppendString($"{Guid.Empty}-{Guid.Empty}-{Guid.Empty}-{Guid.Empty}");
        hash.AppendLong(42);

        var result = hash.GetQuotedHexStringAndReset();

        Assert.Equal("\"62D6F977A8C2B79983A489AA5932C512\"", result);
    }

    private delegate string HashFromBytes(ReadOnlySpan<byte> source);

    private static void AssertHashFromBytes(HashFromBytes hasher, int length)
    {
        var input1 = CreateBuffer(length, 0);
        var input2 = CreateBuffer(length, 42);

        var hash1_a = hasher(input1);
        var hash1_b = hasher(input1);

        var hash2 = hasher(input2);

        Assert.NotNull(hash1_a);
        Assert.NotNull(hash1_a);
        Assert.NotNull(hash2);

        Assert.NotEmpty(hash1_a);
        Assert.NotEmpty(hash1_b);
        Assert.NotEmpty(hash2);

        Assert.Equal(hash1_a, hash1_b);

        Assert.NotEqual(hash1_a, hash2);
    }

    private static void AssertHash(Func<string, string> hasher, int length)
    {
        var input1 = new string('a', length);
        var input2 = new string('b', length);

        var hash1_a = hasher(input1);
        var hash1_b = hasher(input1);

        var hash2 = hasher(input2);

        Assert.NotNull(hash1_a);
        Assert.NotNull(hash1_a);
        Assert.NotNull(hash2);

        Assert.NotEmpty(hash1_a);
        Assert.NotEmpty(hash1_b);
        Assert.NotEmpty(hash2);

        Assert.Equal(hash1_a, hash1_b);

        Assert.NotEqual(hash1_a, hash2);
    }

    private static byte[] CreateBuffer(int length, int offset)
    {
        var input = new byte[length];

        for (var i = 0; i < length; i++)
        {
            input[i] = (byte)(i + offset);
        }

        return input;
    }

    private static string WebhookHash(string value)
    {
        var bytesArray = Encoding.UTF8.GetBytes(value);
        var bytesHash = SHA256.HashData(bytesArray);

        var result = Convert.ToBase64String(bytesHash);

        return result;
    }
}
